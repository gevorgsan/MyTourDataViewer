import React, { createContext, useContext, useState, useCallback } from 'react';
import { isAuthenticated, isAdmin, removeToken } from '../services/auth';

interface AuthContextType {
  authenticated: boolean;
  admin: boolean;
  /** Re-read auth state from localStorage (call after a successful login). */
  refresh: () => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  authenticated: false,
  admin: false,
  refresh: () => undefined,
  logout: () => undefined,
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [authenticated, setAuthenticated] = useState(() => isAuthenticated());
  const [admin, setAdmin] = useState(() => isAdmin());

  const refresh = useCallback(() => {
    setAuthenticated(isAuthenticated());
    setAdmin(isAdmin());
  }, []);

  const logout = useCallback(() => {
    removeToken();
    setAuthenticated(false);
    setAdmin(false);
  }, []);

  return (
    <AuthContext.Provider value={{ authenticated, admin, refresh, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  return useContext(AuthContext);
}
