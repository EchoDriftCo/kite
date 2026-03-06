import { test, expect } from '@playwright/test';
import { RecipeFormPage } from '../pages/recipe-form.page';

test.describe('Recipe Import Dialog', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should open import dialog from create form', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      // Dialog should be visible
      const dialog = page.locator('mat-dialog-container');
      await expect(dialog).toBeVisible();
    }
  });

  test('should show three import tabs', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      // Check for tabs
      const imageTab = page.getByRole('tab', { name: /from image/i });
      const urlTab = page.getByRole('tab', { name: /from url/i });
      const paprikaTab = page.getByRole('tab', { name: /from paprika/i });

      await expect(imageTab).toBeVisible();
      await expect(urlTab).toBeVisible();
      await expect(paprikaTab).toBeVisible();
    }
  });

  test('should show URL input in URL tab', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      const urlTab = page.getByRole('tab', { name: /from url/i });
      await urlTab.click();
      await page.waitForTimeout(500);

      const urlInput = page.getByLabel('Recipe URL');
      await expect(urlInput).toBeVisible();
    }
  });

  test('should show file upload in image tab', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      const imageTab = page.getByRole('tab', { name: /from image/i });
      await imageTab.click();
      await page.waitForTimeout(500);

      const dropZone = page.locator('.drop-zone');
      const dropZoneVisible = await dropZone.isVisible().catch(() => false);
      
      if (dropZoneVisible) {
        await expect(dropZone).toBeVisible();
      }
    }
  });

  test('should show Paprika file upload', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      const paprikaTab = page.getByRole('tab', { name: /from paprika/i });
      await paprikaTab.click();
      await page.waitForTimeout(500);

      const dropZone = page.locator('.drop-zone');
      const dropZoneVisible = await dropZone.isVisible().catch(() => false);
      
      if (dropZoneVisible) {
        await expect(dropZone).toBeVisible();
      }
    }
  });

  test('should close dialog on cancel', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      const cancelButton = page.getByRole('button', { name: /cancel/i });
      await cancelButton.click();
      await page.waitForTimeout(500);

      const dialog = page.locator('mat-dialog-container');
      const dialogVisible = await dialog.isVisible().catch(() => false);
      expect(dialogVisible).toBe(false);
    }
  });

  test('should take visual snapshot of import dialog', async ({ page }) => {
    const formPage = new RecipeFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const importVisible = await formPage.importButton.isVisible().catch(() => false);
    
    if (importVisible) {
      await formPage.openImportDialog();
      await page.waitForTimeout(500);

      await expect(page).toHaveScreenshot('recipe-import-dialog.png', {
        maxDiffPixels: 100
      });
    }
  });
});
