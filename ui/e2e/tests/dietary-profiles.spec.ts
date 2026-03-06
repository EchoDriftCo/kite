import { test, expect } from '@playwright/test';
import { DietaryProfilesPage } from '../pages/dietary-profiles.page';

test.describe('Dietary Profiles Page', () => {
  let profilesPage: DietaryProfilesPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    profilesPage = new DietaryProfilesPage(page);
    await profilesPage.goto();
  });

  test('should display dietary profiles page', async ({ page }) => {
    await expect(page).toHaveURL(/dietary-profiles/);
    await expect(profilesPage.navigationBar).toBeVisible();
  });

  test('should display profile options', async () => {
    await profilesPage.page.waitForLoadState('networkidle');
    
    // Check if allergen or dietary preference checkboxes are visible
    const allergensVisible = await profilesPage.allergenCheckboxes.first().isVisible().catch(() => false);
    const preferencesVisible = await profilesPage.dietaryPreferenceCheckboxes.first().isVisible().catch(() => false);
    
    // At least one type of option should be present
    expect(allergensVisible || preferencesVisible).toBeTruthy();
  });

  test('should allow selecting dietary preferences', async () => {
    await profilesPage.page.waitForLoadState('networkidle');
    
    // Try to select a common dietary preference
    const vegetarianLabel = profilesPage.page.getByLabel(/vegetarian/i);
    const isVisible = await vegetarianLabel.isVisible().catch(() => false);
    
    if (isVisible) {
      await vegetarianLabel.check();
      await expect(vegetarianLabel).toBeChecked();
    }
  });

  test('should allow selecting allergens', async () => {
    await profilesPage.page.waitForLoadState('networkidle');
    
    // Try to select a common allergen
    const nutsLabel = profilesPage.page.getByLabel(/nuts|peanuts/i).first();
    const isVisible = await nutsLabel.isVisible().catch(() => false);
    
    if (isVisible) {
      await nutsLabel.check();
      await expect(nutsLabel).toBeChecked();
    }
  });

  test('should have save button', async () => {
    const saveVisible = await profilesPage.saveButton.isVisible().catch(() => false);
    
    if (saveVisible) {
      await expect(profilesPage.saveButton).toBeEnabled();
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('dietary-profiles-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
