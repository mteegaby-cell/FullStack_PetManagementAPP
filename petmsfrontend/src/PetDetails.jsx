import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import Card from "react-bootstrap/Card";
import Table from "react-bootstrap/Table";
import Alert from "react-bootstrap/Alert";
import Button from "react-bootstrap/Button";
import axios from "axios";
import GroomingRecordForm from "./GroomingRecordForm";

const API = "https://localhost:7242/api/Groomer";

const PetDetails = () => {
  const { id } = useParams();
  const [pet, setPet] = useState(null);
  const [history, setHistory] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showForm, setShowForm] = useState(false);

  const load = () => {
    Promise.all([
      axios.get(`${API}/pets/${id}`),
      axios.get(`${API}/pets/${id}/history`),
    ])
      .then(([p, h]) => {
        setPet(p.data);
        setHistory(h.data);
      })
      .catch((err) =>
        setError(
          err.response?.status === 404
            ? "You do not have access to this pet."
            : "Could not load pet information."
        )
      );
  };

  useEffect(() => {
    load();
  }, [id]);

  if (error)
    return (
      <>
        <Alert variant="danger">{error}</Alert>
        <Link to="/appointments">Back to appointments</Link>
      </>
    );
  if (!pet) return <p>Loading...</p>;

  return (
    <>
      <Link to="/appointments">&larr; Back to appointments</Link>
      {success && (
        <Alert variant="success" className="mt-3" dismissible onClose={() => setSuccess("")}>
          {success}
        </Alert>
      )}

      <Card className="my-3">
        <Card.Header as="h5">{pet.petName}</Card.Header>
        <Card.Body>
          <p><strong>Species:</strong> {pet.species}</p>
          <p><strong>Breed:</strong> {pet.breed}</p>
          <p><strong>Gender:</strong> {pet.gender}</p>
          <p><strong>Date of birth:</strong> {pet.dateOfBirth.slice(0, 10)}</p>
          <p><strong>Colour:</strong> {pet.colour}</p>
          <p><strong>Weight:</strong> {pet.weight} kg</p>
          <p><strong>Owner:</strong> {pet.ownerName}</p>
          <p><strong>Medical / allergy notes:</strong> {pet.medicalNotes || "None recorded"}</p>
        </Card.Body>
      </Card>

      <div className="d-flex justify-content-between align-items-center mb-2">
        <h5 className="mb-0">Grooming history</h5>
        <Button onClick={() => setShowForm(true)}>Add grooming record</Button>
      </div>

      <Table striped bordered hover>
        <thead>
          <tr><th>Date</th><th>Service performed</th><th>Notes</th><th>Groomer</th></tr>
        </thead>
        <tbody>
          {history.length > 0 ? (
            history.map((r) => (
              <tr key={r.id}>
                <td>{r.groomingDate.slice(0, 10)}</td>
                <td>{r.servicePerformed}</td>
                <td>{r.notes}</td>
                <td>{r.groomerName}</td>
              </tr>
            ))
          ) : (
            <tr><td colSpan="4">No grooming records yet.</td></tr>
          )}
        </tbody>
      </Table>

      <GroomingRecordForm
        show={showForm}
        onHide={() => setShowForm(false)}
        petId={id}
        onSaved={() => {
          setShowForm(false);
          setSuccess("Grooming record saved.");
          load();
        }}
      />
    </>
  );
};

export default PetDetails;
