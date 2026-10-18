import { Navigate } from "react-router-dom";
import { useAuth, homeFor } from "./../AuthContext";

// Blocks pages for logged-out users or users with the wrong role.
export default function ProtectedRoute({ role, children }) {
  const { auth } = useAuth();

  // Redirect users who are not logged in.
  if (!auth) {
    return <Navigate to="/login" replace />;
  }

  // Safely retrieve the user's roles.
  const roles = Array.isArray(auth.roles) ? auth.roles : [];

  // Compare role names without case sensitivity.
  const hasRole =
    !role ||
    roles.some(
      (userRole) =>
        userRole.toUpperCase() === role.toUpperCase()
    );

  // Redirect users who do not have the required role.
  if (!hasRole) {
    return <Navigate to={homeFor(roles)} replace />;
  }

  // Allow access if the user has the required role.
  return children;
}

