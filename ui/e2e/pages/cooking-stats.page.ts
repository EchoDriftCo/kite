import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CookingStatsPage extends BasePage {
  readonly statsContainer: Locator;
  readonly totalRecipesCooked: Locator;
  readonly favoriteRecipes: Locator;
  readonly cookingStreak: Locator;
  readonly timeRangeSelector: Locator;

  constructor(page: Page) {
    super(page);
    this.statsContainer = page.locator('[data-testid="stats-container"]');
    this.totalRecipesCooked = page.locator('[data-testid="total-recipes"]');
    this.favoriteRecipes = page.locator('[data-testid="favorite-recipes"]');
    this.cookingStreak = page.locator('[data-testid="cooking-streak"]');
    this.timeRangeSelector = page.getByLabel(/time range/i);
  }

  async goto() {
    await super.goto('/cooking-stats');
  }

  async selectTimeRange(range: string) {
    await this.timeRangeSelector.selectOption(range);
  }

  async getTotalRecipesCooked(): Promise<string> {
    return await this.totalRecipesCooked.textContent() || '0';
  }
}
