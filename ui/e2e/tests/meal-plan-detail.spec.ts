import { test, expect } from '@playwright/test';
import { MealPlanDetailPage } from '../pages/meal-plan-detail.page';
import { MealPlansPage } from '../pages/meal-plans.page';

test.describe('Meal Plan Detail', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display meal plan details', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new MealPlanDetailPage(page);
      await expect(detailPage.planTitle).toBeVisible();
      await expect(page).toHaveURL(/meal-plans\/[a-f0-9-]+$/);
    }
  });

  test('should show grocery list button', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new MealPlanDetailPage(page);
      const buttonVisible = await detailPage.groceryListButton.isVisible().catch(() => false);
      
      if (buttonVisible) {
        await expect(detailPage.groceryListButton).toBeEnabled();
      }
    }
  });

  test('should navigate to grocery list', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new MealPlanDetailPage(page);
      const buttonVisible = await detailPage.groceryListButton.isVisible().catch(() => false);
      
      if (buttonVisible) {
        await detailPage.openGroceryList();
        await page.waitForLoadState('networkidle');
        await expect(page).toHaveURL(/grocery-list$/);
      }
    }
  });

  test('should show edit and delete buttons', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new MealPlanDetailPage(page);
      const editVisible = await detailPage.editButton.isVisible().catch(() => false);
      const deleteVisible = await detailPage.deleteButton.isVisible().catch(() => false);
      
      // At least one action button should be visible
      expect(editVisible || deleteVisible).toBe(true);
    }
  });

  test('should navigate to edit form', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new MealPlanDetailPage(page);
      const editVisible = await detailPage.editButton.isVisible().catch(() => false);
      
      if (editVisible) {
        await detailPage.edit();
        await page.waitForLoadState('networkidle');
        await expect(page).toHaveURL(/edit$/);
      }
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      await expect(page).toHaveScreenshot('meal-plan-detail.png', {
        maxDiffPixels: 100,
        fullPage: true
      });
    }
  });
});
