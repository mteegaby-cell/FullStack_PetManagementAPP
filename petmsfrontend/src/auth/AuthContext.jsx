import { createContext, useContext, useState } from "react";
import api from "../api";

const KEY = "pawscare_auth";
const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

function loadStored() {
  try {
    const a = JSON.parse(localStorage.getItem(KEY));
    if (a && new Date(a.expiresAt) > new Date()) return a;
  } catch { /* ignore */ }
  localStorage.removeItem(KEY);
  return null;
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStored); // { token, email, fullName, role, expiresAt }

  const login = async (email, password) => {
    const { data } = await api.post("/auth/login", { email, password });
    localStorage.setItem(KEY, JSON.stringify(data));
    setUser(data);
    return data;
  };

  const logout = () => {
    localStorage.removeItem(KEY);
    setUser(null);
  };

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>;
}
