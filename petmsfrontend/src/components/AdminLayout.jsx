import { Button, Container, Nav, Navbar } from "react-bootstrap";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const links = [
  { to: "/admin", label: "Dashboard", end: true },
  { to: "/admin/owners", label: "Pet Owners" },
  { to: "/admin/groomers", label: "Groomers" },
  { to: "/admin/pets", label: "Pets" },
  { to: "/admin/services", label: "Services" },
  { to: "/admin/appointments", label: "Appointments" }
];

export default function AdminLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <>
      <Navbar bg="dark" variant="dark" expand="lg" className="mb-4">
        <Container>
          <Navbar.Brand as={NavLink} to="/admin">🐾 Paws &amp; Care</Navbar.Brand>
          <Navbar.Toggle aria-controls="admin-nav" />
          <Navbar.Collapse id="admin-nav">
            <Nav className="me-auto">
              {links.map((l) => (
                <Nav.Link key={l.to} as={NavLink} to={l.to} end={l.end}>{l.label}</Nav.Link>
              ))}
            </Nav>
            <span className="text-light me-3">{user.fullName} (Admin)</span>
            <Button size="sm" variant="outline-light" onClick={() => { logout(); navigate("/login"); }}>
              Logout
            </Button>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container className="pb-5">
        <Outlet />
      </Container>
    </>
  );
}
