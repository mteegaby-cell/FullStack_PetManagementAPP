import React, { useState } from "react";
import Modal from "react-bootstrap/Modal";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import Alert from "react-bootstrap/Alert";
import api from "./api";

const API = "/groomer";
const today = () => new Date().toISOString().slice(0, 10);

const GroomingRecordForm = ({ show, onHide, petId, onSaved }) => {
  const [date, setDate] = useState(today());
  const [service, setService] = useState("");
  const [notes, setNotes] = useState("");
  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState("");
  const [saving, setSaving] = useState(false);

  const validate = () => {
    const e = {};
    if (!date) e.date = "Grooming date is required.";
    else if (date > today()) e.date = "Grooming date cannot be in the future.";
    if (!service.trim()) e.service = "Service performed is required.";
    else if (service.length > 200) e.service = "Maximum 200 characters.";
    if (notes.length > 1000) e.notes = "Maximum 1000 characters.";
    return e;
  };

  const submit = (ev) => {
    ev.preventDefault();
    const e = validate();
    setErrors(e);
    if (Object.keys(e).length > 0) return;

    setSaving(true);
    setApiError("");
    api
      .post(`${API}/records`, {
        petId: Number(petId),
        groomingDate: date,
        servicePerformed: service.trim(),
        notes: notes.trim() || null,
      })
      .then(() => {
        setService("");
        setNotes("");
        onSaved();
      })
      .catch(() => setApiError("Could not save the record."))
      .finally(() => setSaving(false));
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Form noValidate onSubmit={submit}>
        <Modal.Header closeButton>
          <Modal.Title>Add grooming record</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {apiError && <Alert variant="danger">{apiError}</Alert>}

          <Form.Group className="mb-3">
            <Form.Label>Grooming date</Form.Label>
            <Form.Control
              type="date"
              value={date}
              max={today()}
              isInvalid={!!errors.date}
              onChange={(e) => setDate(e.target.value)}
            />
            <Form.Control.Feedback type="invalid">{errors.date}</Form.Control.Feedback>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Service performed</Form.Label>
            <Form.Control
              value={service}
              isInvalid={!!errors.service}
              onChange={(e) => setService(e.target.value)}
            />
            <Form.Control.Feedback type="invalid">{errors.service}</Form.Control.Feedback>
          </Form.Group>

          <Form.Group>
            <Form.Label>Notes</Form.Label>
            <Form.Control
              as="textarea"
              rows={4}
              value={notes}
              isInvalid={!!errors.notes}
              onChange={(e) => setNotes(e.target.value)}
            />
            <Form.Control.Feedback type="invalid">{errors.notes}</Form.Control.Feedback>
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onHide}>Cancel</Button>
          <Button type="submit" disabled={saving}>
            {saving ? "Saving..." : "Save record"}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

export default GroomingRecordForm;