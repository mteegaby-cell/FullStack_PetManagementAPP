import { useState } from "react";
import { Button, Col, Form, Row } from "react-bootstrap";

/**
 * fields: [{ name, label, type?: "text" | "date" | "select", options?: [{value,label}] }]
 * onSearch receives only the filled-in values, e.g. { name: "Sam", surname: "Dlamini" },
 * which become query-string parameters on the API request.
 */
export default function SearchBar({ fields, onSearch }) {
  const empty = Object.fromEntries(fields.map((f) => [f.name, ""]));
  const [values, setValues] = useState(empty);

  const submit = (e) => {
    e.preventDefault();
    onSearch(Object.fromEntries(Object.entries(values).filter(([, v]) => v.trim() !== "")));
  };
  const clear = () => { setValues(empty); onSearch({}); };

  return (
    <Form onSubmit={submit} className="bg-white border rounded p-3 mb-3">
      <Row className="g-2 align-items-end">
        {fields.map((f) => (
          <Col md key={f.name}>
            <Form.Label className="small mb-1">{f.label}</Form.Label>
            {f.type === "select" ? (
              <Form.Select value={values[f.name]} onChange={(e) => setValues({ ...values, [f.name]: e.target.value })}>
                {f.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Form.Select>
            ) : (
              <Form.Control
                type={f.type || "text"}
                placeholder={f.placeholder}
                value={values[f.name]}
                onChange={(e) => setValues({ ...values, [f.name]: e.target.value })}
              />
            )}
          </Col>
        ))}
        <Col md="auto" className="d-flex gap-2">
          <Button type="submit">Search</Button>
          <Button variant="outline-secondary" onClick={clear}>Clear</Button>
        </Col>
      </Row>
    </Form>
  );
}
