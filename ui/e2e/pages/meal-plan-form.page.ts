import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class MealPlanFormPage extends BasePage {
  readonly nameInput: Locator;
  readonly startDateInput: Locator;
  readonly endDateInput: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;
  readonly loadingSpinner: Locator;

  constructor(page: Page) {
    super(page);
    this.nameInput = page.getByLabel('Plan Name');
    this.startDateInput = page.getByLabel('Start Date');
    this.endDateInput = page.getByLabel('End Date');
    this.saveButton = page.getByRole('button', { name: /create plan|update plan/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
    this.loadingSpinner = page.locator('mat-spinner');
  }

  async gotoNew() {
    await super.goto('/meal-plans/new');
  }

  async gotoEdit(planId: string) {
    await super.goto(`/meal-plans/${planId}/edit`);
  }

  async fillPlanDetails(name: string, startDate: string, endDate: string) {
    await this.nameInput.fill(name);
    await this.startDateInput.fill(startDate);
    await this.endDateInput.fill(endDate);
  }

  async save() {
    await this.saveButton.click();
  }

  async cancel() {
    await this.cancelButton.click();
  }

  async isLoading(): Promise<boolean> {
    return await this.loadingSpinner.isVisible().catch(() => false);
  }
}
