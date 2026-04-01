/** Format a date string as short (e.g. "1/1/2026, 12:00 PM"). */
export function formatDate(dateStr: string | undefined | null): string {
  if (!dateStr) return '—';
  try {
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
    }).format(new Date(dateStr));
  } catch {
    return dateStr;
  }
}

/** Format a date string as medium (e.g. "Jan 1, 2026, 12:00:00 PM"). */
export function formatDateMedium(dateStr: string | undefined | null): string {
  if (!dateStr) return '—';
  try {
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      second: 'numeric',
    }).format(new Date(dateStr));
  } catch {
    return dateStr;
  }
}
