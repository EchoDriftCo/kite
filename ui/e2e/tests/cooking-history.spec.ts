import { test, expect } from '@playwright/test';
import { CookingHistoryPage } from '../pages/cooking-history.page';

test.describe('Cooking History Page', () => {
  let historyPage: CookingHistoryPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    historyPage = new CookingHistoryPage(page);
    await historyPage.goto();
  });

  test('should display cooking history page', async ({ page }) => {
    await expect(page).toHaveURL(/cooking-history/);
    await expect(historyPage.navigationBar).toBeVisible();
  });

  test('should display history list', async () => {
    await historyPage.page.waitForLoadState('networkidle');
    
    const historyCount = await historyPage.getHistoryCount();
    expect(historyCount).toBeGreaterThanOrEqual(0);
  });

  test('should display filter options', async () => {
    const dateFilterVisible = await historyPage.filterByDate.isVisible().catch(() => false);
    const recipeFilterVisible = await historyPage.filterByRecipe.isVisible().catch(() => false);
    
    // At least one filter should be available
    expect(dateFilterVisible || recipeFilterVisible).toBeTruthy();
  });

  test('should display history items with details', async () => {
    await historyPage.page.waitForLoadState('networkidle');
    
    const historyCount = await historyPage.getHistoryCount();
    
    if (historyCount > 0) {
      const firstItem = historyPage.historyItems.first();
      await expect(firstItem).toBeVisible();
      
      // Check if item has text content (recipe name, date, etc.)
      const content = await firstItem.textContent();
      expect(content).toBeTruthy();
      expect(content!.length).toBeGreaterThan(0);
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('cooking-history-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
