import { test, expect, Page } from '@playwright/test';
import { RecipeFormPage } from '../pages/recipe-form.page';

/**
 * Guest access: new visitors get a Supabase anonymous session instead of the login screen,
 * and signing up converts that same user so their data is kept.
 *
 * Needs the local stack with anonymous sign-ins enabled (supabase/config.toml:
 * enable_anonymous_sign_ins = true) and email confirmations disabled, so conversion completes immediately.
 */
test.describe('Guest access', () => {
  // Start every test as a brand-new visitor
  test.use({ storageState: { cookies: [], origins: [] } });

  async function dismissOnboarding(page: Page) {
    const skip = page.getByRole('button', { name: /skip setup/i });
    try {
      await skip.waitFor({ state: 'visible', timeout: 5000 });
      await skip.click();
      await expect(skip).toBeHidden();
    } catch {
      // Onboarding didn't open (already completed or the status check failed)
    }
  }

  test('visitor can create a recipe as a guest and keep it after signing up', async ({ page }) => {
    const stamp = Date.now();
    const recipeTitle = `Guest Recipe ${stamp}`;
    const email = `guest-e2e-${stamp}@example.com`;
    const password = 'GuestPass123!';

    // Lands in the app without logging in
    await page.goto('/recipes');
    await expect(page).toHaveURL(/\/recipes/);
    await dismissOnboarding(page);

    const createAccountButton = page.locator('.create-account-btn');
    await expect(createAccountButton).toBeVisible();

    await page.getByRole('button', { name: 'Account menu' }).click();
    await expect(page.getByRole('menuitem', { name: 'Guest' })).toBeVisible();
    await page.keyboard.press('Escape');

    // Create a private recipe
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await formPage.titleInput.fill(recipeTitle);
    await page.getByLabel('Ingredient').first().fill('flour');
    await page.getByLabel('Step 1').fill('Mix everything together');
    await page.getByRole('button', { name: /create recipe/i }).click();

    await expect(page).toHaveURL(/\/recipes\/[0-9a-f-]{36}$/);
    await expect(page.getByRole('heading', { level: 1, name: recipeTitle })).toBeVisible();

    // Convert the guest from the toolbar
    await createAccountButton.click();
    const dialog = page.getByRole('dialog');
    await expect(dialog.getByText("Everything you've made as a guest will be kept.")).toBeVisible();
    await dialog.getByLabel('Email').fill(email);
    await dialog.getByLabel('Password', { exact: true }).fill(password);
    await dialog.getByLabel('Confirm Password').fill(password);
    await dialog.getByRole('checkbox').check();
    await dialog.getByRole('button', { name: 'Create Account' }).click();

    await expect(page.getByText('Account created. Your recipes are saved.')).toBeVisible();
    await expect(createAccountButton).toBeHidden();

    // Same user, so the recipe is still theirs after a reload
    await page.goto('/recipes');
    await expect(page.getByText(recipeTitle)).toBeVisible();

    await page.getByRole('button', { name: 'Account menu' }).click();
    await expect(page.getByRole('menuitem', { name: email })).toBeVisible();
  });

  test('login page lets a visitor continue as a guest', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('button', { name: /continue as guest/i }).click();

    await expect(page).toHaveURL(/\/recipes/);
    await dismissOnboarding(page);
    await expect(page.locator('.create-account-btn')).toBeVisible();
  });
});
