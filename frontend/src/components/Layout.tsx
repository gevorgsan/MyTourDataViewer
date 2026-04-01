import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';
import { useAuth } from '../context/AuthContext';

export function Layout() {
  const { authenticated } = useAuth();
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      {authenticated && <Navbar />}
      <main className={`main-content${authenticated ? '' : ' main-content--guest'}`}>
        <Outlet />
      </main>
    </div>
  );
}
