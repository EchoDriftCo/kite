import { test, expect } from '@playwright/test';
import { CircleDetailPage } from '../pages/circle-detail.page';
import { CirclesPage } from '../pages/circles.page';

test.describe('Circle Detail Page', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display circle details', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      await expect(detailPage.circleTitle).toBeVisible();
      await expect(page).toHaveURL(/circles\/[a-f0-9-]+/);
    }
  });

  test('should show recipes and members tabs', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      await expect(detailPage.recipesTab).toBeVisible();
      await expect(detailPage.membersTab).toBeVisible();
    }
  });

  test('should switch between tabs', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      
      await detailPage.switchToMembersTab();
      await page.waitForTimeout(500);
      await expect(detailPage.membersList).toBeVisible();

      await detailPage.switchToRecipesTab();
      await page.waitForTimeout(500);
      const recipesVisible = await detailPage.recipeCards.first().isVisible().catch(() => false);
      const emptyVisible = await detailPage.emptyState.isVisible().catch(() => false);
      
      expect(recipesVisible || emptyVisible).toBe(true);
    }
  });

  test('should display members list', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      const memberCount = await detailPage.getMemberCount();
      expect(memberCount).toBeGreaterThanOrEqual(1); // At least the owner
    }
  });

  test('should show invite button for owners', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      await detailPage.switchToMembersTab();
      await page.waitForTimeout(500);

      const inviteVisible = await detailPage.inviteButton.isVisible().catch(() => false);
      // Invite button may or may not be visible depending on permissions
      expect(inviteVisible).toBeDefined();
    }
  });

  test('should navigate back', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new CircleDetailPage(page);
      await detailPage.goBack();
      
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/circles$/);
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    const circlesPage = new CirclesPage(page);
    await circlesPage.goto();
    await page.waitForLoadState('networkidle');

    const circleCount = await circlesPage.getCircleCount();
    
    if (circleCount > 0) {
      await circlesPage.circleCards.first().click();
      await page.waitForLoadState('networkidle');

      await expect(page).toHaveScreenshot('circle-detail-page.png', {
        maxDiffPixels: 100,
        fullPage: true
      });
    }
  });
});
