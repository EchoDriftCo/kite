import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class GroceryListPage extends BasePage {
  readonly backButton: Locator;
  readonly copyButton: Locator;
  readonly printButton: Locator;
  readonly progressText: Locator;
  readonly categoryPanels: Locator;
  readonly groceryItems: Locator;
  readonly addItemInput: Locator;
  readonly addItemButton: Locator;
  readonly emptyState: Locator;

  constructor(page: Page) {
    super(page);
    this.backButton = page.getByRole('button', { name: /arrow_back/i });
    this.copyButton = page.getByRole('button', { name: /copy/i });
    this.printButton = page.getByRole('button', { name: /print/i });
    this.progressText = page.locator('.progress');
    this.categoryPanels = page.locator('mat-expansion-panel');
    this.groceryItems = page.locator('.grocery-item');
    this.addItemInput = page.getByLabel('Add item');
    this.addItemButton = page.getByRole('button', { name: /add_circle/i });
    this.emptyState = page.locator('.empty-state');
  }

  async gotoGroceryList(planId: string) {
    await super.goto(`/meal-plans/${planId}/grocery-list`);
  }

  async goBack() {
    await this.backButton.click();
  }

  async copyToClipboard() {
    await this.copyButton.click();
  }

  async print() {
    await this.printButton.click();
  }

  async toggleItem(index: number) {
    await this.groceryItems.nth(index).locator('mat-checkbox').click();
  }

  async addItem(itemText: string) {
    await this.addItemInput.fill(itemText);
    await this.addItemButton.click();
  }

  async removeItem(index: number) {
    await this.groceryItems.nth(index).locator('.remove-btn').click();
  }

  async getItemCount(): Promise<number> {
    return await this.groceryItems.count();
  }

  async getCategoryCount(): Promise<number> {
    return await this.categoryPanels.count();
  }

  async getProgress(): Promise<string> {
    return await this.progressText.textContent() || '';
  }

  async isEmpty(): Promise<boolean> {
    return await this.emptyState.isVisible().catch(() => false);
  }
}
