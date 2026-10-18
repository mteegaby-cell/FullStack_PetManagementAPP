import { useEffect, useState } from "react";
import { Badge } from "react-bootstrap";
import api from "../../api";
import CrudPage from "../../components/CrudPage";
import { APPOINTMENT_STATUSES, hhmm, money, statusLabel, statusVariant, todayISO } from "../../utils";

const columns = [
  { header: "Date", cell: (r) => r.appointmentDate },
  { header: "Time", cell: (r) => hhmm(r.appointmentTime) },
  { header: "Pet", cell: (r) => r.petName },
  { header: "Owner", cell: (r) => r.ownerName },
  { header: "Groomer", cell: (r) => r.groomerName },
  { header: "Service", cell: (r) => r.serviceName },
  { header: "Status", cell: (r) => <Badge bg={statusVariant(r.status)}>{statusLabel(r.status)}</Badge> }
];

export default function Appointments() {
  const [pets, setPets] = useState([]);
  const [groomers, setGroomers] = useState([]);
  const [services, setServices] = useState([]);

  useEffect(() => {
    api.get("/pets").then((r) => setPets(r.data)).catch(() => {});
    api.get("/groomers").then((r) => setGroomers(r.data)).catch(() => {});
    api.get("/services").then((r) => setServices(r.data)).catch(() => {});
  }, []);

  const statusOptions = APPOINTMENT_STATUSES.map((s) => ({ value: s, label: statusLabel(s) }));

  const fields = [
    {
      name: "appointmentDate", label: "Appointment Date", type: "date", required: true,
      check: (v, _f, isEditing) => (!isEditing && v < todayISO() ? "Appointments cannot be booked in the past." : "")
    },
    {
      name: "appointmentTime", label: "Appointment Time", type: "time", required: true,
      check: (v) => (v >= "08:00" && v <= "17:00" ? "" : "Opening hours are 08:00 - 17:00.")
    },
    {
      name: "petId", label: "Pet (owner is assigned automatically)", type: "select", required: true, numeric: true, full: true,
      options: pets.map((p) => ({ value: String(p.id), label: `${p.petName} – ${p.ownerName} (${p.petNumber})` }))
    },
    {
      name: "groomerId", label: "Groomer", type: "select", required: true, numeric: true,
      options: groomers.map((g) => ({ value: String(g.id), label: `${g.name} ${g.surname} – ${g.specialization}` }))
    },
    {
      name: "serviceId", label: "Service", type: "select", required: true, numeric: true,
      options: services.map((s) => ({
        value: String(s.id),
        label: `${s.serviceName} (${s.durationMinutes} min, ${money(s.price)})${s.status === "Inactive" ? " – inactive" : ""}`,
        disabled: s.status === "Inactive"
      }))
    },
    { name: "status", label: "Status", type: "select", required: true, defaultValue: "Scheduled", options: statusOptions },
    { name: "notes", label: "Notes", type: "textarea", full: true }
  ];

  const searchFields = [
    { name: "status", label: "Status", type: "select", options: [{ value: "", label: "All statuses" }, ...statusOptions] },
    { name: "date", label: "Date", type: "date" },
    {
      name: "groomerId", label: "Groomer", type: "select",
      options: [{ value: "", label: "All groomers" }, ...groomers.map((g) => ({ value: String(g.id), label: `${g.name} ${g.surname}` }))]
    }
  ];

  return (
    <CrudPage title="Appointments" singular="Appointment" endpoint="/appointments" columns={columns} fields={fields}
      searchFields={searchFields} rowLabel={(r) => `${r.petName}'s appointment on ${r.appointmentDate}`} />
  );
}
