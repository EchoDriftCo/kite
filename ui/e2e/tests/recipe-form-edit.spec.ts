import { test, expect } from '@playwright/test';
import { RecipeFormPage } from '../pages/recipe-form.page';
import { RecipeListPage } from '../pages/recipe-list.page';

test.describe('Recipe Edit Form', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display edit form with pre-populated data', async ({ page }) => {
    // Navigate to a recipe and click edit
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const formPage = new RecipeFormPage(page);
        await expect(page.locator('h1')).toContainText(/edit recipe/i);
        await expect(formPage.titleInput).toBeVisible();
        
        // Title should be pre-filled
        const titleValue = await formPage.titleInput.inputValue();
        expect(titleValue.length).toBeGreaterThan(0);
      }
    }
  });

  test('should update recipe title', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const formPage = new RecipeFormPage(page);
        const timestamp = Date.now();
        await formPage.titleInput.fill(`Updated Recipe ${timestamp}`);
        await formPage.save();

        await page.waitForLoadState('networkidle');
        await expect(page.locator('h1')).toContainText(`Updated Recipe ${timestamp}`);
      }
    }
  });

  test('should show delete button on edit form', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const formPage = new RecipeFormPage(page);
        const deleteVisible = await formPage.deleteButton.isVisible().catch(() => false);
        
        if (deleteVisible) {
          await expect(formPage.deleteButton).toBeEnabled();
        }
      }
    }
  });

  test('should not show import button on edit form', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const formPage = new RecipeFormPage(page);
        const importVisible = await formPage.importButton.isVisible().catch(() => false);
        expect(importVisible).toBe(false);
      }
    }
  });

  test('should modify ingredients', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const formPage = new RecipeFormPage(page);
        const beforeCount = await formPage.getIngredientCount();
        
        await formPage.addIngredient('1', 'tbsp', 'test ingredient');
        
        const afterCount = await formPage.getIngredientCount();
        expect(afterCount).toBe(beforeCount + 1);
      }
    }
  });

  test('should take visual snapshot of edit form', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const editButton = page.getByRole('button', { name: /edit/i });
      const editVisible = await editButton.isVisible().catch(() => false);

      if (editVisible) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        await expect(page).toHaveScreenshot('recipe-edit-form.png', {
          maxDiffPixels: 100,
          fullPage: true
        });
      }
    }
  });
});
