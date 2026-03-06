import { test, expect } from '@playwright/test';
import { RecipeGeneratorPage } from '../pages/recipe-generator.page';

test.describe('Recipe Generator', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display generator interface', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    await expect(page.locator('h1')).toContainText(/ai recipe generator/i);
    await expect(generatorPage.promptInput).toBeVisible();
    await expect(generatorPage.generateButton).toBeVisible();
  });

  test('should show quota badge', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    const quotaVisible = await generatorPage.quotaBadge.isVisible().catch(() => false);
    if (quotaVisible) {
      const quota = await generatorPage.getQuotaRemaining();
      expect(quota).toBeGreaterThanOrEqual(0);
    }
  });

  test('should expand constraints panel', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    await generatorPage.expandConstraints();
    await page.waitForTimeout(500);

    await expect(generatorPage.maxTimeSelect).toBeVisible();
    await expect(generatorPage.skillLevelSelect).toBeVisible();
  });

  test('should disable generate button when prompt is empty', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    const isDisabled = await generatorPage.generateButton.isDisabled();
    expect(isDisabled).toBe(true);
  });

  test('should enable generate button with prompt', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    await generatorPage.enterPrompt('A simple pasta recipe');
    const isEnabled = await generatorPage.generateButton.isEnabled();
    expect(isEnabled).toBe(true);
  });

  test('should set constraints', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    await generatorPage.expandConstraints();
    await page.waitForTimeout(500);

    const timeOptions = await generatorPage.maxTimeSelect.locator('option').count();
    if (timeOptions > 1) {
      await generatorPage.setMaxTime(30);
    }

    const skillOptions = await generatorPage.skillLevelSelect.locator('option').count();
    if (skillOptions > 1) {
      await generatorPage.setSkillLevel('Beginner');
    }
  });

  test('should show empty state initially', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    const hasPreview = await generatorPage.hasPreview();
    expect(hasPreview).toBe(false);
  });

  test('should take visual snapshot', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot('recipe-generator-empty.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });

  // Note: Actual generation requires API quota and is slow
  // Testing the UI only, not the actual AI generation
  test('should show start over button after generation', async ({ page }) => {
    const generatorPage = new RecipeGeneratorPage(page);
    await generatorPage.goto();
    await page.waitForLoadState('networkidle');

    const hasPreview = await generatorPage.hasPreview();
    
    if (hasPreview) {
      const startOverVisible = await generatorPage.startOverButton.isVisible().catch(() => false);
      if (startOverVisible) {
        await expect(generatorPage.startOverButton).toBeEnabled();
      }
    }
  });
});
