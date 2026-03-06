import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CircleInvitePage extends BasePage {
  readonly circleName: Locator;
  readonly circleDescription: Locator;
  readonly inviterEmail: Locator;
  readonly acceptButton: Locator;
  readonly declineButton: Locator;
  readonly loginPrompt: Locator;
  readonly signUpButton: Locator;

  constructor(page: Page) {
    super(page);
    this.circleName = page.locator('h1').first();
    this.circleDescription = page.locator('.circle-description');
    this.inviterEmail = page.locator('.inviter-email');
    this.acceptButton = page.getByRole('button', { name: /accept|join/i });
    this.declineButton = page.getByRole('button', { name: /decline|reject/i });
    this.loginPrompt = page.locator('.login-prompt');
    this.signUpButton = page.getByRole('button', { name: /sign up/i });
  }

  async gotoInvite(token: string) {
    await super.goto(`/join/${token}`);
  }

  async accept() {
    await this.acceptButton.click();
  }

  async decline() {
    await this.declineButton.click();
  }

  async needsLogin(): Promise<boolean> {
    return await this.loginPrompt.isVisible().catch(() => false);
  }
}
