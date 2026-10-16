import { Alert, Badge, Spinner } from "react-bootstrap";

export function Loading() {
  return (
    <div className="text-center my-5">
      <Spinner animation="border" />
    </div>
  );
}

export function ErrorAlert({ message }) {
  return message ? <Alert variant="danger">{message}</Alert> : null;
}

const colours = {
  Scheduled: "primary",
  CheckedIn: "info",
  InProgress: "warning",
  Completed: "success",
  Cancelled: "secondary",
};

export function StatusBadge({ status }) {
  const label = status.replace(/([a-z])([A-Z])/g, "$1 $2"); // CheckedIn -> Checked In
  return <Badge bg={colours[status] ?? "dark"}>{label}</Badge>;
}

export const STATUSES = ["Scheduled", "CheckedIn", "InProgress", "Completed", "Cancelled"];
export const fmtTime = (t) => (t ? t.slice(0, 5) : "");
export const fmtMoney = (n) => `R${Number(n).toFixed(2)}`;
export const fmtDate = (d) =>
  new Date(d + "T00:00:00").toLocaleDateString("en-ZA", { day: "numeric", month: "short", year: "numeric" });
