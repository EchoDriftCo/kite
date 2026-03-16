import { Component, AfterViewChecked, ElementRef, HostListener, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TourService } from '../../../services/tour.service';

@Component({
  selector: 'app-tour-tooltip',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    @if (tourService.stepReady && tourService.currentStep; as step) {
      <div class="tour-backdrop" (click)="tourService.end()"></div>
      <div
        class="tour-tooltip"
        [class.mobile]="tourService.isMobile"
        [ngStyle]="tooltipStyle"
      >
        <div class="tour-title">
          <mat-icon class="tour-title-icon">ads_click</mat-icon>
          {{ step.title }}
        </div>
        <div class="tour-message">{{ step.message }}</div>
        <div class="tour-action-hint">{{ step.actionHint }}</div>
        <div class="tour-actions">
          <button mat-button (click)="tourService.end()">Skip Tour</button>
          <button mat-raised-button color="primary" (click)="onNext()">
            @if (tourService.isLastStep) {
              Finish Tour
            } @else {
              Next ({{ tourService.stepLabel }})
            }
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .tour-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      z-index: 9998;
    }

    .tour-tooltip {
      position: fixed;
      background: #1e293b;
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
      max-width: 400px;
      z-index: 9999;

      &.mobile {
        max-width: calc(100vw - 40px);
        padding: 20px;
      }
    }

    .tour-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 20px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #f59e0b;
    }

    .tour-title-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .tour-message {
      font-size: 15px;
      line-height: 1.5;
      margin-bottom: 16px;
      white-space: pre-line;
      color: #cbd5e1;
    }

    .tour-action-hint {
      font-size: 14px;
      font-style: italic;
      color: #64748b;
      margin-bottom: 20px;
    }

    .tour-actions {
      display: flex;
      justify-content: space-between;
      gap: 12px;

      button {
        min-height: 48px;
      }
    }
  `]
})
export class TourTooltipComponent implements AfterViewChecked, OnDestroy {
  tooltipStyle: { [key: string]: string } = {};
  private lastStepIndex = -1;
  private resizeListener: (() => void) | null = null;

  constructor(
    public tourService: TourService,
    private elRef: ElementRef
  ) {}

  ngAfterViewChecked(): void {
    if (this.tourService.currentStepIndex !== this.lastStepIndex) {
      this.lastStepIndex = this.tourService.currentStepIndex;
      // Delay to allow DOM to render the tooltip
      setTimeout(() => this.updatePosition(), 50);
    }
  }

  ngOnDestroy(): void {
    if (this.resizeListener) {
      window.removeEventListener('resize', this.resizeListener);
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    this.updatePosition();
  }

  async onNext(): Promise<void> {
    await this.tourService.next();
  }

  private updatePosition(): void {
    const el = this.elRef.nativeElement.querySelector('.tour-tooltip') as HTMLElement;
    if (!el) return;

    const rect = el.getBoundingClientRect();
    const pos = this.tourService.calculateTooltipPosition(rect.width, rect.height);

    this.tooltipStyle = {
      top: `${pos.top}px`,
      left: `${pos.left}px`
    };
  }
}
