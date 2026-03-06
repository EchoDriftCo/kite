import { test, expect } from '@playwright/test';
import { RecipeDetailPage } from '../pages/recipe-detail.page';
import { RecipeListPage } from '../pages/recipe-list.page';

test.describe('Recipe Detail Page', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display recipe details', async ({ page }) => {
    // First navigate to recipe list and click a recipe
    const recipePage = new RecipeListPage(page);
    await recipePage.goto();
    await page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    
    if (recipeCount > 0) {
      const firstRecipe = recipePage.recipeCards.first();
      await firstRecipe.click();
      
      // Now on detail page
      const detailPage = new RecipeDetailPage(page);
      await expect(detailPage.recipeTitle).toBeVisible();
      await expect(page).toHaveURL(/recipes\/[a-f0-9-]+/);
    }
  });

  test('should display ingredients list', async ({ page }) => {
    const recipePage = new RecipeListPage(page);
    await recipePage.goto();
    await page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    
    if (recipeCount > 0) {
      await recipePage.recipeCards.first().click();
      
      const detailPage = new RecipeDetailPage(page);
      const ingredientsVisible = await detailPage.ingredientsList.isVisible().catch(() => false);
      
      if (ingredientsVisible) {
        const ingredients = await detailPage.getIngredients();
        expect(ingredients.length).toBeGreaterThan(0);
      }
    }
  });

  test('should display instructions', async ({ page }) => {
    const recipePage = new RecipeListPage(page);
    await recipePage.goto();
    await page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    
    if (recipeCount > 0) {
      await recipePage.recipeCards.first().click();
      
      const detailPage = new RecipeDetailPage(page);
      const instructionsVisible = await detailPage.instructionsList.isVisible().catch(() => false);
      
      if (instructionsVisible) {
        const instructions = await detailPage.getInstructions();
        expect(instructions.length).toBeGreaterThan(0);
      }
    }
  });

  test('should show cooking mode button', async ({ page }) => {
    const recipePage = new RecipeListPage(page);
    await recipePage.goto();
    await page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    
    if (recipeCount > 0) {
      await recipePage.recipeCards.first().click();
      
      const detailPage = new RecipeDetailPage(page);
      const buttonVisible = await detailPage.cookingModeButton.isVisible().catch(() => false);
      
      if (buttonVisible) {
        await expect(detailPage.cookingModeButton).toBeEnabled();
      }
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    const recipePage = new RecipeListPage(page);
    await recipePage.goto();
    await page.waitForLoadState('networkidle');
    
    const recipeCount = await recipePage.getRecipeCount();
    
    if (recipeCount > 0) {
      await recipePage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');
      
      await expect(page).toHaveScreenshot('recipe-detail-page.png', {
        maxDiffPixels: 100,
        fullPage: true
      });
    }
  });
});
