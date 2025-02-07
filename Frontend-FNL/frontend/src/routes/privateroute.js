import React from "react";
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; // ✅ FIX: Use `useAuth` instead of `AuthContext`

const PrivateRoute = () => {
  const { isAuthenticated } = useAuth(); // ✅ FIX: Use `useAuth()` hook instead

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" />;
};

export default PrivateRoute;
