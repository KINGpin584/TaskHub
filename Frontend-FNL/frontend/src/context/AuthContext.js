// AuthContext.js
import React, { createContext, useContext } from "react";
import { useRecoilState } from "recoil";
import { userState } from "../recoil/atom";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  // Use the Recoil atom that persists to localStorage
  const [user, setUser] = useRecoilState(userState);
  const isAuthenticated = Boolean(user);

  // Login: store the user data
  const login = (userData) => {
    setUser(userData);
  };

  // Logout: clear the user state (and localStorage via your effect)
  const logout = () => {
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
