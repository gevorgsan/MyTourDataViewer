import { useState, useEffect, type FormEvent } from 'react';
import {
  getAll,
  create,
  update,
  deleteSettings,
  testConnection,
} from '../services/api-settings.service';
import type {
  ApiSettings,
  CreateApiSettingsRequest,
  UpdateApiSettingsRequest,
  TestConnectionResponse,
} from '../types/models';
import './ApiSettingsPage.scss';

type AuthorizationType = 'None' | 'Bearer' | 'ApiKey' | 'Basic';

interface FormValues {
  name: string;
  tokenUrl: string;
  authorizationType: AuthorizationType;
  credentialsEmail: string;
  credentialsPassword: string;
  timeoutSeconds: number;
  isActive: boolean;
}

function defaultForm(): FormValues {
  return {
    name: '',
    tokenUrl: '',
    authorizationType: 'None',
    credentialsEmail: '',
    credentialsPassword: '',
    timeoutSeconds: 30,
    isActive: true,
  };
}

export function ApiSettingsPage() {
  const [items, setItems] = useState<ApiSettings[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<FormValues>(defaultForm());
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [testResult, setTestResult] = useState<TestConnectionResponse | null>(null);
  const [testingId, setTestingId] = useState<number | null>(null);

  function load() {
    getAll()
      .then(data => setItems(data))
      .catch(() => setError('Failed to load API settings.'));
  }

  useEffect(() => {
    load();
  }, []);

  function openCreate() {
    setEditingId(null);
    setForm(defaultForm());
    setShowForm(true);
    setTestResult(null);
    setError('');
  }

  function openEdit(item: ApiSettings) {
    setEditingId(item.id);
    setForm({
      name: item.name,
      tokenUrl: item.tokenUrl ?? '',
      authorizationType: item.authorizationType ?? 'None',
      credentialsEmail: item.credentialsEmail ?? '',
      credentialsPassword: '',
      timeoutSeconds: item.timeoutSeconds,
      isActive: item.isActive,
    });
    setShowForm(true);
    setTestResult(null);
    setError('');
  }

  function cancel() {
    setShowForm(false);
    setEditingId(null);
    setTestResult(null);
    setError('');
  }

  function handleSave(e: FormEvent) {
    e.preventDefault();
    if (!form.name.trim()) return;
    setSaving(true);
    setError('');

    if (editingId == null) {
      const req: CreateApiSettingsRequest = {
        name: form.name,
        tokenUrl: form.tokenUrl || undefined,
        authorizationType: form.authorizationType || undefined,
        credentialsEmail: form.credentialsEmail || undefined,
        credentialsPassword: form.credentialsPassword || undefined,
        timeoutSeconds: form.timeoutSeconds,
      };
      create(req)
        .then(() => { setSaving(false); setShowForm(false); load(); })
        .catch(() => { setSaving(false); setError('Failed to create.'); });
    } else {
      const req: UpdateApiSettingsRequest = {
        name: form.name,
        tokenUrl: form.tokenUrl || undefined,
        authorizationType: form.authorizationType || undefined,
        credentialsEmail: form.credentialsPassword ? (form.credentialsEmail || undefined) : undefined,
        credentialsPassword: form.credentialsPassword || undefined,
        timeoutSeconds: form.timeoutSeconds,
        isActive: form.isActive,
      };
      update(editingId, req)
        .then(() => { setSaving(false); setShowForm(false); load(); })
        .catch(() => { setSaving(false); setError('Failed to update.'); });
    }
  }

  function handleDelete(item: ApiSettings) {
    if (!confirm(`Delete "${item.name}"?`)) return;
    deleteSettings(item.id)
      .then(() => load())
      .catch(() => setError('Failed to delete.'));
  }

  function handleTestConnection(item: ApiSettings) {
    setTestingId(item.id);
    setTestResult(null);
    testConnection({ apiSettingsId: item.id })
      .then(result => { setTestResult(result); setTestingId(null); })
      .catch(() => {
        setTestResult({ success: false, message: 'Request failed.' });
        setTestingId(null);
      });
  }

  function setField<K extends keyof FormValues>(key: K, value: FormValues[K]) {
    setForm(f => ({ ...f, [key]: value }));
  }

  return (
    <div className="api-settings-container">
      <div className="header">
        <h2>External API Authorization Settings</h2>
        <button className="btn btn-primary" onClick={openCreate}>
          + Add Authorization
        </button>
      </div>

      <p className="hint">
        Use this section to store and manage external API authorization settings only.
      </p>

      {error && <p className="error">{error}</p>}

      {!showForm && (
        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Auth Type</th>
                <th>Token URL</th>
                <th>Timeout (s)</th>
                <th>Active</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map(item => (
                <tr key={item.id}>
                  <td>{item.name}</td>
                  <td>{item.authorizationType}</td>
                  <td>{item.tokenUrl || '-'}</td>
                  <td>{item.timeoutSeconds}</td>
                  <td>{item.isActive ? 'Yes' : 'No'}</td>
                  <td className="actions">
                    <button className="btn btn-sm" onClick={() => openEdit(item)}>
                      Edit
                    </button>
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => handleTestConnection(item)}
                      disabled={testingId === item.id}
                    >
                      {testingId === item.id ? 'Testing\u2026' : 'Test'}
                    </button>
                    <button className="btn btn-sm btn-danger" onClick={() => handleDelete(item)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr>
                  <td colSpan={6} className="empty">
                    No API configurations yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {testResult && (
        <div className={`test-result${testResult.success ? ' success' : ' failure'}`}>
          <strong>{testResult.success ? '✔ Connected' : '✘ Failed'}</strong>
          {testResult.statusCode && <span> &mdash; HTTP {testResult.statusCode}</span>}
          {testResult.message && <span> &mdash; {testResult.message}</span>}
          <button className="close" onClick={() => setTestResult(null)}>
            &times;
          </button>
        </div>
      )}

      {showForm && (
        <form className="settings-form" onSubmit={handleSave}>
          <h3>
            {editingId != null
              ? 'Edit Authorization Configuration'
              : 'New Authorization Configuration'}
          </h3>

          <div className="form-row">
            <label>Name *</label>
            <input
              type="text"
              value={form.name}
              onChange={e => setField('name', e.target.value)}
              placeholder="My External API"
              required
            />
          </div>

          <div className="form-row">
            <label>Authorization Type</label>
            <select
              value={form.authorizationType}
              onChange={e => setField('authorizationType', e.target.value as AuthorizationType)}
            >
              <option value="None">None</option>
              <option value="Bearer">Bearer</option>
              <option value="ApiKey">API Key</option>
              <option value="Basic">Basic</option>
            </select>
          </div>

          <div className="form-row">
            <label>Token URL</label>
            <input
              type="url"
              value={form.tokenUrl}
              onChange={e => setField('tokenUrl', e.target.value)}
              placeholder="https://api.example.com/auth/login"
            />
          </div>

          <div className="form-row">
            <label>Email</label>
            <input
              type="text"
              value={form.credentialsEmail}
              onChange={e => setField('credentialsEmail', e.target.value)}
              placeholder="user@example.com"
            />
          </div>

          <div className="form-row">
            <label>Password</label>
            <input
              type="password"
              value={form.credentialsPassword}
              onChange={e => setField('credentialsPassword', e.target.value)}
              placeholder={
                editingId != null ? 'Leave blank to keep existing password' : 'Password'
              }
            />
          </div>

          <div className="form-row">
            <label>Timeout (seconds)</label>
            <input
              type="number"
              value={form.timeoutSeconds}
              onChange={e => setField('timeoutSeconds', Number(e.target.value))}
              min="0"
              max="300"
            />
          </div>

          {editingId != null && (
            <div className="form-row form-row--checkbox">
              <label>
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={e => setField('isActive', e.target.checked)}
                />
                &nbsp;Active
              </label>
            </div>
          )}

          {error && <p className="error">{error}</p>}

          <div className="form-actions">
            <button
              type="submit"
              className="btn btn-primary"
              disabled={!form.name.trim() || saving}
            >
              {saving ? 'Saving\u2026' : editingId != null ? 'Update' : 'Create'}
            </button>
            <button type="button" className="btn" onClick={cancel}>
              Cancel
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
