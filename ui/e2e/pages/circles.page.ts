import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CirclesPage extends BasePage {
  readonly createCircleButton: Locator;
  readonly circleCards: Locator;
  readonly inviteButton: Locator;
  readonly circleNameInput: Locator;
  readonly circleDescriptionInput: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    super(page);
    this.createCircleButton = page.getByRole('button', { name: /create circle/i });
    this.circleCards = page.locator('[data-testid="circle-card"]');
    this.inviteButton = page.getByRole('button', { name: /invite/i });
    this.circleNameInput = page.getByLabel(/circle name/i);
    this.circleDescriptionInput = page.getByLabel(/description/i);
    this.submitButton = page.getByRole('button', { name: /create|save/i });
  }

  async goto() {
    await super.goto('/circles');
  }

  async clickCreateCircle() {
    await this.createCircleButton.click();
  }

  async createCircle(name: string, description: string) {
    await this.clickCreateCircle();
    await this.circleNameInput.fill(name);
    await this.circleDescriptionInput.fill(description);
    await this.submitButton.click();
  }

  async clickCircle(circleName: string) {
    await this.page.getByText(circleName).first().click();
  }

  async getCircleCount(): Promise<number> {
    return await this.circleCards.count();
  }
}
