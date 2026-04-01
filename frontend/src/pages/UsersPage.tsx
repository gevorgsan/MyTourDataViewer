import { useState, useEffect, type FormEvent } from 'react';
import { getAll, create, changePassword } from '../services/user.service';
import type { User, CreateUserRequest } from '../types/models';
import './UsersPage.scss';

type LoadState = 'idle' | 'loading' | 'success' | 'error';
type FormState = 'idle' | 'submitting' | 'success' | 'error';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$/;
const ROLES = ['Administrator', 'Viewer'];

function emptyForm(): CreateUserRequest {
  return { username: '', email: '', fullName: '', password: '', role: 'Viewer' };
}

export function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [state, setState] = useState<LoadState>('idle');
  const [errorMessage, setErrorMessage] = useState('');

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [formState, setFormState] = useState<FormState>('idle');
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [form, setForm] = useState<CreateUserRequest>(emptyForm());
  const [formTouched, setFormTouched] = useState(false);

  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [passwordModalUser, setPasswordModalUser] = useState<User | null>(null);
  const [passwordModalState, setPasswordModalState] = useState<FormState>('idle');
  const [passwordModalError, setPasswordModalError] = useState('');
  const [passwordModalSuccess, setPasswordModalSuccess] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  function loadUsers() {
    setState('loading');
    setErrorMessage('');
    getAll()
      .then(data => {
        setUsers(data);
        setState('success');
      })
      .catch((err: { body?: Record<string, string>; message?: string }) => {
        setState('error');
        setErrorMessage(err?.body?.['message'] ?? err?.message ?? 'Failed to load users.');
      });
  }

  useEffect(() => {
    loadUsers();
  }, []);

  function toggleCreateForm() {
    setShowCreateForm(v => {
      if (v) {
        setForm(emptyForm());
        setFormState('idle');
        setFormError('');
        setFormSuccess('');
        setFormTouched(false);
      }
      return !v;
    });
  }

  function submitCreate(e: FormEvent) {
    e.preventDefault();
    setFormTouched(true);

    if (
      !form.username.trim() ||
      !form.email.trim() ||
      !form.fullName.trim() ||
      !form.password ||
      !form.role
    ) return;

    if (!PASSWORD_PATTERN.test(form.password)) return;

    setFormState('submitting');
    setFormError('');
    setFormSuccess('');

    create(form)
      .then(() => {
        setFormState('success');
        setFormSuccess(`User "${form.username}" created successfully.`);
        setForm(emptyForm());
        setFormTouched(false);
        loadUsers();
      })
      .catch((err: { body?: Record<string, string>; message?: string }) => {
        setFormState('error');
        setFormError(err?.body?.['message'] ?? err?.message ?? 'Failed to create user.');
      });
  }

  function openPasswordModal(user: User) {
    setPasswordModalUser(user);
    setNewPassword('');
    setConfirmPassword('');
    setPasswordModalState('idle');
    setPasswordModalError('');
    setPasswordModalSuccess('');
    setShowPasswordModal(true);
  }

  function closePasswordModal() {
    if (passwordModalState === 'submitting') return;
    setShowPasswordModal(false);
    setPasswordModalUser(null);
  }

  function submitChangePassword(e: FormEvent) {
    e.preventDefault();

    if (newPassword !== confirmPassword) {
      setPasswordModalError('Passwords do not match.');
      return;
    }

    if (!PASSWORD_PATTERN.test(newPassword)) {
      setPasswordModalError(
        'Password must be at least 6 characters with uppercase, lowercase, digit, and special character.',
      );
      return;
    }

    setPasswordModalState('submitting');
    setPasswordModalError('');
    setPasswordModalSuccess('');

    changePassword(passwordModalUser!.id, newPassword)
      .then(() => {
        setPasswordModalState('success');
        setPasswordModalSuccess(
          `Password for "${passwordModalUser!.username}" changed successfully.`,
        );
        setNewPassword('');
        setConfirmPassword('');
      })
      .catch((err: { body?: Record<string, string>; message?: string }) => {
        setPasswordModalState('error');
        setPasswordModalError(
          err?.body?.['message'] ?? err?.message ?? 'Failed to change password.',
        );
      });
  }

  function isAdministrator(user: User) {
    return user.roles.some(r => r === 'Administrator');
  }

  function roleLabel(user: User) {
    return user.roles.join(', ') || 'No role';
  }

  return (
    <div className="users-container">
      <div className="users-header">
        <div>
          <h2>User Management</h2>
          <p>Users with the Administrator role are labeled and highlighted below.</p>
        </div>
        <button className="btn-create" type="button" onClick={toggleCreateForm}>
          {showCreateForm ? 'Cancel' : '+ Create User'}
        </button>
      </div>

      {showCreateForm && (
        <div className="create-form-wrapper">
          <h3>Create New User</h3>
          {formState === 'success' && (
            <p className="form-msg success">{formSuccess}</p>
          )}
          {formState === 'error' && (
            <p className="form-msg error">{formError}</p>
          )}
          <form className="create-form" onSubmit={submitCreate} noValidate>
            <div className="form-row">
              <label htmlFor="cu-username">Username *</label>
              <input
                id="cu-username"
                type="text"
                value={form.username}
                onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
                placeholder="e.g. johndoe"
                disabled={formState === 'submitting'}
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="cu-email">Email *</label>
              <input
                id="cu-email"
                type="email"
                value={form.email}
                onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                placeholder="e.g. john@example.com"
                disabled={formState === 'submitting'}
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="cu-fullName">Full Name *</label>
              <input
                id="cu-fullName"
                type="text"
                value={form.fullName}
                onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))}
                placeholder="e.g. John Doe"
                disabled={formState === 'submitting'}
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="cu-password">Password *</label>
              <input
                id="cu-password"
                type="password"
                value={form.password}
                onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                placeholder="Min 6 chars, uppercase, lowercase, digit, special"
                disabled={formState === 'submitting'}
                required
              />
              {formTouched && form.password && !PASSWORD_PATTERN.test(form.password) && (
                <span className="field-error">
                  Min 6 chars, one uppercase, one lowercase, one digit, one special character.
                </span>
              )}
            </div>
            <div className="form-row">
              <label htmlFor="cu-role">Role *</label>
              <select
                id="cu-role"
                value={form.role}
                onChange={e => setForm(f => ({ ...f, role: e.target.value }))}
                disabled={formState === 'submitting'}
                required
              >
                {ROLES.map(r => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
            </div>
            <button
              className="btn-submit"
              type="submit"
              disabled={formState === 'submitting'}
            >
              {formState === 'submitting' ? 'Creating...' : 'Create User'}
            </button>
          </form>
        </div>
      )}

      {state === 'loading' && <p className="state-msg">Loading users...</p>}
      {state === 'error' && <p className="state-msg error">{errorMessage}</p>}

      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Username</th>
              <th>Full Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Active</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {state === 'success' && users.length === 0 && (
              <tr>
                <td colSpan={6} className="empty-row">
                  No users found.
                </td>
              </tr>
            )}
            {users.map(user => (
              <tr key={user.id} className={isAdministrator(user) ? 'row-admin' : undefined}>
                <td>
                  <div className="username-cell">
                    <span>{user.username}</span>
                    {isAdministrator(user) && (
                      <span className="role-badge">Administrator</span>
                    )}
                  </div>
                </td>
                <td>{user.fullName}</td>
                <td>{user.email}</td>
                <td>{roleLabel(user)}</td>
                <td>
                  <span className={user.isActive ? 'active-text' : 'inactive-text'}>
                    {user.isActive ? 'Active' : 'Disabled'}
                  </span>
                </td>
                <td className="actions-cell">
                  <button className="btn-action" type="button" onClick={() => openPasswordModal(user)}>
                    Change Password
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Change Password Modal */}
      {showPasswordModal && (
        <>
          <div className="modal-backdrop" onClick={closePasswordModal} />
          <div
            className="modal"
            role="dialog"
            aria-modal="true"
            aria-label={`Change password for ${passwordModalUser?.username}`}
          >
            <div className="modal-header">
              <h3>Change Password</h3>
              <button
                className="modal-close"
                type="button"
                onClick={closePasswordModal}
                aria-label="Close"
              >
                &times;
              </button>
            </div>
            <p className="modal-subtitle">
              User: <strong>{passwordModalUser?.username}</strong>
            </p>

            {passwordModalState === 'success' && (
              <p className="form-msg success">{passwordModalSuccess}</p>
            )}
            {(passwordModalState === 'error' || passwordModalError) && (
              <p className="form-msg error">{passwordModalError}</p>
            )}

            <form className="modal-form" onSubmit={submitChangePassword} noValidate>
              <div className="form-row">
                <label htmlFor="pw-new">New Password *</label>
                <input
                  id="pw-new"
                  type="password"
                  value={newPassword}
                  onChange={e => setNewPassword(e.target.value)}
                  placeholder="Min 6 chars, uppercase, lowercase, digit, special"
                  aria-describedby="pw-hint"
                  disabled={passwordModalState === 'submitting'}
                  required
                />
                <span id="pw-hint" className="field-hint">
                  At least 6 characters, one uppercase letter, one lowercase letter, one digit, and
                  one special character.
                </span>
              </div>
              <div className="form-row">
                <label htmlFor="pw-confirm">Confirm Password *</label>
                <input
                  id="pw-confirm"
                  type="password"
                  value={confirmPassword}
                  onChange={e => setConfirmPassword(e.target.value)}
                  disabled={passwordModalState === 'submitting'}
                  required
                />
              </div>
              <div className="modal-footer">
                <button
                  className="btn-cancel"
                  type="button"
                  onClick={closePasswordModal}
                  disabled={passwordModalState === 'submitting'}
                >
                  Cancel
                </button>
                <button
                  className="btn-submit"
                  type="submit"
                  disabled={passwordModalState === 'submitting'}
                >
                  {passwordModalState === 'submitting' ? 'Saving...' : 'Save Password'}
                </button>
              </div>
            </form>
          </div>
        </>
      )}
    </div>
  );
}
