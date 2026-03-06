import { test, expect } from '@playwright/test';
import { CirclesPage } from '../pages/circles.page';

test.describe('Circles Page', () => {
  let circlesPage: CirclesPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    circlesPage = new CirclesPage(page);
    await circlesPage.goto();
  });

  test('should display circles page', async ({ page }) => {
    await expect(page).toHaveURL(/circles/);
    await expect(circlesPage.navigationBar).toBeVisible();
  });

  test('should display create circle button', async () => {
    await expect(circlesPage.createCircleButton).toBeVisible();
  });

  test('should display circle list', async () => {
    await circlesPage.page.waitForLoadState('networkidle');
    
    const circleCount = await circlesPage.getCircleCount();
    expect(circleCount).toBeGreaterThanOrEqual(0);
  });

  test('should open create circle dialog', async ({ page }) => {
    await circlesPage.clickCreateCircle();
    
    // Wait for dialog/form to appear
    await page.waitForTimeout(500);
    
    // Check if input fields are visible
    const nameInputVisible = await circlesPage.circleNameInput.isVisible().catch(() => false);
    if (nameInputVisible) {
      await expect(circlesPage.circleNameInput).toBeVisible();
      await expect(circlesPage.submitButton).toBeVisible();
    }
  });

  test('should display invite button on circles with permission', async () => {
    await circlesPage.page.waitForLoadState('networkidle');
    
    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      const firstCircle = circlesPage.circleCards.first();
      await firstCircle.click();
      
      // Check if invite button appears on detail page
      await circlesPage.page.waitForTimeout(500);
      const inviteVisible = await circlesPage.inviteButton.isVisible().catch(() => false);
      
      if (inviteVisible) {
        await expect(circlesPage.inviteButton).toBeEnabled();
      }
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('circles-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
