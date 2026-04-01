import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/** Redirects unauthenticated users to /login. */
export function ProtectedRoute() {
  const { authenticated } = useAuth();
  return authenticated ? <Outlet /> : <Navigate to="/login" replace />;
}
