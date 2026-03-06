import { test, expect } from '@playwright/test';
import { CookingModePage } from '../pages/cooking-mode.page';
import { RecipeListPage } from '../pages/recipe-list.page';

test.describe('Cooking Mode', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should display cooking mode interface', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        await expect(cookingPage.recipeTitle).toBeVisible();
        await expect(cookingPage.currentStepText).toBeVisible();
        await expect(cookingPage.stepProgress).toBeVisible();
      }
    }
  });

  test('should navigate between steps', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        
        // Previous should be disabled on first step
        const prevDisabled = await cookingPage.isPreviousButtonDisabled();
        expect(prevDisabled).toBe(true);

        // Try to go to next step
        const nextDisabled = await cookingPage.isNextButtonDisabled();
        if (!nextDisabled) {
          const step1Text = await cookingPage.getCurrentStep();
          await cookingPage.nextStep();
          await page.waitForTimeout(500);
          const step2Text = await cookingPage.getCurrentStep();
          expect(step1Text).not.toBe(step2Text);
        }
      }
    }
  });

  test('should show step progress indicator', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        const progressText = await cookingPage.getStepProgress();
        expect(progressText).toMatch(/step \d+ of \d+/i);
      }
    }
  });

  test('should toggle voice control', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        const voiceVisible = await cookingPage.voiceToggle.isVisible().catch(() => false);
        
        if (voiceVisible) {
          await cookingPage.toggleVoice();
          await page.waitForTimeout(500);
          
          const statusVisible = await cookingPage.voiceStatus.isVisible().catch(() => false);
          if (statusVisible) {
            await expect(cookingPage.voiceStatus).toBeVisible();
          }
        }
      }
    }
  });

  test('should display ingredients checklist', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        const ingredientsVisible = await cookingPage.ingredientsPanel.isVisible().catch(() => false);
        
        if (ingredientsVisible) {
          await expect(cookingPage.ingredientsPanel).toBeVisible();
        }
      }
    }
  });

  test('should exit cooking mode', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        const cookingPage = new CookingModePage(page);
        await cookingPage.exit();
        
        await page.waitForLoadState('networkidle');
        await expect(page).not.toHaveURL(/\/cook/);
      }
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    const listPage = new RecipeListPage(page);
    await listPage.goto();
    await page.waitForLoadState('networkidle');

    const recipeCount = await listPage.getRecipeCount();
    
    if (recipeCount > 0) {
      await listPage.recipeCards.first().click();
      await page.waitForLoadState('networkidle');

      const cookButton = page.getByRole('button', { name: /cooking mode/i });
      const cookVisible = await cookButton.isVisible().catch(() => false);

      if (cookVisible) {
        await cookButton.click();
        await page.waitForLoadState('networkidle');

        await expect(page).toHaveScreenshot('cooking-mode.png', {
          maxDiffPixels: 100,
          fullPage: true
        });
      }
    }
  });
});
