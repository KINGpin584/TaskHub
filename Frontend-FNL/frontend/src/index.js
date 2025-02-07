import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { BrowserRouter } from 'react-router-dom';
import { RecoilRoot } from 'recoil';
import { AuthProvider } from './context/AuthContext'; // Import AuthProvider

const root = ReactDOM.createRoot(document.getElementById('root'));

root.render(
  <React.StrictMode>
    <BrowserRouter>
      <RecoilRoot>
        <AuthProvider> {/* Wrap the App with AuthProvider */}
          <App />
        </AuthProvider>
      </RecoilRoot>
    </BrowserRouter>
  </React.StrictMode>
);
