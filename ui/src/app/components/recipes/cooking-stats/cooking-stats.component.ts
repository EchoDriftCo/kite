import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { CookingLogService } from '../../../services/cooking-log.service';
import { CookingStats } from '../../../models/cooking-log.model';

@Component({
  selector: 'app-cooking-stats',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './cooking-stats.component.html',
  styleUrl: './cooking-stats.component.scss'
})
export class CookingStatsComponent implements OnInit {
  stats: CookingStats | null = null;
  loading = false;
  error = '';

  constructor(private cookingLogService: CookingLogService) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading = true;
    this.error = '';

    this.cookingLogService.getStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load cooking stats';
        this.loading = false;
        console.error('Error loading stats:', err);
      }
    });
  }

  getStreakEmoji(streak: number): string {
    if (streak === 0) return '';
    if (streak < 3) return '🔥';
    if (streak < 7) return '🔥🔥';
    if (streak < 14) return '🔥🔥🔥';
    return '🔥🔥🔥🔥';
  }

  getDayAbbreviation(dayName: string): string {
    const map: { [key: string]: string } = {
      'Monday': 'Mon',
      'Tuesday': 'Tue',
      'Wednesday': 'Wed',
      'Thursday': 'Thu',
      'Friday': 'Fri',
      'Saturday': 'Sat',
      'Sunday': 'Sun'
    };
    return map[dayName] || dayName;
  }
}
