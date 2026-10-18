import { useState } from "react";
import { Alert, Button, Card, Container, Form, Spinner } from "react-bootstrap";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getError, homeFor, isEmail } from "../utils";

export default function Login() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: "", password: "" });
  const [errors, setErrors] = useState({});
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  if (user) return <Navigate to={homeFor(user.role)} replace />;

  const submit = async (e) => {
    e.preventDefault();
    const v = {};
    if (!form.email.trim()) v.email = "Email is required.";
    else if (!isEmail(form.email.trim())) v.email = "Enter a valid email address.";
    if (!form.password) v.password = "Password is required.";
    setErrors(v);
    if (Object.keys(v).length) return;

    setBusy(true); setError("");
    try {
      const u = await login(form.email.trim(), form.password);
      navigate(homeFor(u.role), { replace: true });   // each role has its own dashboard
    } catch (err) {
      setError(getError(err));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: "100vh" }}>
      <Card className="login-card w-100 shadow-sm">
        <Card.Body className="p-4">
          <h3 className="text-center mb-1">🐾 Paws &amp; Care</h3>
          <p className="text-center text-muted mb-4">Sign in to continue</p>
          {error && <Alert variant="danger">{error}</Alert>}
          <Form noValidate onSubmit={submit}>
            <Form.Group className="mb-3">
              <Form.Label>Email</Form.Label>
              <Form.Control type="email" value={form.email} isInvalid={!!errors.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })} />
              <Form.Control.Feedback type="invalid">{errors.email}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-4">
              <Form.Label>Password</Form.Label>
              <Form.Control type="password" value={form.password} isInvalid={!!errors.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })} />
              <Form.Control.Feedback type="invalid">{errors.password}</Form.Control.Feedback>
            </Form.Group>
            <Button type="submit" className="w-100" disabled={busy}>
              {busy ? <Spinner size="sm" /> : "Login"}
            </Button>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
}
