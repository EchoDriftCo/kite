import { Page, Locator } from '@playwright/test';

/**
 * Base Page Object that contains common functionality across all pages
 */
export class BasePage {
  readonly page: Page;
  readonly navigationBar: Locator;
  readonly menuButton: Locator;
  readonly userMenu: Locator;

  constructor(page: Page) {
    this.page = page;
    this.navigationBar = page.getByRole('navigation');
    this.menuButton = page.getByLabel('Open menu');
    this.userMenu = page.getByLabel('User menu');
  }

  async goto(path: string) {
    await this.page.goto(path);
  }

  async navigateToRecipes() {
    await this.page.getByRole('link', { name: /recipes/i }).first().click();
  }

  async navigateToCircles() {
    await this.page.getByRole('link', { name: /circles/i }).first().click();
  }

  async navigateToDietaryProfiles() {
    await this.page.getByRole('link', { name: /dietary profiles/i }).first().click();
  }

  async navigateToMealPlans() {
    await this.page.getByRole('link', { name: /meal plans/i }).first().click();
  }

  async navigateToCookingHistory() {
    await this.page.getByRole('link', { name: /cooking history/i }).first().click();
  }

  async navigateToCookingStats() {
    await this.page.getByRole('link', { name: /cooking stats/i }).first().click();
  }

  async navigateToEquipment() {
    await this.page.getByRole('link', { name: /equipment/i }).first().click();
  }

  async openMobileMenu() {
    const isMobile = await this.menuButton.isVisible();
    if (isMobile) {
      await this.menuButton.click();
    }
  }

  async takeSnapshot(name: string) {
    await this.page.screenshot({ path: `e2e/snapshots/${name}.png`, fullPage: true });
  }
}
