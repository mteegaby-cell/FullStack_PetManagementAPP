import { useState } from "react";
import { Button, Card, Container, Form } from "react-bootstrap";
import { Link, useNavigate } from "react-router-dom";
import { useAuth, homeFor } from "../AuthContext";
import { errMsg } from "../api";
import { ErrorAlert } from "../components/Common";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: "", password: "" });
  const [validated, setValidated] = useState(false);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!e.currentTarget.checkValidity()) {
      setValidated(true);
      return;
    }
    setBusy(true);
    try {
      const data = await login(form.email, form.password);
      navigate(homeFor(data.roles), { replace: true });   // role-specific dashboard
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Container className="d-flex justify-content-center py-5">
      <Card style={{ width: 420 }} className="shadow-sm">
        <Card.Body className="p-4">
          <h3 className="text-center mb-1">🐾 Paws &amp; Care</h3>
          <p className="text-center text-muted">Sign in to your account</p>
          <ErrorAlert message={error} />
          <Form noValidate validated={validated} onSubmit={onSubmit}>
            <Form.Group className="mb-3" controlId="email">
              <Form.Label>Email</Form.Label>
              <Form.Control
                type="email" required value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
              />
              <Form.Control.Feedback type="invalid">Enter a valid email address.</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="password">
              <Form.Label>Password</Form.Label>
              <Form.Control
                type="password" required value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
              />
              <Form.Control.Feedback type="invalid">Password is required.</Form.Control.Feedback>
            </Form.Group>
            <Button type="submit" className="w-100" disabled={busy}>
              {busy ? "Signing in..." : "Login"}
            </Button>
          </Form>
          <p className="text-center mt-3 mb-0">
            New pet owner? <Link to="/register">Create an account</Link>
          </p>
        </Card.Body>
      </Card>
    </Container>
  );
}
