import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminGuard } from './components/AdminGuard';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage, DashboardIndexRedirect } from './pages/DashboardPage';
import { OverviewPage } from './pages/OverviewPage';
import { SearchRequestPage } from './pages/SearchRequestPage';
import { UsersPage } from './pages/UsersPage';
import { ApiSettingsPage } from './pages/ApiSettingsPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />

          {/* Authenticated routes wrapped in Layout (Navbar + main) */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/dashboard" element={<DashboardPage />}>
                <Route index element={<DashboardIndexRedirect />} />
                <Route path="overview" element={<OverviewPage />} />
                <Route path="search-requests" element={<SearchRequestPage />} />
              </Route>

              {/* Admin-only routes */}
              <Route element={<AdminGuard />}>
                <Route path="/admin/users" element={<UsersPage />} />
                <Route path="/admin/api-settings" element={<ApiSettingsPage />} />
              </Route>
            </Route>
          </Route>

          {/* Fallback */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
