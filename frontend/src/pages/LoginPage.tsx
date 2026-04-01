import { useState, useRef, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../services/auth';
import type { ApiError } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './LoginPage.scss';

const RETRYABLE_STATUSES = [0, 502, 503, 504];
const MAX_RETRIES = 3;
const RETRY_DELAY_MS = 10_000;

function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export function LoginPage() {
  const year = new Date().getFullYear();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRetrying, setIsRetrying] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const abortRef = useRef(false);
  const { refresh } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    abortRef.current = false;
    return () => {
      abortRef.current = true;
    };
  }, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (isSubmitting) return;

    const trimmedUsername = username.trim();
    if (!trimmedUsername || !password) {
      setErrorMessage('Enter both username and password.');
      return;
    }

    setIsSubmitting(true);
    setIsRetrying(false);
    setErrorMessage('');

    let lastError: ApiError | null = null;

    try {
      for (let attempt = 0; attempt <= MAX_RETRIES; attempt++) {
        if (abortRef.current) return;
        try {
          await login({ username: trimmedUsername, password });
          refresh();
          navigate('/dashboard');
          return;
        } catch (err) {
          const apiErr = err as ApiError;
          lastError = apiErr;
          const status = apiErr.status ?? 0;
          if (attempt < MAX_RETRIES && RETRYABLE_STATUSES.includes(status)) {
            if (!abortRef.current) setIsRetrying(true);
            await sleep((attempt + 1) * RETRY_DELAY_MS);
          } else {
            break;
          }
        }
      }

      if (!abortRef.current) {
        const status = lastError?.status ?? 0;
        if (RETRYABLE_STATUSES.includes(status)) {
          setErrorMessage('The server could not be reached. Please try again later.');
        } else {
          setErrorMessage(
            (lastError?.body as Record<string, string> | null)?.['message'] ??
            lastError?.message ??
            'Sign in failed. Please try again.',
          );
        }
      }
    } finally {
      if (!abortRef.current) {
        setIsSubmitting(false);
        setIsRetrying(false);
      }
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <span className="login-logo">&#9685;</span>
          <h1>MyTourDataViewer</h1>
          <p className="login-subtitle">Sign in to your account</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              name="username"
              placeholder="Enter your username"
              autoComplete="username"
              value={username}
              onChange={e => setUsername(e.target.value)}
              disabled={isSubmitting}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              name="password"
              placeholder="Enter your password"
              autoComplete="current-password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              disabled={isSubmitting}
            />
          </div>

          {isRetrying && (
            <p className="login-info" aria-live="polite">
              Server is starting up. Please wait&hellip;
            </p>
          )}

          {errorMessage && <p className="login-error">{errorMessage}</p>}

          <button type="submit" className="btn-signin" disabled={isSubmitting}>
            {isRetrying ? 'Waiting for server...' : isSubmitting ? 'Signing In...' : 'Sign In'}
          </button>
        </form>

        <p className="login-footer">MyTourDataViewer &copy; {year}</p>
      </div>
    </div>
  );
}
