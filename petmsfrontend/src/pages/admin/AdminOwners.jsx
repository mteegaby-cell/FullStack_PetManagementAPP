import CrudPage from "../../components/CrudPage";
import { isEmail, isPhone, isStrongPassword } from "../../../utils";

const fields = [
  { name: "ownerNumber", label: "Owner Number", required: true, check: (v) => (v.length > 20 ? "Maximum 20 characters." : "") },
  { name: "name", label: "Name", required: true },
  { name: "surname", label: "Surname", required: true },
  { name: "email", label: "Email Address", type: "email", required: true, check: (v) => (isEmail(v) ? "" : "Enter a valid email address.") },
  { name: "phoneNumber", label: "Phone Number", type: "tel", required: true, check: (v) => (isPhone(v) ? "" : "Enter a valid phone number (10-15 digits).") },
  { name: "homeAddress", label: "Home Address", required: true, full: true },
  {
    name: "password", label: "Login Password", type: "password", required: true, onlyOnCreate: true,
    help: "The owner uses their email + this password to log in. Min 8 chars with upper, lower, digit and symbol.",
    check: (v) => (isStrongPassword(v) ? "" : "Min 8 characters with upper, lower, digit and symbol.")
  }
];

const columns = [
  { header: "Owner No.", cell: (r) => r.ownerNumber },
  { header: "Name", cell: (r) => r.name },
  { header: "Surname", cell: (r) => r.surname },
  { header: "Email", cell: (r) => r.email },
  { header: "Phone", cell: (r) => r.phoneNumber },
  { header: "Pets", cell: (r) => r.petCount }
];

const searchFields = [
  { name: "ownerNumber", label: "Owner number" },
  { name: "name", label: "Name" },
  { name: "surname", label: "Surname" }
];

export default function Owners() {
  return (
    <CrudPage title="Pet Owners" singular="Owner" endpoint="/owners" columns={columns} fields={fields}
      searchFields={searchFields} rowLabel={(r) => `${r.name} ${r.surname}`} />
  );
}
