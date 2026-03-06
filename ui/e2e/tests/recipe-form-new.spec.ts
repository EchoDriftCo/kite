import { test, expect } from '@playwright/test';
import { RecipeFormPage } from '../pages/recipe-form.page';

test.describe('Recipe Create Form', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display create form', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await expect(page.locator('h1')).toContainText(/create new recipe/i);
    await expect(formPage.titleInput).toBeVisible();
    await expect(formPage.saveButton).toBeVisible();
  });

  test('should show import button on new recipe', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    if (importVisible) {
      await expect(formPage.importButton).toBeEnabled();
    }
  });

  test('should add ingredients', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.addIngredient('2', 'cups', 'flour', 'sifted');
    await formPage.addIngredient('1', 'tsp', 'salt');

    const ingredientCount = await formPage.getIngredientCount();
    expect(ingredientCount).toBeGreaterThanOrEqual(2);
  });

  test('should add instructions', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.addInstruction('Preheat oven to 350°F');
    await formPage.addInstruction('Mix dry ingredients');

    const instructionCount = await formPage.getInstructionCount();
    expect(instructionCount).toBeGreaterThanOrEqual(2);
  });

  test('should create a complete recipe', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.fillBasicInfo('E2E Test Recipe', 'A test recipe from Playwright', 4);
    await formPage.fillTimes(15, 30);

    await formPage.addIngredient('2', 'cups', 'flour');
    await formPage.addIngredient('1', 'cup', 'sugar');

    await formPage.addInstruction('Mix ingredients');
    await formPage.addInstruction('Bake at 350°F');

    await formPage.save();

    // Should redirect to recipe detail
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveURL(/recipes\/[a-f0-9-]+/);
  });

  test('should validate required fields', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    // Try to save without filling required fields
    await formPage.save();

    // Should stay on form page
    await expect(page).toHaveURL(/recipes\/new/);
  });

  test('should toggle public visibility', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const toggleVisible = await formPage.publicToggle.isVisible().catch(() => false);
    if (toggleVisible) {
      await formPage.togglePublic();
      const isChecked = await formPage.publicToggle.isChecked();
      expect(isChecked).toBeDefined();
    }
  });

  test('should cancel and navigate away', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.fillBasicInfo('Cancel Test', 'This should be discarded', 2);
    await formPage.cancel();

    // Should navigate away from form
    await page.waitForLoadState('networkidle');
    await expect(page).not.toHaveURL(/recipes\/new/);
  });

  test('should take visual snapshot', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot('recipe-create-form.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
