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
import GroomerDashboard from "./GroomerDashboard";
import LMSCRUD from "./LMSCRUD";
import GroomerPetDetails from "./PetDetails";
import AdminLayout from "./components/AdminLayout";
import AdminDashboard from "./pages/admin/AdminDashboard";
import AdminOwners from "./pages/admin/AdminOwners";
import AdminGroomers from "./pages/admin/AdminGroomers";
import AdminPets from "./pages/admin/AdminPets";
import AdminServices from "./pages/admin/AdminServices";
import AdminAppointments from "./pages/admin/AdminAppointments";

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

      {/* Groomer area */}
      <Route
        path="/groomer"
        element={
          <ProtectedRoute role="Groomer">
            <GroomerDashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/groomer/appointments"
        element={
          <ProtectedRoute role="Groomer">
            <LMSCRUD />
          </ProtectedRoute>
        }
      />

      {/* Admin area */}
      <Route
    path="/admin"
    element={
      <ProtectedRoute role="Admin">
        <AdminLayout />
      </ProtectedRoute>
    }
  />
    <Route>
      <Route index element={<AdminDashboard />} />
        <Route path="/admin/owners" element={<AdminOwners />} />
        <Route path="/admin/groomers" element={<AdminGroomers />} />
        <Route path="/admin/pets" element={<AdminPets />} />
        <Route path="/admin/services" element={<AdminServices />} />
        <Route path="/admin/appointments" element={<AdminAppointments />} />
    </Route>

      {/* Backwards-compatible routes from the original App.jsx */}
      <Route
        path="/appointments"
        element={
          <ProtectedRoute role="Groomer">
            <LMSCRUD />
          </ProtectedRoute>
        }
      />
      <Route
        path="/pets/:id"
        element={
          <ProtectedRoute role="Groomer">
            <GroomerPetDetails />
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}