import { Navigate, Route, Routes } from "react-router-dom";
import { Container } from "react-bootstrap";
import { useAuth, homeFor } from "./AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import OwnerLayout from "./components/OwnerLayout";
import Login from "./pages/Login";
import Register from "./pages/Register";
import OwnerDashboard from "./pages/OwnerDashboard";
import Profile from "./pages/Profile";
import MyPets from "./pages/MyPets";
import PetDetails from "./pages/PetDetails";
import Services from "./pages/Services";
import BookAppointment from "./pages/BookAppointment";
import Appointments from "./pages/Appointments";

// Placeholders so Admin/Groomer logins land somewhere. Teammates replace these.
const Placeholder = ({ title }) => (
  <Container className="py-5">
    <h2>{title}</h2>
    <p className="text-muted">This dashboard is built by another group member.</p>
  </Container>
);

export default function App() {
  const { auth } = useAuth();

  return (
    <Routes>
      <Route path="/" element={<Navigate to={auth ? homeFor(auth.roles) : "/login"} replace />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* Pet Owner area */}
      <Route
        path="/owner"
        element={
          <ProtectedRoute role="PetOwner">
            <OwnerLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<OwnerDashboard />} />
        <Route path="profile" element={<Profile />} />
        <Route path="pets" element={<MyPets />} />
        <Route path="pets/:id" element={<PetDetails />} />
        <Route path="services" element={<Services />} />
        <Route path="book" element={<BookAppointment />} />
        <Route path="appointments" element={<Appointments />} />
      </Route>

      <Route path="/admin" element={<ProtectedRoute role="Admin"><Placeholder title="Admin Dashboard" /></ProtectedRoute>} />
      <Route path="/groomer" element={<ProtectedRoute role="Groomer"><Placeholder title="Groomer Dashboard" /></ProtectedRoute>} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
