import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Navbar.scss';

export function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);
  const { admin, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    setMenuOpen(false);
    logout();
    navigate('/login');
  }

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <span className="brand-icon">&#9685;</span>
        <NavLink className="brand-name" to="/dashboard">
          MyTourDataViewer
        </NavLink>
      </div>

      <button
        className={`nav-toggle${menuOpen ? ' open' : ''}`}
        onClick={() => setMenuOpen(v => !v)}
        aria-label="Toggle navigation"
      >
        <span />
        <span />
        <span />
      </button>

      <div className={`nav-links${menuOpen ? ' open' : ''}`} onClick={() => setMenuOpen(false)}>
        <NavLink to="/dashboard" end={false}>
          Dashboard
        </NavLink>
        {admin && (
          <>
            <NavLink to="/admin/users">Users</NavLink>
            <NavLink to="/admin/api-settings">API Settings</NavLink>
          </>
        )}
        <button type="button" className="btn-logout" onClick={handleLogout}>
          Logout
        </button>
      </div>
    </nav>
  );
}
