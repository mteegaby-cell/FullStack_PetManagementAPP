import { useState } from "react";
import { Alert, Button, Card, Col, Container, Form, Row } from "react-bootstrap";
import { Link } from "react-router-dom";
import api, { errMsg } from "../api";
import { ErrorAlert } from "../components/Common";

const empty = { name: "", surname: "", email: "", phoneNumber: "", homeAddress: "", password: "" };

export default function Register() {
  const [form, setForm] = useState(empty);
  const [validated, setValidated] = useState(false);
  const [error, setError] = useState("");
  const [done, setDone] = useState(false);
  const [busy, setBusy] = useState(false);

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!e.currentTarget.checkValidity()) {
      setValidated(true);
      return;
    }
    setBusy(true);
    try {
      await api.post("/auth/register", form);
      setDone(true);
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setBusy(false);
    }
  };

  if (done)
    return (
      <Container className="py-5" style={{ maxWidth: 520 }}>
        <Alert variant="success">
          Registration successful! <Link to="/login">Go to login</Link>
        </Alert>
      </Container>
    );

  return (
    <Container className="d-flex justify-content-center py-5">
      <Card style={{ width: 600 }} className="shadow-sm">
        <Card.Body className="p-4">
          <h3 className="mb-3">Pet Owner Registration</h3>
          <ErrorAlert message={error} />
          <Form noValidate validated={validated} onSubmit={onSubmit}>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3" controlId="name">
                  <Form.Label>Name</Form.Label>
                  <Form.Control required maxLength={50} value={form.name} onChange={set("name")} />
                  <Form.Control.Feedback type="invalid">Name is required.</Form.Control.Feedback>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3" controlId="surname">
                  <Form.Label>Surname</Form.Label>
                  <Form.Control required maxLength={50} value={form.surname} onChange={set("surname")} />
                  <Form.Control.Feedback type="invalid">Surname is required.</Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>
            <Form.Group className="mb-3" controlId="email">
              <Form.Label>Email address</Form.Label>
              <Form.Control type="email" required value={form.email} onChange={set("email")} />
              <Form.Control.Feedback type="invalid">Enter a valid email address.</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="phone">
              <Form.Label>Phone number</Form.Label>
              <Form.Control
                required pattern="^\+?[0-9\s\-]{10,15}$" placeholder="e.g. 0821234567"
                value={form.phoneNumber} onChange={set("phoneNumber")}
              />
              <Form.Control.Feedback type="invalid">Enter a valid phone number (10-15 digits).</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="address">
              <Form.Label>Home address</Form.Label>
              <Form.Control required maxLength={200} value={form.homeAddress} onChange={set("homeAddress")} />
              <Form.Control.Feedback type="invalid">Home address is required.</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="password">
              <Form.Label>Password</Form.Label>
              <Form.Control type="password" required minLength={8} value={form.password} onChange={set("password")} />
              <Form.Text muted>At least 8 characters with upper and lower case, a number and a symbol.</Form.Text>
              <Form.Control.Feedback type="invalid">Password must be at least 8 characters.</Form.Control.Feedback>
            </Form.Group>
            <Button type="submit" className="w-100" disabled={busy}>
              {busy ? "Registering..." : "Register"}
            </Button>
          </Form>
          <p className="text-center mt-3 mb-0">
            Already registered? <Link to="/login">Login</Link>
          </p>
        </Card.Body>
      </Card>
    </Container>
  );
}
