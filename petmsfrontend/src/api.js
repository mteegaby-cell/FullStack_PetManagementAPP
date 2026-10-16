// All communication with the ASP.NET Core Web API goes through this one axios instance.
import axios from "axios";

const api = axios.create({
  baseURL: (import.meta.env.VITE_API_URL ?? "http://localhost:5228") + "/api",
});

// Attach the JWT to every request: "Authorization: Bearer <token>"
api.interceptors.request.use((config) => {
  const auth = JSON.parse(localStorage.getItem("auth") || "null");
  if (auth?.token) config.headers.Authorization = `Bearer ${auth.token}`;
  return config;
});

// If the token has expired or is invalid, send the user back to login
api.interceptors.response.use(
  (res) => res,
  (err) => {
    const isAuthCall = err.config?.url?.includes("/auth/");
    if (err.response?.status === 401 && !isAuthCall) {
      localStorage.removeItem("auth");
      window.location.href = "/login";
    }
    return Promise.reject(err);
  }
);

// Pull a readable message out of an API error
export function errMsg(e) {
  const d = e.response?.data;
  if (d?.message) return d.message;
  if (d?.errors) return Object.values(d.errors).flat().join(" ");
  if (!e.response) return "Cannot reach the server. Is the API running?";
  return "Something went wrong. Please try again.";
}

export default api;
