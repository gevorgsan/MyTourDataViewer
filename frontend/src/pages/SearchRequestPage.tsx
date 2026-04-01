import { useState, useEffect, useCallback, type FormEvent } from 'react';
import { search, getHistory } from '../services/search-request.service';
import type { SearchRequestItem, RequestHistoryItem } from '../types/models';
import { formatDate, formatDateMedium } from '../utils/date';
import './SearchRequestPage.scss';

type LoadState = 'idle' | 'loading' | 'success' | 'error';

function extractErrorMessage(err: unknown): string {
  const fallback = 'Failed to retrieve search results.';
  const e = err as Record<string, unknown> | null;
  if (!e) return fallback;

  const errorObj = e['error'];
  if (typeof errorObj === 'object' && errorObj !== null) {
    const msg = (errorObj as Record<string, unknown>)['message'];
    if (typeof msg === 'string' && msg.trim()) return msg;
  }
  if (typeof e['message'] === 'string' && (e['message'] as string).trim()) {
    return e['message'] as string;
  }
  return fallback;
}

export function SearchRequestPage() {
  const [createdFrom, setCreatedFrom] = useState('');
  const [createdTo, setCreatedTo] = useState('');
  const [requestChanels, setRequestChanels] = useState('');
  const [requestStatus, setRequestStatus] = useState('');
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  const [searchState, setSearchState] = useState<LoadState>('idle');
  const [results, setResults] = useState<SearchRequestItem[]>([]);
  const [errorMessage, setErrorMessage] = useState('');

  const [selectedRequestId, setSelectedRequestId] = useState<number | null>(null);
  const [historyModalOpen, setHistoryModalOpen] = useState(false);
  const [historyState, setHistoryState] = useState<LoadState>('idle');
  const [history, setHistory] = useState<RequestHistoryItem[]>([]);
  const [historyErrorMessage, setHistoryErrorMessage] = useState('');
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  const closeHistory = useCallback(() => setHistoryModalOpen(false), []);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape' && historyModalOpen) {
        closeHistory();
      }
    }
    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, [historyModalOpen, closeHistory]);

  function markAllTouched() {
    setTouched({ createdFrom: true, createdTo: true });
  }

  function isInvalid(field: string): boolean {
    if (!touched[field]) return false;
    if (field === 'createdFrom') return !createdFrom;
    if (field === 'createdTo') return !createdTo;
    return false;
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    markAllTouched();
    if (!createdFrom || !createdTo) return;

    setSearchState('loading');
    setResults([]);
    setErrorMessage('');
    setSelectedRequestId(null);
    setHistory([]);
    setHistoryState('idle');

    search({
      createdFrom,
      createdTo,
      requestChanels: requestChanels !== '' ? Number(requestChanels) : null,
      requestStatus: requestStatus !== '' ? Number(requestStatus) : null,
    })
      .then(items => {
        setResults(items);
        setSearchState('success');
      })
      .catch(err => {
        setSearchState('error');
        setErrorMessage(extractErrorMessage(err));
      });
  }

  function selectRequest(item: SearchRequestItem) {
    setHistoryModalOpen(true);
    if (selectedRequestId === item.id) return;

    setSelectedRequestId(item.id);
    setHistory([]);
    setExpandedRows(new Set());
    setHistoryState('loading');
    setHistoryErrorMessage('');

    getHistory(item.id)
      .then(records => {
        setHistory(records);
        setHistoryState('success');
      })
      .catch(err => {
        setHistoryState('error');
        setHistoryErrorMessage(extractErrorMessage(err));
      });
  }

  function toggleRow(index: number) {
    setExpandedRows(prev => {
      const next = new Set(prev);
      if (next.has(index)) {
        next.delete(index);
      } else {
        next.add(index);
      }
      return next;
    });
  }

  return (
    <div className="search-request-container">
      <h2>Search Requests</h2>

      <form className="search-form" onSubmit={handleSubmit} noValidate>
        <div className="form-row two-col">
          <div className="form-group">
            <label htmlFor="createdFrom">
              Created From <span className="required">*</span>
            </label>
            <input
              id="createdFrom"
              type="date"
              value={createdFrom}
              onChange={e => setCreatedFrom(e.target.value)}
              onBlur={() => setTouched(t => ({ ...t, createdFrom: true }))}
              className={isInvalid('createdFrom') ? 'invalid' : undefined}
            />
            {isInvalid('createdFrom') && (
              <span className="field-error">Start date is required.</span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="createdTo">
              Created To <span className="required">*</span>
            </label>
            <input
              id="createdTo"
              type="date"
              value={createdTo}
              onChange={e => setCreatedTo(e.target.value)}
              onBlur={() => setTouched(t => ({ ...t, createdTo: true }))}
              className={isInvalid('createdTo') ? 'invalid' : undefined}
            />
            {isInvalid('createdTo') && (
              <span className="field-error">End date is required.</span>
            )}
          </div>
        </div>

        <div className="form-row two-col">
          <div className="form-group">
            <label htmlFor="requestChanels">
              Request Channel <span className="optional">(optional)</span>
            </label>
            <input
              id="requestChanels"
              type="number"
              value={requestChanels}
              onChange={e => setRequestChanels(e.target.value)}
              placeholder="e.g. 1"
              min="0"
            />
          </div>

          <div className="form-group">
            <label htmlFor="requestStatus">
              Request Status <span className="optional">(optional)</span>
            </label>
            <input
              id="requestStatus"
              type="number"
              value={requestStatus}
              onChange={e => setRequestStatus(e.target.value)}
              placeholder="e.g. 2"
              min="0"
            />
          </div>
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={searchState === 'loading'}>
            {searchState === 'loading' ? 'Searching\u2026' : 'Search'}
          </button>
        </div>
      </form>

      {searchState === 'loading' && <div className="state-msg">Fetching results\u2026</div>}

      {searchState === 'error' && (
        <div className="state-msg error">
          <strong>Error:</strong> {errorMessage}
        </div>
      )}

      {searchState === 'success' && (
        <>
          <div className="results-header">
            <span className="badge success">&#10004; {results.length} result(s) found</span>
            {selectedRequestId != null && (
              <span className="badge info">Viewing history for Request #{selectedRequestId}</span>
            )}
          </div>

          {results.length === 0 ? (
            <p className="empty">No results match the given criteria.</p>
          ) : (
            <div className="table-wrapper">
              <table className="results-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Offer ID</th>
                    <th>Price</th>
                    <th>Currency</th>
                    <th>Start Date</th>
                    <th>End Date</th>
                    <th>Status</th>
                    <th>Travelers</th>
                    <th>Created Date</th>
                    <th>History</th>
                  </tr>
                </thead>
                <tbody>
                  {results.map(item => (
                    <tr
                      key={item.id}
                      className={item.id === selectedRequestId ? 'selected-row' : undefined}
                    >
                      <td>{item.id}</td>
                      <td>{item.offerId ?? '—'}</td>
                      <td>{item.price != null ? item.price : '—'}</td>
                      <td>{item.currency ?? '—'}</td>
                      <td>{formatDate(item.startDate)}</td>
                      <td>{formatDate(item.endDate)}</td>
                      <td>{item.status != null ? item.status : '—'}</td>
                      <td>{item.travelers?.length ?? 0}</td>
                      <td>{formatDate(item.createdDate)}</td>
                      <td>
                        <button
                          className={`btn btn-sm btn-outline${item.id === selectedRequestId ? ' active' : ''}`}
                          onClick={() => selectRequest(item)}
                        >
                          View History
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      {/* History modal */}
      {historyModalOpen && selectedRequestId != null && (
        <div className="history-modal-backdrop" onClick={closeHistory}>
          <div
            className="history-modal"
            role="dialog"
            aria-modal="true"
            aria-label={`Request History #${selectedRequestId}`}
            onClick={e => e.stopPropagation()}
          >
            <div className="history-modal-header">
              <h3>Request History &mdash; #{selectedRequestId}</h3>
              <button
                className="btn btn-sm btn-outline close-btn"
                onClick={closeHistory}
                aria-label="Close"
              >
                &#10005;
              </button>
            </div>

            <div className="history-modal-body">
              {historyState === 'loading' && (
                <div className="state-msg">Loading history\u2026</div>
              )}

              {historyState === 'error' && (
                <div className="state-msg error">
                  <strong>Error:</strong> {historyErrorMessage}
                </div>
              )}

              {historyState === 'success' && (
                <>
                  {history.length === 0 ? (
                    <p className="empty">No history records found.</p>
                  ) : (
                    history.map((record, i) => (
                      <div key={i} className="history-record">
                        <div
                          className="history-record-header"
                          onClick={() => toggleRow(i)}
                        >
                          <div className="history-record-meta">
                            <span className="badge change-type">
                              {record.changeType ?? '—'}
                            </span>
                            <span className="changed-at">
                              {formatDateMedium(record.changedAt)}
                            </span>
                            <span className="changed-by">
                              by <strong>{record.changedBy ?? '—'}</strong>
                            </span>
                          </div>
                          <button className="btn btn-sm btn-outline toggle-btn">
                            {expandedRows.has(i) ? '▲ Collapse' : '▼ Expand'}
                          </button>
                        </div>

                        {expandedRows.has(i) && (
                          <div className="history-record-body">
                            <div className="values-comparison">
                              <div className="values-column">
                                <h4>Old Values</h4>
                                {!record.oldValues ? (
                                  <div className="empty-values">—</div>
                                ) : (
                                  <table className="values-table">
                                    <tbody>
                                      {Object.keys(record.oldValues).map(key => (
                                        <tr key={key}>
                                          <td className="value-key">{key}</td>
                                          <td className="value-val">
                                            {String(record.oldValues![key] ?? '—')}
                                          </td>
                                        </tr>
                                      ))}
                                    </tbody>
                                  </table>
                                )}
                              </div>
                              <div className="values-column">
                                <h4>New Values</h4>
                                {!record.newValues ? (
                                  <div className="empty-values">—</div>
                                ) : (
                                  <table className="values-table">
                                    <tbody>
                                      {Object.keys(record.newValues).map(key => (
                                        <tr key={key}>
                                          <td className="value-key">{key}</td>
                                          <td className="value-val">
                                            {String(record.newValues![key] ?? '—')}
                                          </td>
                                        </tr>
                                      ))}
                                    </tbody>
                                  </table>
                                )}
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    ))
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
