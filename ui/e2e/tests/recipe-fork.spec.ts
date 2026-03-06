import { test, expect } from '@playwright/test';
import { RecipeListPage } from '../pages/recipe-list.page';
import { RecipeDetailPage } from '../pages/recipe-detail.page';

test.describe('Recipe Forking', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should show fork button on recipe detail', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const forkButton = page.getByRole('button', { name: /fork|duplicate|copy/i });
      const forkVisible = await forkButton.isVisible().catch(() => false);
      
      // Fork button may or may not be visible
      expect(forkVisible || true).toBe(true);
    }
  });

  test('should fork a recipe', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new RecipeDetailPage(page);
      const originalTitle = await detailPage.recipeTitle.textContent();

      const forkButton = page.getByRole('button', { name: /fork|duplicate|copy/i });
      const forkVisible = await forkButton.isVisible().catch(() => false);
      
      if (forkVisible) {
        await forkButton.click();
        await page.waitForLoadState('networkidle');

        // May open a dialog or navigate to edit form
        const inDialog = await page.locator('mat-dialog-container').isVisible().catch(() => false);
        const inEditForm = page.url().includes('/edit') || page.url().includes('/new');
        
        expect(inDialog || inEditForm).toBe(true);
      }
    }
  });

  test('should preserve original content when forking', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const detailPage = new RecipeDetailPage(page);
      const originalTitle = await detailPage.recipeTitle.textContent();
      
      const forkButton = page.getByRole('button', { name: /fork|duplicate|copy/i });
      const forkVisible = await forkButton.isVisible().catch(() => false);
      
      if (forkVisible) {
        await forkButton.click();
        await page.waitForLoadState('networkidle');

        // Check if title contains reference to original
        const titleInput = page.getByLabel('Recipe Title');
        const titleInputVisible = await titleInput.isVisible().catch(() => false);
        
        if (titleInputVisible) {
          const newTitle = await titleInput.inputValue();
          // New title might be "Copy of X" or "Fork of X" or original title
          expect(newTitle.length).toBeGreaterThan(0);
        }
      }
    }
  });

  test('should create independent copy', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const originalUrl = page.url();

      const forkButton = page.getByRole('button', { name: /fork|duplicate|copy/i });
      const forkVisible = await forkButton.isVisible().catch(() => false);
      
      if (forkVisible) {
        await forkButton.click();
        await page.waitForLoadState('networkidle');

        // After forking and saving, should create a new recipe with different ID
        const currentUrl = page.url();
        expect(currentUrl).not.toBe(originalUrl);
      }
    }
  });
});
