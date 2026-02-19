import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FeedbackService } from '../../../core/services/feedback.service';

@Component({
  selector: 'app-feedback-button',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule],
  template: `
    @if (feedbackService.isEnabled()) {
      <button 
        mat-fab 
        color="accent" 
        class="feedback-fab"
        matTooltip="Report a bug"
        (click)="openFeedback()">
        <mat-icon>bug_report</mat-icon>
      </button>
    }
  `,
  styles: [`
    .feedback-fab {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
    }
  `]
})
export class FeedbackButtonComponent {
  constructor(public feedbackService: FeedbackService) {}

  openFeedback(): void {
    this.feedbackService.openFeedbackDialog();
  }
}
