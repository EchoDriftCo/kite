import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class DietaryProfilesPage extends BasePage {
  readonly addProfileButton: Locator;
  readonly profilesList: Locator;
  readonly allergenCheckboxes: Locator;
  readonly dietaryPreferenceCheckboxes: Locator;
  readonly saveButton: Locator;

  constructor(page: Page) {
    super(page);
    this.addProfileButton = page.getByRole('button', { name: /add profile/i });
    this.profilesList = page.locator('[data-testid="profile-list"]');
    this.allergenCheckboxes = page.locator('[data-testid="allergen-checkbox"]');
    this.dietaryPreferenceCheckboxes = page.locator('[data-testid="dietary-preference-checkbox"]');
    this.saveButton = page.getByRole('button', { name: /save/i });
  }

  async goto() {
    await super.goto('/dietary-profiles');
  }

  async selectAllergen(allergen: string) {
    await this.page.getByLabel(allergen).check();
  }

  async selectDietaryPreference(preference: string) {
    await this.page.getByLabel(preference).check();
  }

  async saveProfile() {
    await this.saveButton.click();
  }
}
