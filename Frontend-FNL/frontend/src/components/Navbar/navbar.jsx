import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext'

function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className="fixed top-0 w-full z-50 bg-black text-white flex justify-between items-center px-4 py-2">
      {/* Logo on the left */}
      <div className="text-lg font-bold">
        <Link to="/">Logo</Link>
      </div>

      {/* Right side buttons */}
      <div>
        {user ? (
          <button
            onClick={logout}
            className="border border-white bg-black text-white px-4 py-2 mr-2 hover:bg-white hover:text-black hover:border-black transition-colors"
          >
            Logout
          </button>
        ) : (
          <>
            <Link to="/login">
              <button className="border border-white bg-black text-white px-4 py-2 mr-2 hover:bg-white hover:text-black hover:border-black transition-colors">
                Login
              </button>
            </Link>
            <Link to="/signup">
              <button className="border border-white bg-black text-white px-4 py-2 hover:bg-white hover:text-black hover:border-black transition-colors">
                Signup
              </button>
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}

export default Navbar;
