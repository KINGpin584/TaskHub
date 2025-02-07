import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

function Signup() {
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleSignup = async (e) => {
    e.preventDefault();
    setError(null); // Reset error before making request

    try {
        const response = await axios.post('http://localhost:5087/api/users/register', {
            email,
            username,
            password,
        }, {
            headers: {
                'Content-Type': 'application/json'
            }
        });
        

      console.log('Signup Successful:', response.data);

      // Redirect user after successful signup
      navigate('/login'); // Redirect to login page after signup
    } catch (err) {
      console.error('Signup Error:', err.response?.data || 'An error occurred');
      setError(err.response?.data || 'Error signing up');
    }
  };

  return (
    <div className="flex justify-center items-center h-screen w-screen bg-gray-900">
      <div className="bg-white p-8 rounded-lg shadow-lg w-96 relative">
        {/* Close (✖) button */}
        <button
          onClick={() => navigate('/')}
          className="absolute top-2 right-2 text-gray-500 hover:text-black text-xl"
        >
          ✖
        </button>

        {/* Left-aligned Signup Heading */}
        <h1 className="text-2xl font-bold mb-6">Signup</h1>

        {error && <p className="text-red-600 mb-4">{error}</p>}

        <form className="flex flex-col" onSubmit={handleSignup}>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="p-2 mb-4 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
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
            Signup
          </button>
        </form>
      </div>
    </div>
  );
}

export default Signup;
