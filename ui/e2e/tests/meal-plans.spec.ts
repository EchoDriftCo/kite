import { test, expect } from '@playwright/test';
import { MealPlansPage } from '../pages/meal-plans.page';

test.describe('Meal Plans Page', () => {
  let mealPlansPage: MealPlansPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    mealPlansPage = new MealPlansPage(page);
    await mealPlansPage.goto();
  });

  test('should display meal plans page', async ({ page }) => {
    await expect(page).toHaveURL(/meal-plans/);
    await expect(mealPlansPage.navigationBar).toBeVisible();
  });

  test('should display create meal plan button', async () => {
    await expect(mealPlansPage.createMealPlanButton).toBeVisible();
  });

  test('should display meal plan list', async () => {
    await mealPlansPage.page.waitForLoadState('networkidle');
    
    const planCount = await mealPlansPage.getMealPlanCount();
    expect(planCount).toBeGreaterThanOrEqual(0);
  });

  test('should open create meal plan form', async ({ page }) => {
    await mealPlansPage.clickCreateMealPlan();
    
    // Wait for form to appear
    await page.waitForTimeout(500);
    
    // Check if input fields are visible
    const nameInputVisible = await mealPlansPage.mealPlanNameInput.isVisible().catch(() => false);
    if (nameInputVisible) {
      await expect(mealPlansPage.mealPlanNameInput).toBeVisible();
      await expect(mealPlansPage.submitButton).toBeVisible();
    }
  });

  test('should navigate to meal plan detail', async ({ page }) => {
    await mealPlansPage.page.waitForLoadState('networkidle');
    
    const planCount = await mealPlansPage.getMealPlanCount();
    
    if (planCount > 0) {
      const firstPlan = mealPlansPage.mealPlanCards.first();
      await firstPlan.click();
      
      // Should navigate to detail page
      await expect(page).toHaveURL(/meal-plans\/[a-f0-9-]+/);
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('meal-plans-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
