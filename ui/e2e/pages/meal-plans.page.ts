import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class MealPlansPage extends BasePage {
  readonly createMealPlanButton: Locator;
  readonly mealPlanCards: Locator;
  readonly mealPlanNameInput: Locator;
  readonly startDateInput: Locator;
  readonly endDateInput: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    super(page);
    this.createMealPlanButton = page.getByRole('button', { name: /create meal plan/i });
    this.mealPlanCards = page.locator('[data-testid="meal-plan-card"]');
    this.mealPlanNameInput = page.getByLabel(/meal plan name/i);
    this.startDateInput = page.getByLabel(/start date/i);
    this.endDateInput = page.getByLabel(/end date/i);
    this.submitButton = page.getByRole('button', { name: /create|save/i });
  }

  async goto() {
    await super.goto('/meal-plans');
  }

  async clickCreateMealPlan() {
    await this.createMealPlanButton.click();
  }

  async createMealPlan(name: string, startDate: string, endDate: string) {
    await this.clickCreateMealPlan();
    await this.mealPlanNameInput.fill(name);
    await this.startDateInput.fill(startDate);
    await this.endDateInput.fill(endDate);
    await this.submitButton.click();
  }

  async clickMealPlan(planName: string) {
    await this.page.getByText(planName).first().click();
  }

  async getMealPlanCount(): Promise<number> {
    return await this.mealPlanCards.count();
  }
}
