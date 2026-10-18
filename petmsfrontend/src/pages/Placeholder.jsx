import { Button, Container } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

// Temporary dashboards for the Groomer and Pet Owner roles (built by other group members).
export default function Placeholder({ title }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <Container className="py-5 text-center">
      <h3>{title}</h3>
      <p className="text-muted">Welcome, {user.fullName}. This dashboard is still to be built.</p>
      <Button variant="outline-dark" onClick={() => { logout(); navigate("/login"); }}>Logout</Button>
    </Container>
  );
}
