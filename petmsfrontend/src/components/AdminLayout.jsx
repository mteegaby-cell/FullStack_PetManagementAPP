import { Button, Container, Nav, Navbar } from "react-bootstrap";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../AuthContext";

const links = [
  { to: "/admin", label: "Dashboard", end: true },
  { to: "/admin/owners", label: "Pet Owners" },
  { to: "/admin/groomers", label: "Groomers" },
  { to: "/admin/pets", label: "Pets" },
  { to: "/admin/services", label: "Services" },
  { to: "/admin/appointments", label: "Appointments" },
];

export default function AdminLayout() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();

  // Get the logged-in user's details safely.
  const user = auth?.user ?? auth;

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <>
      <Navbar
        bg="dark"
        variant="dark"
        expand="lg"
        className="mb-4"
      >
        <Container>
          <Navbar.Brand as={NavLink} to="/admin">
            🐾 Paws &amp; Care
          </Navbar.Brand>

          <Navbar.Toggle aria-controls="admin-nav" />

          <Navbar.Collapse id="admin-nav">
            <Nav className="me-auto">
              {links.map((link) => (
                <Nav.Link
                  key={link.to}
                  as={NavLink}
                  to={link.to}
                  end={link.end}
                >
                  {link.label}
                </Nav.Link>
              ))}
            </Nav>

            <span className="text-light me-3">
              {user?.fullName ?? user?.email ?? "Admin"} (Admin)
            </span>

            <Button
              size="sm"
              variant="outline-light"
              onClick={handleLogout}
            >
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
