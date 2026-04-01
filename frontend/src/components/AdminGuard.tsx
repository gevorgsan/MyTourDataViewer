import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/** Must be nested inside ProtectedRoute. Redirects non-admin users to /dashboard. */
export function AdminGuard() {
  const { admin } = useAuth();
  return admin ? <Outlet /> : <Navigate to="/dashboard" replace />;
}
