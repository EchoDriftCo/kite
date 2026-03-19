import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { CookingLogService } from '../../../services/cooking-log.service';
import { CookingLogEntry, CalendarDay } from '../../../models/cooking-log.model';

@Component({
  selector: 'app-cooking-history',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatChipsModule,
    MatTooltipModule,
    MatTabsModule
  ],
  templateUrl: './cooking-history.component.html',
  styleUrl: './cooking-history.component.scss'
})
export class CookingHistoryComponent implements OnInit {
  // List view
  entries: CookingLogEntry[] = [];
  loading = false;
  error = '';
  
  // Pagination
  totalCount = 0;
  pageSize = 20;
  pageNumber = 1;

  // Calendar view
  calendarLoading = false;
  calendarError = '';
  calendarDays: CalendarDay[] = [];
  currentYear = new Date().getFullYear();
  currentMonth = new Date().getMonth() + 1; // 1-indexed

  constructor(private cookingLogService: CookingLogService) {}

  ngOnInit() {
    this.loadHistory();
    this.loadCalendar();
  }

  loadHistory() {
    this.loading = true;
    this.error = '';

    this.cookingLogService.getHistory(this.pageNumber, this.pageSize).subscribe({
      next: (result) => {
        this.entries = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load cooking history';
        this.loading = false;
        console.error('Error loading history:', err);
      }
    });
  }

  loadCalendar() {
    this.calendarLoading = true;
    this.calendarError = '';

    this.cookingLogService.getCalendar(this.currentYear, this.currentMonth).subscribe({
      next: (result) => {
        this.calendarDays = result.days || [];
        this.calendarLoading = false;
      },
      error: (err) => {
        this.calendarError = err.message || 'Failed to load calendar';
        this.calendarLoading = false;
        console.error('Error loading calendar:', err);
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadHistory();
  }

  deleteEntry(entry: CookingLogEntry) {
    if (!confirm(`Delete this cooking log for "${entry.recipeTitle}"?`)) {
      return;
    }

    this.cookingLogService.deleteLog(entry.cookingLogId).subscribe({
      next: () => {
        this.loadHistory();
        this.loadCalendar(); // Refresh calendar too
      },
      error: (err) => {
        console.error('Error deleting log:', err);
        alert('Failed to delete log entry');
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  }

  previousMonth() {
    this.currentMonth--;
    if (this.currentMonth < 1) {
      this.currentMonth = 12;
      this.currentYear--;
    }
    this.loadCalendar();
  }

  nextMonth() {
    this.currentMonth++;
    if (this.currentMonth > 12) {
      this.currentMonth = 1;
      this.currentYear++;
    }
    this.loadCalendar();
  }

  getMonthName(): string {
    const date = new Date(this.currentYear, this.currentMonth - 1);
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  getCalendarGrid(): (CalendarDay | null)[][] {
    // Build a 7-column grid for calendar display
    const firstDay = new Date(this.currentYear, this.currentMonth - 1, 1);
    const lastDay = new Date(this.currentYear, this.currentMonth, 0);
    const startDayOfWeek = firstDay.getDay(); // 0 = Sunday
    const daysInMonth = lastDay.getDate();

    const grid: (CalendarDay | null)[][] = [];
    let week: (CalendarDay | null)[] = [];

    // Fill leading empty days
    for (let i = 0; i < startDayOfWeek; i++) {
      week.push(null);
    }

    // Fill actual days
    for (let day = 1; day <= daysInMonth; day++) {
      const dateStr = `${this.currentYear}-${String(this.currentMonth).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
      const calendarDay = this.calendarDays.find(d => d.date === dateStr);
      
      week.push(calendarDay || { date: dateStr, cookCount: 0, entries: [] });

      if (week.length === 7) {
        grid.push(week);
        week = [];
      }
    }

    // Fill trailing empty days
    while (week.length > 0 && week.length < 7) {
      week.push(null);
    }
    if (week.length > 0) {
      grid.push(week);
    }

    return grid;
  }

  getDayNumber(day: CalendarDay | null): number {
    if (!day) return 0;
    return parseInt(day.date.split('-')[2], 10);
  }

  getHeatmapClass(cookCount: number): string {
    if (cookCount === 0) return 'heat-0';
    if (cookCount === 1) return 'heat-1';
    if (cookCount === 2) return 'heat-2';
    return 'heat-3';
  }
}
