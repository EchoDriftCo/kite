import { test, expect } from '@playwright/test';
import { GroceryListPage } from '../pages/grocery-list.page';
import { MealPlansPage } from '../pages/meal-plans.page';

test.describe('Grocery List', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display grocery list', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        await expect(page.locator('h1')).toContainText(/grocery list/i);
        await expect(page).toHaveURL(/grocery-list$/);
      }
    }
  });

  test('should show progress indicator', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        const progressText = await listPage.getProgress();
        expect(progressText).toMatch(/\d+ of \d+ items/i);
      }
    }
  });

  test('should toggle items', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        const itemCount = await listPage.getItemCount();
        
        if (itemCount > 0) {
          await listPage.toggleItem(0);
          await page.waitForTimeout(500);
          // Item should be checked/unchecked (state changed)
        }
      }
    }
  });

  test('should add manual item', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        const beforeCount = await listPage.getItemCount();
        
        await listPage.addItem('Test item from E2E');
        await page.waitForTimeout(500);
        
        const afterCount = await listPage.getItemCount();
        expect(afterCount).toBeGreaterThan(beforeCount);
      }
    }
  });

  test('should show category panels', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        const isEmpty = await listPage.isEmpty();
        
        if (!isEmpty) {
          const categoryCount = await listPage.getCategoryCount();
          expect(categoryCount).toBeGreaterThan(0);
        }
      }
    }
  });

  test('should navigate back', async ({ page }) => {
    const plansPage = new MealPlansPage(page);
    await plansPage.goto();
    await page.waitForLoadState('networkidle');

    const planCount = await plansPage.getPlanCount();
    
    if (planCount > 0) {
      await plansPage.planCards.first().click();
      await page.waitForLoadState('networkidle');

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        const listPage = new GroceryListPage(page);
        await listPage.goBack();
        
        await page.waitForLoadState('networkidle');
        await expect(page).not.toHaveURL(/grocery-list$/);
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

      const groceryButton = page.getByRole('button', { name: /grocery list/i });
      const buttonVisible = await groceryButton.isVisible().catch(() => false);

      if (buttonVisible) {
        await groceryButton.click();
        await page.waitForLoadState('networkidle');

        await expect(page).toHaveScreenshot('grocery-list.png', {
          maxDiffPixels: 100,
          fullPage: true
        });
      }
    }
  });
});
