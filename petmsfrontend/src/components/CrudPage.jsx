import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Col, Form, Modal, Row, Spinner, Table } from "react-bootstrap";
import api from "../api";
import { getError, serverFieldErrors, validateForm } from "../../utils";
import SearchBar from "./SearchBar";
import ConfirmModal from "./ConfirmModal";

/**
 * A reusable list + search + add/edit modal + delete page, driven by configuration:
 *  title, singular   - headings / messages
 *  endpoint          - e.g. "/owners"
 *  columns           - [{ header, cell: (row) => JSX }]
 *  fields            - form fields (see Owners.jsx for the shape)
 *  searchFields      - optional search/filter inputs (sent as query-string params)
 *  rowLabel          - (row) => text shown in the delete confirmation
 */
export default function CrudPage({ title, singular, endpoint, columns, fields, searchFields, rowLabel }) {
  const [rows, setRows] = useState([]);
  const [params, setParams] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");

  const [show, setShow] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({});
  const [errors, setErrors] = useState({});
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  // GET /api/<endpoint>?<search params>
  const load = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get(endpoint, { params });
      setRows(data);
      setError("");
    } catch (e) {
      setError(getError(e));
    } finally {
      setLoading(false);
    }
  }, [endpoint, params]);

  useEffect(() => { load(); }, [load]);

  const openAdd = () => {
    setEditing(null);
    setForm(Object.fromEntries(fields.map((f) => [f.name, f.defaultValue ?? ""])));
    setErrors({}); setFormError(""); setShow(true);
  };

  const openEdit = (row) => {
    setEditing(row);
    setForm(Object.fromEntries(fields.map((f) => [
      f.name,
      f.type === "time" ? (row[f.name] ?? "").slice(0, 5) : String(row[f.name] ?? "")
    ])));
    setErrors({}); setFormError(""); setShow(true);
  };

  const change = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
    setErrors((p) => ({ ...p, [name]: undefined }));
  };

  const buildBody = () => {
    const body = {};
    for (const f of fields) {
      if (f.onlyOnCreate && editing) continue;        // e.g. password is only sent when adding
      let v = typeof form[f.name] === "string" ? form[f.name].trim() : form[f.name];
      if (f.numeric) v = Number(v);
      else if (f.type === "time" && v.length === 5) v = `${v}:00`; // API expects HH:mm:ss
      body[f.name] = v;
    }
    return body;
  };

  const save = async (e) => {
    e.preventDefault();
    const v = validateForm(fields, form, !!editing);
    setErrors(v);
    if (Object.keys(v).length) return;               // stop: client-side validation failed

    setSaving(true); setFormError("");
    try {
      if (editing) await api.put(`${endpoint}/${editing.id}`, buildBody());
      else await api.post(endpoint, buildBody());
      setShow(false);
      setNotice(`${singular} ${editing ? "updated" : "added"} successfully.`);
      load();
    } catch (err) {
      setFormError(getError(err));                    // e.g. duplicate number, booking conflict
      setErrors((p) => ({ ...p, ...serverFieldErrors(err) }));
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    setDeleting(true);
    try {
      await api.delete(`${endpoint}/${toDelete.id}`);
      setNotice(`${singular} deleted.`);
      setError("");
      load();
    } catch (err) {
      setNotice("");
      setError(getError(err));                        // e.g. 409 "still has pets"
    } finally {
      setDeleting(false);
      setToDelete(null);
    }
  };

  const renderField = (f) => {
    if (f.onlyOnCreate && editing) return null;
    const err = errors[f.name];
    const common = { name: f.name, value: form[f.name] ?? "", onChange: change, isInvalid: !!err };
    let control;
    if (f.type === "select") {
      control = (
        <Form.Select {...common}>
          <option value="">Select…</option>
          {f.options.map((o) => <option key={o.value} value={o.value} disabled={o.disabled}>{o.label}</option>)}
        </Form.Select>
      );
    } else if (f.type === "radio") {
      control = (
        <div>
          {f.options.map((o) => (
            <Form.Check inline key={o.value} type="radio" id={`${f.name}-${o.value}`} name={f.name}
              label={o.label} value={o.value} checked={form[f.name] === o.value} onChange={change} isInvalid={!!err} />
          ))}
        </div>
      );
    } else if (f.type === "textarea") {
      control = <Form.Control as="textarea" rows={3} {...common} />;
    } else {
      control = <Form.Control type={f.type || "text"} step={f.step} {...common} />;
    }
    return (
      <Col md={f.full ? 12 : 6} key={f.name}>
        <Form.Group className="mb-3">
          <Form.Label>{f.label}{f.required && <span className="text-danger"> *</span>}</Form.Label>
          {control}
          {err && <Form.Control.Feedback type="invalid" className="d-block">{err}</Form.Control.Feedback>}
          {f.help && <Form.Text muted>{f.help}</Form.Text>}
        </Form.Group>
      </Col>
    );
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h3 className="mb-0">{title}</h3>
        <Button onClick={openAdd}>+ Add {singular}</Button>
      </div>

      {notice && <Alert variant="success" dismissible onClose={() => setNotice("")}>{notice}</Alert>}
      {error && <Alert variant="danger" dismissible onClose={() => setError("")}>{error}</Alert>}

      {searchFields && <SearchBar fields={searchFields} onSearch={setParams} />}

      <div className="bg-white border rounded">
        <Table responsive hover className="mb-0 align-middle">
          <thead className="table-light">
            <tr>
              {columns.map((c) => <th key={c.header}>{c.header}</th>)}
              <th style={{ width: 150 }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={columns.length + 1} className="text-center py-4"><Spinner size="sm" /> Loading…</td></tr>
            ) : rows.length === 0 ? (
              <tr><td colSpan={columns.length + 1} className="text-center text-muted py-4">No records found.</td></tr>
            ) : rows.map((row) => (
              <tr key={row.id}>
                {columns.map((c) => <td key={c.header}>{c.cell(row)}</td>)}
                <td>
                  <Button size="sm" variant="outline-primary" className="me-2" onClick={() => openEdit(row)}>Edit</Button>
                  <Button size="sm" variant="outline-danger" onClick={() => setToDelete(row)}>Delete</Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>
      <div className="text-muted small mt-2">{rows.length} record(s)</div>

      {/* Add / edit modal */}
      <Modal show={show} onHide={() => setShow(false)} size="lg" backdrop="static">
        <Form noValidate onSubmit={save}>
          <Modal.Header closeButton>
            <Modal.Title>{editing ? `Edit ${singular}` : `Add ${singular}`}</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            {formError && <Alert variant="danger">{formError}</Alert>}
            <Row>{fields.map(renderField)}</Row>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShow(false)} disabled={saving}>Cancel</Button>
            <Button type="submit" disabled={saving}>{saving ? <Spinner size="sm" /> : "Save"}</Button>
          </Modal.Footer>
        </Form>
      </Modal>

      <ConfirmModal
        show={!!toDelete}
        title={`Delete ${singular}`}
        message={toDelete ? `Are you sure you want to delete ${rowLabel(toDelete)}? This cannot be undone.` : ""}
        busy={deleting}
        onConfirm={confirmDelete}
        onCancel={() => setToDelete(null)}
      />
    </>
  );
}
