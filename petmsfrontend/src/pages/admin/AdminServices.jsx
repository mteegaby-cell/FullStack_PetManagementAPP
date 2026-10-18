import { Badge } from "react-bootstrap";
import CrudPage from "../../components/CrudPage";
import { money, statusVariant } from "../../../utils";

const fields = [
  { name: "serviceName", label: "Service Name", required: true },
  {
    name: "status", label: "Service Status", type: "select", required: true, defaultValue: "Active",
    options: [{ value: "Active", label: "Active" }, { value: "Inactive", label: "Inactive" }]
  },
  { name: "description", label: "Description", type: "textarea", required: true, full: true },
  {
    name: "durationMinutes", label: "Duration (minutes)", type: "number", required: true, numeric: true,
    check: (v) => (Number.isInteger(Number(v)) && Number(v) >= 5 && Number(v) <= 480 ? "" : "Duration must be a whole number between 5 and 480.")
  },
  {
    name: "price", label: "Price (R)", type: "number", step: "0.01", required: true, numeric: true,
    check: (v) => (Number(v) >= 0 ? "" : "Price cannot be negative.")
  }
];

const columns = [
  { header: "Service", cell: (r) => r.serviceName },
  { header: "Description", cell: (r) => r.description },
  { header: "Duration", cell: (r) => `${r.durationMinutes} min` },
  { header: "Price", cell: (r) => money(r.price) },
  { header: "Status", cell: (r) => <Badge bg={statusVariant(r.status)}>{r.status}</Badge> }
];

const searchFields = [
  { name: "name", label: "Service name" },
  {
    name: "status", label: "Status", type: "select",
    options: [{ value: "", label: "All" }, { value: "Active", label: "Active" }, { value: "Inactive", label: "Inactive" }]
  }
];

export default function Services() {
  return (
    <CrudPage title="Grooming Services" singular="Service" endpoint="/services" columns={columns} fields={fields}
      searchFields={searchFields} rowLabel={(r) => r.serviceName} />
  );
}
