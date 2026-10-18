// Meal plan dates are calendar days, but the API returns them as UTC timestamps
// (`2026-10-19T00:00:00Z`). `new Date()` on those lands on the previous day
// anywhere west of UTC, so read and write the YYYY-MM-DD part in local time.

/** Parses the leading YYYY-MM-DD of a date or timestamp string as local midnight. */
export function parseCalendarDate(value: string): Date {
  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
  if (!match) return new Date(value);
  return new Date(+match[1], +match[2] - 1, +match[3]);
}

/** Formats a local date as YYYY-MM-DD. */
export function formatCalendarDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** Whole calendar days from `from` to `to` (DST-safe). */
export function daysBetween(from: Date, to: Date): number {
  const utc = (d: Date) => Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());
  return Math.round((utc(to) - utc(from)) / 86_400_000);
}

/** Returns a new date `days` calendar days after `date`. */
export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}
