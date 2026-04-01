import { NavLink, Outlet, Navigate } from 'react-router-dom';
import './DashboardPage.scss';

export function DashboardPage() {
  return (
    <div className="dashboard-container">
      <h2>Dashboard</h2>

      <nav className="dashboard-subnav">
        <NavLink to="overview">Overview</NavLink>
        <NavLink to="search-requests">Request Search</NavLink>
      </nav>

      <Outlet />
    </div>
  );
}

/** Redirect /dashboard → /dashboard/overview */
export function DashboardIndexRedirect() {
  return <Navigate to="overview" replace />;
}
