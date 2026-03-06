import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class MealPlanDetailPage extends BasePage {
  readonly planTitle: Locator;
  readonly editButton: Locator;
  readonly deleteButton: Locator;
  readonly groceryListButton: Locator;
  readonly addRecipeButton: Locator;
  readonly dayCards: Locator;
  readonly mealSlots: Locator;
  readonly backButton: Locator;

  constructor(page: Page) {
    super(page);
    this.planTitle = page.locator('h1').first();
    this.editButton = page.getByRole('button', { name: /edit/i });
    this.deleteButton = page.getByRole('button', { name: /delete/i });
    this.groceryListButton = page.getByRole('button', { name: /grocery list/i });
    this.addRecipeButton = page.getByRole('button', { name: /add recipe/i });
    this.dayCards = page.locator('[data-testid="day-card"]');
    this.mealSlots = page.locator('[data-testid="meal-slot"]');
    this.backButton = page.getByRole('button', { name: /arrow_back/i });
  }

  async gotoPlan(planId: string) {
    await super.goto(`/meal-plans/${planId}`);
  }

  async edit() {
    await this.editButton.click();
  }

  async delete() {
    await this.deleteButton.click();
  }

  async openGroceryList() {
    await this.groceryListButton.click();
  }

  async addRecipe() {
    await this.addRecipeButton.click();
  }

  async goBack() {
    await this.backButton.click();
  }

  async getDayCount(): Promise<number> {
    return await this.dayCards.count();
  }

  async getMealCount(): Promise<number> {
    return await this.mealSlots.count();
  }
}
