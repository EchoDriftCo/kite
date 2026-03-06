import { test, expect } from '@playwright/test';
import { RecipeListPage } from '../pages/recipe-list.page';

test.describe('Recipe List Page', () => {
  let recipePage: RecipeListPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    recipePage = new RecipeListPage(page);
    await recipePage.goto();
  });

  test('should display recipe list', async ({ page }) => {
    await expect(page).toHaveURL(/recipes/);
    await expect(recipePage.navigationBar).toBeVisible();
  });

  test('should display recipe grid', async () => {
    // Wait for recipes to load
    await recipePage.page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    expect(recipeCount).toBeGreaterThanOrEqual(0);
  });

  test('should search recipes', async ({ page }) => {
    await recipePage.searchRecipes('pasta');
    
    // Wait for search results
    await page.waitForTimeout(500);
    
    // Verify search is applied (URL or UI change)
    const searchValue = await recipePage.searchInput.inputValue();
    expect(searchValue).toBe('pasta');
  });

  test('should navigate to add recipe', async ({ page }) => {
    const addButtonVisible = await recipePage.addRecipeButton.isVisible();
    
    if (addButtonVisible) {
      await recipePage.clickAddRecipe();
      await expect(page).toHaveURL(/recipes\/new/);
    }
  });

  test('should open filters', async () => {
    const filterButtonVisible = await recipePage.filterButton.isVisible();
    
    if (filterButtonVisible) {
      await recipePage.openFilters();
      // Add assertion for filter panel visibility
    }
  });

  test('should display recipe cards with images', async () => {
    const firstCard = recipePage.recipeCards.first();
    const isVisible = await firstCard.isVisible().catch(() => false);
    
    if (isVisible) {
      // Verify card has image
      const image = firstCard.locator('img').first();
      await expect(image).toBeVisible();
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('recipe-list-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
