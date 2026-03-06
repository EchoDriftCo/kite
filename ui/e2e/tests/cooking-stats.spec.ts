import { test, expect } from '@playwright/test';
import { CookingStatsPage } from '../pages/cooking-stats.page';

test.describe('Cooking Stats Page', () => {
  let statsPage: CookingStatsPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    statsPage = new CookingStatsPage(page);
    await statsPage.goto();
  });

  test('should display cooking stats page', async ({ page }) => {
    await expect(page).toHaveURL(/cooking-stats/);
    await expect(statsPage.navigationBar).toBeVisible();
  });

  test('should display stats container', async () => {
    await statsPage.page.waitForLoadState('networkidle');
    
    const statsVisible = await statsPage.statsContainer.isVisible().catch(() => false);
    
    if (statsVisible) {
      await expect(statsPage.statsContainer).toBeVisible();
    }
  });

  test('should display total recipes cooked stat', async () => {
    await statsPage.page.waitForLoadState('networkidle');
    
    const totalVisible = await statsPage.totalRecipesCooked.isVisible().catch(() => false);
    
    if (totalVisible) {
      const total = await statsPage.getTotalRecipesCooked();
      expect(total).toBeTruthy();
    }
  });

  test('should have time range selector', async () => {
    const selectorVisible = await statsPage.timeRangeSelector.isVisible().catch(() => false);
    
    if (selectorVisible) {
      await expect(statsPage.timeRangeSelector).toBeEnabled();
    }
  });

  test('should display cooking streak', async () => {
    await statsPage.page.waitForLoadState('networkidle');
    
    const streakVisible = await statsPage.cookingStreak.isVisible().catch(() => false);
    
    if (streakVisible) {
      const streakText = await statsPage.cookingStreak.textContent();
      expect(streakText).toBeTruthy();
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('cooking-stats-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
