import { Navigate } from "react-router-dom";
import { useAuth, homeFor } from "../AuthContext";

// Blocks pages for people who are logged out or have the wrong role.
// (The API enforces this too - this only keeps the UI tidy.)
export default function ProtectedRoute({ role, children }) {
  const { auth } = useAuth();
  if (!auth) return <Navigate to="/login" replace />;
  if (role && !auth.roles.includes(role)) return <Navigate to={homeFor(auth.roles)} replace />;
  return children;
}
