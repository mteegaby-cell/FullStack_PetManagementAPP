import { useEffect, useState } from "react";
import { Button, Card, Col, Row, Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading, StatusBadge, fmtDate, fmtTime } from "../components/Common";

export default function OwnerDashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([api.get("/owner/me"), api.get("/owner/pets"), api.get("/owner/appointments/upcoming")])
      .then(([me, pets, upcoming]) => setData({ me: me.data, pets: pets.data, upcoming: upcoming.data }))
      .catch((e) => setError(errMsg(e)));
  }, []);

  if (error) return <ErrorAlert message={error} />;
  if (!data) return <Loading />;

  const { me, pets, upcoming } = data;
  const next = upcoming[0];

  return (
    <>
      <h2 className="mb-1">Welcome, {me.name}!</h2>
      <p className="text-muted">Owner no. {me.ownerNumber}</p>

      <Row className="g-3 mb-4">
        <Col md={4}>
          <Card className="h-100 text-center shadow-sm">
            <Card.Body>
              <div className="display-5">{pets.length}</div>
              <Card.Text>Registered pets</Card.Text>
              <Button as={Link} to="/owner/pets" variant="outline-primary" size="sm">View pets</Button>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="h-100 text-center shadow-sm">
            <Card.Body>
              <div className="display-5">{upcoming.length}</div>
              <Card.Text>Upcoming appointments</Card.Text>
              <Button as={Link} to="/owner/appointments" variant="outline-primary" size="sm">View all</Button>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="h-100 text-center shadow-sm">
            <Card.Body>
              <Card.Title>Next appointment</Card.Title>
              {next ? (
                <Card.Text>
                  <strong>{next.petName}</strong> - {next.serviceName}<br />
                  {fmtDate(next.appointmentDate)} at {fmtTime(next.appointmentTime)}
                </Card.Text>
              ) : (
                <Card.Text className="text-muted">Nothing booked yet.</Card.Text>
              )}
              <Button as={Link} to="/owner/book" size="sm">Book a groom</Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <h5>Upcoming appointments</h5>
      {upcoming.length === 0 ? (
        <p className="text-muted">No upcoming appointments.</p>
      ) : (
        <Table responsive hover size="sm">
          <thead><tr><th>Date</th><th>Time</th><th>Pet</th><th>Service</th><th>Status</th></tr></thead>
          <tbody>
            {upcoming.slice(0, 5).map((a) => (
              <tr key={a.id}>
                <td>{fmtDate(a.appointmentDate)}</td>
                <td>{fmtTime(a.appointmentTime)}</td>
                <td>{a.petName}</td>
                <td>{a.serviceName}</td>
                <td><StatusBadge status={a.status} /></td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </>
  );
}
