import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDark = signal(false);

  constructor() {
    const saved = localStorage.getItem('rv-theme');
    if (saved) {
      this.setDark(saved === 'dark');
    } else {
      // Respect OS preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.setDark(prefersDark);
    }

    // Listen for OS theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      if (!localStorage.getItem('rv-theme')) {
        this.setDark(e.matches);
      }
    });
  }

  toggle(): void {
    this.setDark(!this.isDark());
  }

  private setDark(dark: boolean): void {
    this.isDark.set(dark);
    document.body.classList.toggle('dark-theme', dark);
    localStorage.setItem('rv-theme', dark ? 'dark' : 'light');
  }
}
