import { useEffect, useState } from "react";
import { Col, Form, Row, Tab, Table, Tabs } from "react-bootstrap";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading, STATUSES, StatusBadge, fmtDate, fmtMoney, fmtTime } from "../components/Common";

export default function Appointments() {
  const [tab, setTab] = useState("upcoming");
  const [status, setStatus] = useState("");
  const [rows, setRows] = useState(null);
  const [error, setError] = useState("");

  // reload whenever the tab or the status filter changes (filtering is done by the API)
  useEffect(() => {
    setRows(null);
    setError("");
    api
      .get(`/owner/appointments/${tab}`, { params: status ? { status } : {} })
      .then((r) => setRows(r.data))
      .catch((e) => setError(errMsg(e)));
  }, [tab, status]);

  return (
    <>
      <h2 className="mb-3">My Appointments</h2>
      <Row className="mb-3">
        <Col md={4}>
          <Form.Group controlId="statusFilter">
            <Form.Label>Filter by status</Form.Label>
            <Form.Select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">All statuses</option>
              {STATUSES.map((s) => (
                <option key={s} value={s}>{s.replace(/([a-z])([A-Z])/g, "$1 $2")}</option>
              ))}
            </Form.Select>
          </Form.Group>
        </Col>
      </Row>

      <Tabs activeKey={tab} onSelect={(k) => setTab(k)} className="mb-3">
        <Tab eventKey="upcoming" title="Upcoming" />
        <Tab eventKey="history" title="History" />
      </Tabs>

      <ErrorAlert message={error} />
      {!rows && !error && <Loading />}
      {rows && rows.length === 0 && <p className="text-muted">No appointments found.</p>}
      {rows && rows.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr><th>Date</th><th>Time</th><th>Pet</th><th>Service</th><th>Groomer</th><th>Price</th><th>Status</th><th>Notes</th></tr>
          </thead>
          <tbody>
            {rows.map((a) => (
              <tr key={a.id}>
                <td>{fmtDate(a.appointmentDate)}</td>
                <td>{fmtTime(a.appointmentTime)}</td>
                <td>{a.petName}</td>
                <td>{a.serviceName}</td>
                <td>{a.groomerName ?? <span className="text-muted">To be assigned</span>}</td>
                <td>{fmtMoney(a.price)}</td>
                <td><StatusBadge status={a.status} /></td>
                <td>{a.notes}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </>
  );
}
