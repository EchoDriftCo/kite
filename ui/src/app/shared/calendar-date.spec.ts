import { addDays, daysBetween, formatCalendarDate, parseCalendarDate } from './calendar-date';

describe('calendar-date', () => {
  it('reads a UTC-midnight timestamp as that calendar day', () => {
    const date = parseCalendarDate('2026-10-19T00:00:00Z');
    expect([date.getFullYear(), date.getMonth(), date.getDate()]).toEqual([2026, 9, 19]);
  });

  it('reads a bare date as that calendar day', () => {
    expect(formatCalendarDate(parseCalendarDate('2026-10-19'))).toBe('2026-10-19');
  });

  it('counts calendar days across a DST change', () => {
    expect(daysBetween(parseCalendarDate('2026-10-26'), parseCalendarDate('2026-11-09'))).toBe(14);
    expect(formatCalendarDate(addDays(parseCalendarDate('2026-10-31'), 2))).toBe('2026-11-02');
  });
});
