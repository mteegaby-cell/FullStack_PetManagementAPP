import { useEffect, useState } from "react";
import { Button, Card, Col, Row } from "react-bootstrap";
import { Link } from "react-router-dom";
import api, { errMsg } from "./../api";
import { ErrorAlert, Loading, fmtMoney } from "../components/Common";

export default function Services() {
  const [services, setServices] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.get("/owner/services").then((r) => setServices(r.data)).catch((e) => setError(errMsg(e)));
  }, []);

  if (error) return <ErrorAlert message={error} />;
  if (!services) return <Loading />;

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="mb-0">Grooming Services</h2>
        <Button as={Link} to="/owner/book">Book an appointment</Button>
      </div>
      <Row className="g-3">
        {services.map((s) => (
          <Col md={6} lg={4} key={s.id}>
            <Card className="h-100 shadow-sm">
              <Card.Body>
                <Card.Title>{s.serviceName}</Card.Title>
                <Card.Text>{s.description}</Card.Text>
              </Card.Body>
              <Card.Footer className="d-flex justify-content-between">
                <span>{s.durationMinutes} min</span>
                <strong>{fmtMoney(s.price)}</strong>
              </Card.Footer>
            </Card>
          </Col>
        ))}
      </Row>
    </>
  );
}
