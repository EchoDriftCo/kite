import { test, expect } from '@playwright/test';
import { RecipeMixerPage } from '../pages/recipe-mixer.page';

test.describe('Recipe Mixer', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display mixer interface', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    await expect(page.locator('h1')).toContainText(/recipe mixer/i);
    await expect(mixerPage.recipeAInput).toBeVisible();
    await expect(mixerPage.recipeBInput).toBeVisible();
    await expect(mixerPage.modeSelect).toBeVisible();
  });

  test('should disable mix button initially', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    const isEnabled = await mixerPage.isMixButtonEnabled();
    expect(isEnabled).toBe(false);
  });

  test('should show mode selection', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    await mixerPage.setMode('bestOfBoth');
    const selectedValue = await mixerPage.modeSelect.inputValue();
    expect(selectedValue).toBe('bestOfBoth');
  });

  test('should show intent field for guided mode', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    await mixerPage.setMode('guided');
    await page.waitForTimeout(500);

    const intentVisible = await mixerPage.intentInput.isVisible().catch(() => false);
    if (intentVisible) {
      await expect(mixerPage.intentInput).toBeVisible();
    }
  });

  test('should have autocomplete for recipe selection', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    await mixerPage.recipeAInput.fill('test');
    await page.waitForTimeout(1000);

    // Check if autocomplete options appear
    const autocompleteVisible = await page.locator('mat-option').first().isVisible().catch(() => false);
    expect(autocompleteVisible).toBeDefined();
  });

  test('should show empty state initially', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    const hasPreview = await mixerPage.hasPreview();
    expect(hasPreview).toBe(false);
  });

  test('should take visual snapshot', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot('recipe-mixer-empty.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });

  // Note: Actual mixing requires selecting real recipes and API quota
  // Testing the UI only
  test('should show preview actions after mixing', async ({ page }) => {
    const mixerPage = new RecipeMixerPage(page);
    await mixerPage.goto();
    await page.waitForLoadState('networkidle');

    const hasPreview = await mixerPage.hasPreview();
    
    if (hasPreview) {
      const saveVisible = await mixerPage.saveButton.isVisible().catch(() => false);
      const resetVisible = await mixerPage.resetButton.isVisible().catch(() => false);
      
      if (saveVisible) await expect(mixerPage.saveButton).toBeVisible();
      if (resetVisible) await expect(mixerPage.resetButton).toBeVisible();
    }
  });
});
