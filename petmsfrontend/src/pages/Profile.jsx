import { useEffect, useState } from "react";
import { Card, ListGroup } from "react-bootstrap";
import api, { errMsg } from "../api";
import { ErrorAlert, Loading } from "../components/Common";

export default function Profile() {
  const [me, setMe] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.get("/owner/me").then((r) => setMe(r.data)).catch((e) => setError(errMsg(e)));
  }, []);

  if (error) return <ErrorAlert message={error} />;
  if (!me) return <Loading />;

  const rows = [
    ["Owner number", me.ownerNumber],
    ["Name", me.name],
    ["Surname", me.surname],
    ["Email address", me.email],
    ["Phone number", me.phoneNumber],
    ["Home address", me.homeAddress],
  ];

  return (
    <>
      <h2 className="mb-3">My Profile</h2>
      <Card style={{ maxWidth: 560 }} className="shadow-sm">
        <ListGroup variant="flush">
          {rows.map(([label, value]) => (
            <ListGroup.Item key={label} className="d-flex justify-content-between">
              <span className="text-muted">{label}</span>
              <strong>{value}</strong>
            </ListGroup.Item>
          ))}
        </ListGroup>
      </Card>
    </>
  );
}
