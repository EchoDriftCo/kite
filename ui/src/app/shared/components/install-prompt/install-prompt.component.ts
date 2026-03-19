import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Platform } from '@angular/cdk/platform';

@Component({
  selector: 'app-install-prompt',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    @if (showPrompt) {
      <div class="install-banner">
        <mat-icon>install_mobile</mat-icon>
        <span class="install-text">Install RecipeVault for quick access</span>
        <button mat-raised-button color="primary" (click)="installApp()">
          Install
        </button>
        <button mat-icon-button (click)="dismiss()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
    }
  `,
  styles: [`
    .install-banner {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      background-color: #334155;
      color: #f1f5f9;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);

      .install-text {
        flex: 1;
        font-size: 14px;
        word-wrap: break-word;
        overflow-wrap: break-word;
      }

      button[mat-raised-button] {
        flex-shrink: 0;
      }
    }

    @media (max-width: 600px) {
      .install-banner {
        flex-wrap: wrap;
        gap: 8px;
        padding: 10px 12px;

        .install-text {
          flex-basis: calc(100% - 48px);
          font-size: 13px;
        }
      }
    }
  `]
})
export class InstallPromptComponent implements OnInit, OnDestroy {
  showPrompt = false;
  private deferredPrompt: any;
  private boundHandler = (e: Event) => this.onBeforeInstallPrompt(e);

  constructor(private platform: Platform) {}

  ngOnInit(): void {
    if (!this.platform.isBrowser) return;

    window.addEventListener('beforeinstallprompt', this.boundHandler);

    window.addEventListener('appinstalled', () => {
      this.showPrompt = false;
      this.deferredPrompt = null;
    });
  }

  ngOnDestroy(): void {
    window.removeEventListener('beforeinstallprompt', this.boundHandler);
  }

  private onBeforeInstallPrompt(e: Event): void {
    e.preventDefault();
    this.deferredPrompt = e;
    const dismissed = localStorage.getItem('pwa-install-dismissed');
    if (!dismissed) {
      this.showPrompt = true;
    }
  }

  async installApp(): Promise<void> {
    if (!this.deferredPrompt) return;

    this.deferredPrompt.prompt();
    const { outcome } = await this.deferredPrompt.userChoice;
    if (outcome === 'accepted') {
      this.showPrompt = false;
    }
    this.deferredPrompt = null;
  }

  dismiss(): void {
    this.showPrompt = false;
    localStorage.setItem('pwa-install-dismissed', 'true');
  }
}
