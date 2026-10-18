import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { homeFor } from "./AuthContext";

/** Blocks the page unless logged in (and, if `role` is given, unless the user has that role). */
export default function ProtectedRoute({ role, children }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (role && user.role !== role) return <Navigate to={homeFor(user.role)} replace />;
  return children;
}
