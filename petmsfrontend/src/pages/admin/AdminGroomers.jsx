import CrudPage from "../../components/CrudPage";
import { SPECIALIZATIONS, isEmail, isPhone, isStrongPassword } from "../../../utils";

const fields = [
  { name: "staffNumber", label: "Staff Number", required: true, check: (v) => (v.length > 20 ? "Maximum 20 characters." : "") },
  { name: "name", label: "Name", required: true },
  { name: "surname", label: "Surname", required: true },
  { name: "email", label: "Email Address", type: "email", required: true, check: (v) => (isEmail(v) ? "" : "Enter a valid email address.") },
  { name: "phoneNumber", label: "Phone Number", type: "tel", required: true, check: (v) => (isPhone(v) ? "" : "Enter a valid phone number (10-15 digits).") },
  { name: "specialization", label: "Specialization", type: "select", required: true, options: SPECIALIZATIONS.map((s) => ({ value: s, label: s })) },
  {
    name: "password", label: "Login Password", type: "password", required: true, onlyOnCreate: true, full: true,
    help: "The groomer uses their email + this password to log in. Min 8 chars with upper, lower, digit and symbol.",
    check: (v) => (isStrongPassword(v) ? "" : "Min 8 characters with upper, lower, digit and symbol.")
  }
];

const columns = [
  { header: "Staff No.", cell: (r) => r.staffNumber },
  { header: "Name", cell: (r) => r.name },
  { header: "Surname", cell: (r) => r.surname },
  { header: "Email", cell: (r) => r.email },
  { header: "Phone", cell: (r) => r.phoneNumber },
  { header: "Specialization", cell: (r) => r.specialization }
];

const searchFields = [
  { name: "staffNumber", label: "Staff number" },
  { name: "name", label: "Name" },
  { name: "surname", label: "Surname" }
];

export default function Groomers() {
  return (
    <CrudPage title="Groomers" singular="Groomer" endpoint="/groomers" columns={columns} fields={fields}
      searchFields={searchFields} rowLabel={(r) => `${r.name} ${r.surname}`} />
  );
}
