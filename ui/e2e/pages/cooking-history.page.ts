import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CookingHistoryPage extends BasePage {
  readonly historyList: Locator;
  readonly filterByDate: Locator;
  readonly filterByRecipe: Locator;
  readonly historyItems: Locator;

  constructor(page: Page) {
    super(page);
    this.historyList = page.locator('[data-testid="history-list"]');
    this.filterByDate = page.getByLabel(/filter by date/i);
    this.filterByRecipe = page.getByLabel(/filter by recipe/i);
    this.historyItems = page.locator('[data-testid="history-item"]');
  }

  async goto() {
    await super.goto('/cooking-history');
  }

  async getHistoryCount(): Promise<number> {
    return await this.historyItems.count();
  }

  async filterByDateRange(startDate: string, endDate: string) {
    // Implementation depends on the actual UI
    await this.filterByDate.click();
  }

  async clickHistoryItem(index: number) {
    await this.historyItems.nth(index).click();
  }
}
