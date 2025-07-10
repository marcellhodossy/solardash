import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom';
import Login from './Login/Login.jsx';
import Resend from './Register/Resend.jsx';
import Register from './Register/Register.jsx';
import Verify from './Register/Verify.jsx';
import ForgotPassword from './Password/forgot_password.jsx';
import ChangePassword from './Password/change_password.jsx';
import MenuPage from './Users/Menu.jsx';
import Map from './Users/Map.jsx';
import ImageEditor from './Users/Editor.jsx';
import NotFound from './components/NotFound.jsx';

// Itt csatolod a root elemhez, pl. <div id="root"></div> az index.html-ben
const root = createRoot(document.getElementById('root'));

root.render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Navigate to="/" replace />} />
        <Route path="/register" element={<Register />} />
        <Route path="/register/resend" element={<Resend />} />
        <Route path="/register/verify" element={<Verify />} />
        <Route path="/forgot/password" element={<ForgotPassword />} />
        <Route path="/reset/password" element={<ChangePassword />} />
        <Route path="/users" element={<MenuPage />} />
        <Route path="/users/map" element={<Map />} />
        <Route path="/users/editor" element={<ImageEditor />} />

        {/* Fallback routes */}
        <Route path="/404" element={<NotFound />} />
        <Route path="*" element={<Navigate to="/404" replace />} />
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
