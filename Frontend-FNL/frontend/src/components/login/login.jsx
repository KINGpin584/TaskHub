import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSetRecoilState } from "recoil";
import axios from "axios";
import { userState } from "../../recoil/atom";
import { useAuth } from "../../context/AuthContext";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError(null);

    try {
      const response = await axios.post("http://localhost:5087/api/users/login", {
        username,
        password,
      });

      const userData = response.data;
      console.log("Login Successful:", userData);

      // Store user in Recoil state
      login(userData);
      // Redirect to dashboard
      navigate("/dashboard");
    } catch (err) {
      console.error("Login Error:", err.response?.data || "An error occurred");
      setError(err.response?.data || "Invalid credentials");
    }
  };

  return (
    <div className="flex justify-center items-center h-screen w-screen bg-gray-900">
      <div className="bg-white p-8 rounded-lg shadow-lg w-96 relative">
        <button
          onClick={() => navigate("/")}
          className="absolute top-3 right-3 text-gray-600 hover:text-gray-900 text-xl"
        >
          &times;
        </button>

        <h1 className="text-2xl font-bold mb-6 text-left">Login</h1>

        {error && <p className="text-red-600 mb-4">{error}</p>}

        <form className="flex flex-col" onSubmit={handleLogin}>
          <input
            type="text"
            placeholder="Username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="p-2 mb-4 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="p-2 mb-4 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button
            type="submit"
            className="bg-blue-600 text-white py-2 rounded hover:bg-blue-700 transition"
          >
            Login
          </button>
        </form>
      </div>
    </div>
  );
}

export default Login;
