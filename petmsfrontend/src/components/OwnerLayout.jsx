import { Container, Nav, Navbar, Button } from "react-bootstrap";
import { LinkContainerNav } from "./NavLinkItem";
import { Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../AuthContext";

export default function OwnerLayout() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <>
      <Navbar bg="dark" variant="dark" expand="md">
        <Container>
          <Navbar.Brand>🐾 Paws &amp; Care</Navbar.Brand>
          <Navbar.Toggle aria-controls="owner-nav" />
          <Navbar.Collapse id="owner-nav">
            <Nav className="me-auto">
              <LinkContainerNav to="/owner" end>Dashboard</LinkContainerNav>
              <LinkContainerNav to="/owner/profile">My Profile</LinkContainerNav>
              <LinkContainerNav to="/owner/pets">My Pets</LinkContainerNav>
              <LinkContainerNav to="/owner/services">Services</LinkContainerNav>
              <LinkContainerNav to="/owner/book">Book</LinkContainerNav>
              <LinkContainerNav to="/owner/appointments">Appointments</LinkContainerNav>
            </Nav>
            <Navbar.Text className="me-3">{auth.displayName}</Navbar.Text>
            <Button variant="outline-light" size="sm" onClick={handleLogout}>Logout</Button>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container className="py-4">
        <Outlet />
      </Container>
    </>
  );
}
