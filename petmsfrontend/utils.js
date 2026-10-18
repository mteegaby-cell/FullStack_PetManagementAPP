export const APPOINTMENT_STATUSES = ["Scheduled", "CheckedIn", "InProgress", "Completed", "Cancelled"];
export const SPECIES = ["Dog", "Cat", "Rabbit", "Bird", "Other"];
export const SPECIALIZATIONS = ["Dog Grooming", "Cat Grooming", "Bathing & Drying", "Nail Care", "Breed-Specific Grooming"];

export const statusLabel = (s) => ({ CheckedIn: "Checked In", InProgress: "In Progress" }[s] ?? s);
export const statusVariant = (s) =>
  ({ Scheduled: "primary", CheckedIn: "info", InProgress: "warning", Completed: "success", Cancelled: "secondary", Active: "success", Inactive: "secondary" }[s] ?? "dark");

export const hhmm = (t) => (t ?? "").slice(0, 5);
export const money = (n) => `R ${Number(n).toFixed(2)}`;
export const todayISO = () => new Date().toLocaleDateString("en-CA"); // YYYY-MM-DD in local time

export const homeFor = (role) => ({ Admin: "/admin", Groomer: "/groomer", PetOwner: "/owner" }[role] ?? "/login");

// ---- validation rules (mirror the backend's rules) ----
export const isEmail = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
export const isPhone = (v) => /^\+?[0-9\s-]{10,15}$/.test(v);
export const isStrongPassword = (v) => v.length >= 8 && /[a-z]/.test(v) && /[A-Z]/.test(v) && /\d/.test(v) && /[^a-zA-Z0-9]/.test(v);

/** Declarative validation used by CrudPage. field.check(value, form, isEditing) returns an error message or "". */
export function validateForm(fields, form, isEditing) {
  const errors = {};
  for (const f of fields) {
    if (f.onlyOnCreate && isEditing) continue;
    const raw = form[f.name];
    const value = raw == null ? "" : String(raw).trim();
    if (f.required && !value) {
      errors[f.name] = `${f.label} is required.`;
      continue;
    }
    if (value && f.check) {
      const msg = f.check(value, form, isEditing);
      if (msg) errors[f.name] = msg;
    }
  }
  return errors;
}

// ---- turn API errors into readable text ----
export function getError(err) {
  const d = err?.response?.data;
  if (d?.message) return d.message;
  if (d?.errors) return Object.values(d.errors).flat().join(" ");
  if (err?.code === "ERR_NETWORK") return "Cannot reach the server. Is the API running and is VITE_API_URL correct?";
  return err?.message ?? "Something went wrong.";
}

// ASP.NET validation errors look like { errors: { OwnerNumber: ["..."] } } -> { ownerNumber: "..." }
export function serverFieldErrors(err) {
  const e = err?.response?.data?.errors;
  if (!e) return {};
  return Object.fromEntries(Object.entries(e).map(([k, v]) => [k.charAt(0).toLowerCase() + k.slice(1), v[0]]));
}
