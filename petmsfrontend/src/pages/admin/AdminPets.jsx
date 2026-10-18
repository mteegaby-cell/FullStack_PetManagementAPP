import { useEffect, useState } from "react";
import api from "../../api";
import CrudPage from "../../components/CrudPage";
import { SPECIES, todayISO } from "../../../utils";

const columns = [
  { header: "Pet No.", cell: (r) => r.petNumber },
  { header: "Name", cell: (r) => r.petName },
  { header: "Species", cell: (r) => r.species },
  { header: "Breed", cell: (r) => r.breed },
  { header: "Gender", cell: (r) => r.gender },
  { header: "Date of Birth", cell: (r) => r.dateOfBirth },
  { header: "Weight", cell: (r) => `${r.weight} kg` },
  { header: "Owner", cell: (r) => r.ownerName }
];

const searchFields = [
  { name: "petNumber", label: "Pet number" },
  { name: "petName", label: "Pet name" },
  { name: "ownerName", label: "Owner name" }
];

export default function Pets() {
  const [owners, setOwners] = useState([]);
  useEffect(() => { api.get("/owners").then((r) => setOwners(r.data)).catch(() => {}); }, []);

  const fields = [
    { name: "petNumber", label: "Pet Number", required: true, check: (v) => (v.length > 20 ? "Maximum 20 characters." : "") },
    { name: "petName", label: "Pet Name", required: true },
    { name: "species", label: "Species", type: "select", required: true, options: SPECIES.map((s) => ({ value: s, label: s })) },
    { name: "breed", label: "Breed", required: true },
    { name: "gender", label: "Gender", type: "radio", required: true, options: [{ value: "Male", label: "Male" }, { value: "Female", label: "Female" }] },
    { name: "dateOfBirth", label: "Date of Birth", type: "date", required: true, check: (v) => (v > todayISO() ? "Date of birth cannot be in the future." : "") },
    { name: "colour", label: "Colour", required: true },
    {
      name: "weight", label: "Weight (kg)", type: "number", step: "0.01", required: true, numeric: true,
      check: (v) => (Number(v) > 0 && Number(v) <= 200 ? "" : "Weight must be between 0.01 and 200 kg.")
    },
    {
      name: "ownerId", label: "Owner", type: "select", required: true, numeric: true, full: true,
      options: owners.map((o) => ({ value: String(o.id), label: `${o.name} ${o.surname} (${o.ownerNumber})` }))
    },
    { name: "medicalNotes", label: "Medical / Allergy Notes", type: "textarea", full: true }
  ];

  return (
    <CrudPage title="Pets" singular="Pet" endpoint="/pets" columns={columns} fields={fields}
      searchFields={searchFields} rowLabel={(r) => `${r.petName} (${r.petNumber})`} />
  );
}
