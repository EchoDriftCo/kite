import { test, expect } from '@playwright/test';
import { MealPlanFormPage } from '../pages/meal-plan-form.page';

test.describe('Meal Plan Form', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display create form', async ({ page }) => {
    const formPage = new MealPlanFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await expect(page.locator('h1')).toContainText(/create meal plan/i);
    await expect(formPage.nameInput).toBeVisible();
    await expect(formPage.startDateInput).toBeVisible();
    await expect(formPage.endDateInput).toBeVisible();
  });

  test('should create a meal plan', async ({ page }) => {
    const formPage = new MealPlanFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    const timestamp = Date.now();
    const startDate = new Date();
    const endDate = new Date();
    endDate.setDate(endDate.getDate() + 7);

    await formPage.fillPlanDetails(
      `Test Plan ${timestamp}`,
      startDate.toISOString().split('T')[0],
      endDate.toISOString().split('T')[0]
    );

    await formPage.save();
    
    await page.waitForLoadState('networkidle');
    // Should redirect to meal plan detail or list
    await expect(page).not.toHaveURL(/\/new$/);
  });

  test('should validate required fields', async ({ page }) => {
    const formPage = new MealPlanFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.save();
    
    // Should stay on form page
    await expect(page).toHaveURL(/meal-plans\/new/);
  });

  test('should cancel and navigate away', async ({ page }) => {
    const formPage = new MealPlanFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await formPage.nameInput.fill('Cancel Test');
    await formPage.cancel();
    
    await page.waitForLoadState('networkidle');
    await expect(page).not.toHaveURL(/meal-plans\/new/);
  });

  test('should take visual snapshot', async ({ page }) => {
    const formPage = new MealPlanFormPage(page);
    await formPage.gotoNew();
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot('meal-plan-form.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
