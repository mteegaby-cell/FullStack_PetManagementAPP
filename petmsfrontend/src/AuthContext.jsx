
import { createContext, useContext, useState } from "react";
import api from "./api";

const AuthCtx = createContext(null);

export const useAuth = () => useContext(AuthCtx);

// Each role lands on its own dashboard
export const homeFor = (roles = []) => {
  const normalizedRoles = roles.map((role) => role.toUpperCase());

  if (normalizedRoles.includes("ADMIN")) return "/admin";
  if (normalizedRoles.includes("GROOMER")) return "/groomer";

  return "/owner";
};

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(() =>
    JSON.parse(localStorage.getItem("auth") || "null")
  );

  const login = async (email, password) => {
    const { data } = await api.post("/auth/login", {
      email,
      password,
    });

    localStorage.setItem("auth", JSON.stringify(data));
    setAuth(data);

    return data;
  };

  const logout = () => {
    localStorage.removeItem("auth");
    setAuth(null);
  };

  return (
    <AuthCtx.Provider value={{ auth, login, logout }}>
      {children}
    </AuthCtx.Provider>
  );
}
