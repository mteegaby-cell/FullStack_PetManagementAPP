import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import Card from "react-bootstrap/Card";
import Row from "react-bootstrap/Row";
import Col from "react-bootstrap/Col";
import Alert from "react-bootstrap/Alert";
import axios from "axios";

const API = "https://localhost:7242/api/Groomer";

const todayStr = () => {
  const d = new Date();
  const p = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
};

const GroomerDashboard = () => {
  const [me, setMe] = useState(null);
  const [appts, setAppts] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([axios.get(`${API}/me`), axios.get(`${API}/appointments`)])
      .then(([m, a]) => {
        setMe(m.data);
        setAppts(a.data);
      })
      .catch(() => setError("Could not load the dashboard."));
  }, []);

  const today = todayStr();
  const active = (a) => !["Completed", "Cancelled"].includes(a.status);
  const todays = appts.filter((a) => a.appointmentDate.slice(0, 10) === today);
  const upcoming = appts.filter((a) => a.appointmentDate.slice(0, 10) >= today && active(a));
  const completed = appts.filter((a) => a.status === "Completed");

  return (
    <>
      {error && <Alert variant="danger">{error}</Alert>}
      {me && (
        <>
          <h4>Welcome, {me.name} {me.surname}</h4>
          <p className="text-muted">Staff no. {me.staffNumber} · {me.specialization}</p>
        </>
      )}
      <Row className="g-3 mb-3">
        <Col md={4}><Card body className="text-center"><h2>{todays.length}</h2>Today's appointments</Card></Col>
        <Col md={4}><Card body className="text-center"><h2>{upcoming.length}</h2>Upcoming / active</Card></Col>
        <Col md={4}><Card body className="text-center"><h2>{completed.length}</h2>Completed</Card></Col>
      </Row>
      <Link to="/appointments">View all appointments &rarr;</Link>
    </>
  );
};

export default GroomerDashboard;