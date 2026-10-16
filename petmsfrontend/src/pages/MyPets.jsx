import { useEffect, useState } from "react";
import { Button, Card, Col, Row } from "react-bootstrap";
import { Link } from "react-router-dom";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading, fmtDate } from "../components/Common";

export default function MyPets() {
  const [pets, setPets] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.get("/owner/pets").then((r) => setPets(r.data)).catch((e) => setError(errMsg(e)));
  }, []);

  if (error) return <ErrorAlert message={error} />;
  if (!pets) return <Loading />;

  return (
    <>
      <h2 className="mb-3">My Pets</h2>
      {pets.length === 0 && <p className="text-muted">You have no registered pets yet. Please contact the clinic to add one.</p>}
      <Row className="g-3">
        {pets.map((p) => (
          <Col md={6} lg={4} key={p.id}>
            <Card className="h-100 shadow-sm">
              <Card.Body>
                <Card.Title>{p.petName} <small className="text-muted">({p.petNumber})</small></Card.Title>
                <Card.Subtitle className="mb-2 text-muted">
                  {p.species}{p.breed ? ` - ${p.breed}` : ""}
                </Card.Subtitle>
                <Card.Text>
                  {p.gender}, born {fmtDate(p.dateOfBirth)}<br />
                  {p.colour && <>Colour: {p.colour}<br /></>}
                  Weight: {p.weight} kg
                </Card.Text>
                <Button as={Link} to={`/owner/pets/${p.id}`} size="sm">Details &amp; grooming history</Button>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </>
  );
}
