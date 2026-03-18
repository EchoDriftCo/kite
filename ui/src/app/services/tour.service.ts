import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { OnboardingService } from './onboarding.service';

export interface TourStep {
  recipeResourceId?: string;
  elementSelector: string;
  title: string;
  message: string;
  actionHint: string;
  showcases: string;
}

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private steps: TourStep[] = [
    {
      elementSelector: '[data-tour="serving-selector"]',
      title: 'Recipe Scaling',
      message: 'Change serving sizes and watch ingredient quantities adjust automatically. Perfect for cooking for 2 or feeding a crowd.',
      actionHint: 'Try it: Click a different serving size',
      showcases: 'recipe-scaling'
    },
    {
      elementSelector: '[data-tour="cooking-mode-button"]',
      title: 'Cooking Mode',
      message: 'Step-by-step view with built-in timers. Keep your screen awake while you cook.\n\nThis recipe has 3 timers ready to go:\n\u2022 Simmer: 5 min\n\u2022 Poach eggs: 6 min\n\u2022 Rest: 2 min',
      actionHint: 'Try it: Click Start Cooking',
      showcases: 'cooking-mode'
    },
    {
      elementSelector: '[data-tour="substitution-button"]',
      title: 'Ingredient Substitutions',
      message: 'Out of an ingredient? We\'ve got alternatives ready.\n\nThis recipe has substitutions pre-configured:\n\u2022 Fish sauce \u2192 Soy sauce + lime\n\u2022 Thai basil \u2192 Regular basil\n\u2022 Bird\'s eye chili \u2192 Red pepper flakes',
      actionHint: 'Try it: Click Substitutes',
      showcases: 'ingredient-substitutions'
    },
    {
      elementSelector: '[data-tour="fork-indicator"]',
      title: 'Recipe Forking',
      message: 'Found a great recipe but want to tweak it? Fork it to create your own version while keeping the original.\n\nYour changes don\'t affect the original recipe.',
      actionHint: 'Try it: Open any recipe and click Fork Recipe',
      showcases: 'recipe-forking'
    },
    {
      elementSelector: '[data-tour="add-recipe-button"]',
      title: 'Multiple Import Options',
      message: 'Add recipes from anywhere:\n\n\u2022 Paste any recipe URL\n\u2022 Import from Paprika\n\u2022 Create manually\n\u2022 Photo import (coming soon)',
      actionHint: 'Try it: Click + to add a recipe',
      showcases: 'import-options'
    }
  ];

  currentStepIndex = -1;
  active = false;
  stepReady = false;
  isMobile = false;

  // Store sample recipe resource IDs for navigation
  private sampleRecipeIds: Map<string, string> = new Map();

  constructor(
    private router: Router,
    private breakpointObserver: BreakpointObserver,
    private onboardingService: OnboardingService
  ) {
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe(result => {
        this.isMobile = result.matches;
      });
  }

  setSampleRecipeIds(recipes: { recipeResourceId: string; showcases: string }[]): void {
    this.sampleRecipeIds.clear();
    for (const recipe of recipes) {
      this.sampleRecipeIds.set(recipe.showcases, recipe.recipeResourceId);
    }

    // Map tour steps to their sample recipe IDs
    for (const step of this.steps) {
      const recipeId = this.sampleRecipeIds.get(step.showcases);
      if (recipeId) {
        step.recipeResourceId = recipeId;
      }
    }

    // Persist to localStorage so the tour can be restarted in a new session
    try {
      localStorage.setItem('tour_sample_recipes', JSON.stringify(recipes));
    } catch {
      // Ignore storage errors
    }
  }

  private restoreSampleRecipeIds(): boolean {
    try {
      const stored = localStorage.getItem('tour_sample_recipes');
      if (stored) {
        const recipes = JSON.parse(stored) as { recipeResourceId: string; showcases: string }[];
        if (recipes.length > 0) {
          this.setSampleRecipeIds(recipes);
          return true;
        }
      }
    } catch {
      // Ignore parse errors
    }
    return false;
  }

  async start(): Promise<void> {
    // Don't restart the tour if the user previously skipped or completed it
    if (this.wasSkippedOrCompleted()) {
      return;
    }

    // Ensure sample recipe IDs are available for navigation
    if (this.sampleRecipeIds.size === 0) {
      this.restoreSampleRecipeIds();
    }

    this.currentStepIndex = 0;
    this.active = true;
    this.stepReady = false;
    await this.navigateToStep();
  }

  /** Force-start the tour regardless of skip state (e.g. from Settings). */
  async forceStart(): Promise<void> {
    this.clearSkipState();

    if (this.sampleRecipeIds.size === 0) {
      this.restoreSampleRecipeIds();
    }

    this.currentStepIndex = 0;
    this.active = true;
    this.stepReady = false;
    await this.navigateToStep();
  }

  async next(): Promise<void> {
    this.stepReady = false;
    this.currentStepIndex++;
    if (this.currentStepIndex >= this.steps.length) {
      this.end();
    } else {
      await this.navigateToStep();
    }
  }

  end(): void {
    this.currentStepIndex = -1;
    this.active = false;
    this.stepReady = false;
    this.persistSkipState();
    this.onboardingService.updateProgress({ tourCompleted: true }).subscribe();
  }

  private persistSkipState(): void {
    try {
      localStorage.setItem('tour_skipped', 'true');
    } catch {
      // Ignore storage errors
    }
  }

  private wasSkippedOrCompleted(): boolean {
    try {
      return localStorage.getItem('tour_skipped') === 'true';
    } catch {
      return false;
    }
  }

  private clearSkipState(): void {
    try {
      localStorage.removeItem('tour_skipped');
    } catch {
      // Ignore storage errors
    }
  }

  get currentStep(): TourStep | null {
    return this.active ? this.steps[this.currentStepIndex] ?? null : null;
  }

  get stepLabel(): string {
    return `${this.currentStepIndex + 1}/${this.steps.length}`;
  }

  get isLastStep(): boolean {
    return this.currentStepIndex === this.steps.length - 1;
  }

  private async navigateToStep(): Promise<void> {
    const step = this.currentStep;
    if (!step) return;

    if (step.recipeResourceId) {
      await this.router.navigate(['/recipes', step.recipeResourceId]);
    } else if (step.showcases === 'import-options') {
      await this.router.navigate(['/recipes']);
    }

    // Bail out if tour was ended during navigation
    if (!this.active) return;

    // Give the routed component a moment to mount and begin rendering
    await new Promise(resolve => setTimeout(resolve, 50));

    // Poll for target element instead of a fixed timeout
    const found = await this.waitForElement(step.elementSelector, 5000);

    // Bail out if tour was ended while waiting
    if (!this.active) return;

    if (!found) {
      // Skip if target element not found
      if (this.currentStepIndex < this.steps.length - 1) {
        this.currentStepIndex++;
        await this.navigateToStep();
      } else {
        this.end();
      }
      return;
    }

    // Scroll target into view so the tooltip can position relative to it
    const targetEl = document.querySelector(step.elementSelector) as HTMLElement;
    if (targetEl) {
      targetEl.scrollIntoView({ behavior: 'instant', block: 'center' });
      // Allow one frame for scroll and layout to settle
      await new Promise(resolve => requestAnimationFrame(resolve));
    }

    this.stepReady = true;
  }

  private waitForElement(selector: string, timeoutMs: number): Promise<boolean> {
    return new Promise(resolve => {
      const isReady = (): boolean => {
        const el = document.querySelector(selector) as HTMLElement;
        if (!el) return false;
        const rect = el.getBoundingClientRect();
        return rect.width > 0 && rect.height > 0;
      };

      if (isReady()) {
        resolve(true);
        return;
      }

      const interval = 100;
      let elapsed = 0;
      const timer = setInterval(() => {
        elapsed += interval;
        if (isReady()) {
          clearInterval(timer);
          resolve(true);
        } else if (elapsed >= timeoutMs) {
          clearInterval(timer);
          resolve(false);
        }
      }, interval);
    });
  }

  calculateTooltipPosition(tooltipWidth: number, tooltipHeight: number): {
    top: number;
    left: number;
    placement: string;
  } {
    const step = this.currentStep;
    if (!step) return { top: 0, left: 0, placement: 'center-bottom' };

    // Mobile: always center-bottom card
    if (this.isMobile) {
      return {
        top: window.innerHeight - tooltipHeight - 20,
        left: (window.innerWidth - tooltipWidth) / 2,
        placement: 'center-bottom'
      };
    }

    // Desktop: collision-detected positioning
    const targetEl = document.querySelector(step.elementSelector) as HTMLElement;
    if (!targetEl) return { top: 100, left: 100, placement: 'below' };

    const rect = targetEl.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const margin = 12;

    // Preferred: below
    let top = rect.bottom + margin;
    let left = rect.left;
    let placement = 'below';

    // Check bottom overflow
    if (top + tooltipHeight > viewportHeight) {
      // Try above
      top = rect.top - tooltipHeight - margin;
      placement = 'above';

      // Still overflows? Try right
      if (top < 0) {
        top = rect.top;
        left = rect.right + margin;
        placement = 'right';

        // Still overflows? Try left
        if (left + tooltipWidth > viewportWidth) {
          left = rect.left - tooltipWidth - margin;
          placement = 'left';
        }
      }
    }

    // Clamp to viewport
    if (left + tooltipWidth > viewportWidth) {
      left = viewportWidth - tooltipWidth - 20;
    }
    if (left < 0) {
      left = 20;
    }
    if (top + tooltipHeight > viewportHeight) {
      top = viewportHeight - tooltipHeight - 20;
    }
    if (top < 0) {
      top = 20;
    }

    return { top, left, placement };
  }
}
