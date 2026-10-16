import { useEffect, useState } from "react";
import { Button, Card, Col, ListGroup, Row, Table } from "react-bootstrap";
import { Link, useParams } from "react-router-dom";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading, fmtDate } from "../components/Common";

export default function PetDetails() {
  const { id } = useParams();
  const [pet, setPet] = useState(null);
  const [history, setHistory] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    setPet(null);
    setError("");
    Promise.all([api.get(`/owner/pets/${id}`), api.get(`/owner/pets/${id}/grooming-history`)])
      .then(([p, h]) => {
        setPet(p.data);
        setHistory(h.data);
      })
      .catch((e) =>
        setError(e.response?.status === 404 ? "Pet not found on your account." : errMsg(e))
      );
  }, [id]);

  if (error)
    return (
      <>
        <ErrorAlert message={error} />
        <Button as={Link} to="/owner/pets" variant="secondary">Back to my pets</Button>
      </>
    );
  if (!pet || !history) return <Loading />;

  const rows = [
    ["Pet number", pet.petNumber],
    ["Species", pet.species],
    ["Breed", pet.breed || "-"],
    ["Gender", pet.gender],
    ["Date of birth", fmtDate(pet.dateOfBirth)],
    ["Colour", pet.colour || "-"],
    ["Weight", `${pet.weight} kg`],
    ["Medical / allergy notes", pet.medicalNotes || "None recorded"],
  ];

  return (
    <>
      <Button as={Link} to="/owner/pets" variant="link" className="p-0 mb-2">&larr; Back to my pets</Button>
      <h2 className="mb-3">{pet.petName}</h2>
      <Row className="g-4">
        <Col lg={5}>
          <Card className="shadow-sm">
            <Card.Header>Pet information</Card.Header>
            <ListGroup variant="flush">
              {rows.map(([label, value]) => (
                <ListGroup.Item key={label} className="d-flex justify-content-between">
                  <span className="text-muted">{label}</span>
                  <strong className="text-end">{value}</strong>
                </ListGroup.Item>
              ))}
            </ListGroup>
          </Card>
        </Col>
        <Col lg={7}>
          <Card className="shadow-sm">
            <Card.Header>Grooming history</Card.Header>
            <Card.Body>
              {history.length === 0 ? (
                <p className="text-muted mb-0">No grooming records yet.</p>
              ) : (
                <Table responsive hover size="sm" className="mb-0">
                  <thead><tr><th>Date</th><th>Service</th><th>Groomer</th><th>Notes</th></tr></thead>
                  <tbody>
                    {history.map((h) => (
                      <tr key={h.id}>
                        <td>{fmtDate(h.groomingDate)}</td>
                        <td>{h.servicePerformed}</td>
                        <td>{h.groomerName}</td>
                        <td>{h.notes}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </>
  );
}
