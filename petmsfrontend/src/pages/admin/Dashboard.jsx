import { useEffect, useState } from "react";
import { Alert, Badge, Card, Col, Row, Spinner, Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import api from "../../api";
import { APPOINTMENT_STATUSES, getError, hhmm, statusLabel, statusVariant, todayISO } from "../../utils";

const tiles = [
  { key: "owners", label: "Pet Owners", to: "/admin/owners", icon: "👤" },
  { key: "groomers", label: "Groomers", to: "/admin/groomers", icon: "✂️" },
  { key: "pets", label: "Pets", to: "/admin/pets", icon: "🐶" },
  { key: "services", label: "Services", to: "/admin/services", icon: "🛁" }
];

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all(["owners", "groomers", "pets", "services", "appointments"].map((e) => api.get(`/${e}`)))
      .then(([o, g, p, s, a]) =>
        setData({ owners: o.data.length, groomers: g.data.length, pets: p.data.length, services: s.data.length, appointments: a.data }))
      .catch((e) => setError(getError(e)));
  }, []);

  if (error) return <Alert variant="danger">{error}</Alert>;
  if (!data) return <div className="text-center py-5"><Spinner /></div>;

  const upcoming = data.appointments
    .filter((a) => a.status === "Scheduled" && a.appointmentDate >= todayISO())
    .slice(0, 5);

  return (
    <>
      <h3 className="mb-3">Administrator Dashboard</h3>
      <Row className="g-3 mb-4">
        {tiles.map((t) => (
          <Col md={3} sm={6} key={t.key}>
            <Card as={Link} to={t.to} className="stat-tile text-decoration-none text-center shadow-sm h-100">
              <Card.Body>
                <div style={{ fontSize: 32 }}>{t.icon}</div>
                <h2 className="mb-0">{data[t.key]}</h2>
                <div className="text-muted">{t.label}</div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

      <Card className="shadow-sm mb-4">
        <Card.Header className="d-flex justify-content-between">
          <strong>Appointments by status</strong>
          <Link to="/admin/appointments">Manage</Link>
        </Card.Header>
        <Card.Body className="d-flex flex-wrap gap-3">
          {APPOINTMENT_STATUSES.map((s) => (
            <div key={s}>
              <Badge bg={statusVariant(s)} className="me-1">{data.appointments.filter((a) => a.status === s).length}</Badge>
              {statusLabel(s)}
            </div>
          ))}
        </Card.Body>
      </Card>

      <Card className="shadow-sm">
        <Card.Header><strong>Next scheduled appointments</strong></Card.Header>
        <Table responsive className="mb-0">
          <thead className="table-light"><tr><th>Date</th><th>Time</th><th>Pet</th><th>Groomer</th><th>Service</th></tr></thead>
          <tbody>
            {upcoming.length === 0 ? (
              <tr><td colSpan={5} className="text-center text-muted py-3">No upcoming appointments.</td></tr>
            ) : upcoming.map((a) => (
              <tr key={a.id}>
                <td>{a.appointmentDate}</td><td>{hhmm(a.appointmentTime)}</td>
                <td>{a.petName}</td><td>{a.groomerName}</td><td>{a.serviceName}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </Card>
    </>
  );
}
