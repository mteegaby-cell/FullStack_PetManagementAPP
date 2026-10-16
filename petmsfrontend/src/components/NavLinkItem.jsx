import { Nav } from "react-bootstrap";
import { NavLink } from "react-router-dom";

// A react-bootstrap nav link that uses react-router for navigation and highlights the active page
export function LinkContainerNav({ to, end, children }) {
  return (
    <Nav.Link as={NavLink} to={to} end={end}>
      {children}
    </Nav.Link>
  );
}
