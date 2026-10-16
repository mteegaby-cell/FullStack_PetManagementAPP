import { useEffect, useState } from "react";
import { Button, Card, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading, fmtMoney } from "../components/Common";

// today's date as yyyy-MM-dd in local time (used as the minimum selectable date)
const todayStr = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
};

export default function BookAppointment() {
  const navigate = useNavigate();
  const [pets, setPets] = useState(null);
  const [services, setServices] = useState(null);
  const [form, setForm] = useState({ petId: "", groomingServiceId: "", appointmentDate: "", appointmentTime: "", notes: "" });
  const [validated, setValidated] = useState(false);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    Promise.all([api.get("/owner/pets"), api.get("/owner/services")])
      .then(([p, s]) => {
        setPets(p.data);
        setServices(s.data);
      })
      .catch((e) => setError(errMsg(e)));
  }, []);

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!e.currentTarget.checkValidity()) {
      setValidated(true);
      return;
    }
    setBusy(true);
    try {
      await api.post("/owner/appointments", {
        petId: Number(form.petId),
        groomingServiceId: Number(form.groomingServiceId),
        appointmentDate: form.appointmentDate,
        appointmentTime: form.appointmentTime + ":00",   // "HH:mm" -> "HH:mm:ss" for the API
        notes: form.notes || null,
      });
      navigate("/owner/appointments");
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setBusy(false);
    }
  };

  if (!pets || !services) return error ? <ErrorAlert message={error} /> : <Loading />;

  const chosen = services.find((s) => String(s.id) === form.groomingServiceId);

  return (
    <>
      <h2 className="mb-3">Book a Grooming Appointment</h2>
      <Card style={{ maxWidth: 600 }} className="shadow-sm">
        <Card.Body>
          <ErrorAlert message={error} />
          {pets.length === 0 && <ErrorAlert message="You need a registered pet before you can book." />}
          <Form noValidate validated={validated} onSubmit={onSubmit}>
            <Form.Group className="mb-3" controlId="pet">
              <Form.Label>Pet</Form.Label>
              <Form.Select required value={form.petId} onChange={set("petId")}>
                <option value="">Select a pet...</option>
                {pets.map((p) => <option key={p.id} value={p.id}>{p.petName} ({p.species})</option>)}
              </Form.Select>
              <Form.Control.Feedback type="invalid">Please select a pet.</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3" controlId="service">
              <Form.Label>Service</Form.Label>
              <Form.Select required value={form.groomingServiceId} onChange={set("groomingServiceId")}>
                <option value="">Select a service...</option>
                {services.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.serviceName} - {s.durationMinutes} min - {fmtMoney(s.price)}
                  </option>
                ))}
              </Form.Select>
              <Form.Control.Feedback type="invalid">Please select a service.</Form.Control.Feedback>
              {chosen && <Form.Text muted>{chosen.description}</Form.Text>}
            </Form.Group>

            <div className="row">
              <Form.Group className="mb-3 col-md-6" controlId="date">
                <Form.Label>Date</Form.Label>
                <Form.Control type="date" required min={todayStr()} value={form.appointmentDate} onChange={set("appointmentDate")} />
                <Form.Control.Feedback type="invalid">Choose today or a future date.</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3 col-md-6" controlId="time">
                <Form.Label>Time</Form.Label>
                <Form.Control type="time" required value={form.appointmentTime} onChange={set("appointmentTime")} />
                <Form.Control.Feedback type="invalid">Please choose a time.</Form.Control.Feedback>
              </Form.Group>
            </div>

            <Form.Group className="mb-3" controlId="notes">
              <Form.Label>Notes (optional)</Form.Label>
              <Form.Control as="textarea" rows={3} maxLength={500} value={form.notes} onChange={set("notes")} />
            </Form.Group>

            <Button type="submit" disabled={busy || pets.length === 0}>
              {busy ? "Booking..." : "Request appointment"}
            </Button>
            <p className="text-muted small mt-3 mb-0">
              A groomer will be assigned by the clinic after your request.
            </p>
          </Form>
        </Card.Body>
      </Card>
    </>
  );
}
