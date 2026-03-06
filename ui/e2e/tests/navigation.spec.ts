import { test, expect } from '@playwright/test';
import { BasePage } from '../pages/base.page';

test.describe('Navigation', () => {
  test.use({ storageState: '.auth/user.json' });

  test.describe('Desktop Navigation', () => {
    test.use({ viewport: { width: 1920, height: 1080 } });

    test('should display navigation bar', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      await expect(basePage.navigationBar).toBeVisible();
    });

    test('should navigate to all main sections', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      // Navigate to Recipes
      await basePage.navigateToRecipes();
      await expect(page).toHaveURL(/recipes/);
      
      // Navigate to Circles
      await basePage.navigateToCircles();
      await expect(page).toHaveURL(/circles/);
      
      // Navigate to Dietary Profiles
      await basePage.navigateToDietaryProfiles();
      await expect(page).toHaveURL(/dietary-profiles/);
      
      // Navigate to Meal Plans
      await basePage.navigateToMealPlans();
      await expect(page).toHaveURL(/meal-plans/);
      
      // Navigate to Equipment
      await basePage.navigateToEquipment();
      await expect(page).toHaveURL(/equipment/);
    });
  });

  test.describe('Mobile Navigation', () => {
    test.use({ viewport: { width: 375, height: 667 } });

    test('should display hamburger menu on mobile', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      // Should show menu button on mobile
      await expect(basePage.menuButton).toBeVisible();
    });

    test('should open mobile menu', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      await basePage.openMobileMenu();
      
      // Wait for menu to open
      await page.waitForTimeout(500);
      
      // Menu items should be visible
      const recipesLink = page.getByRole('link', { name: /recipes/i }).first();
      await expect(recipesLink).toBeVisible();
    });

    test('should navigate from mobile menu', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      await basePage.openMobileMenu();
      await page.waitForTimeout(500);
      
      // Click on a menu item
      await basePage.navigateToCircles();
      await expect(page).toHaveURL(/circles/);
    });

    test('should take mobile navigation snapshot', async ({ page }) => {
      const basePage = new BasePage(page);
      await basePage.goto('/recipes');
      
      await basePage.openMobileMenu();
      await page.waitForTimeout(500);
      
      await expect(page).toHaveScreenshot('mobile-navigation.png', {
        maxDiffPixels: 100
      });
    });
  });
});
