import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class SharedRecipePage extends BasePage {
  readonly recipeTitle: Locator;
  readonly recipeImage: Locator;
  readonly ingredientsList: Locator;
  readonly instructionsList: Locator;
  readonly servingsInput: Locator;
  readonly prepTime: Locator;
  readonly cookTime: Locator;
  readonly description: Locator;
  readonly signUpButton: Locator;
  readonly loginButton: Locator;

  constructor(page: Page) {
    super(page);
    this.recipeTitle = page.locator('h1').first();
    this.recipeImage = page.locator('img[alt*="recipe"]').first();
    this.ingredientsList = page.locator('[data-testid="ingredients-list"]');
    this.instructionsList = page.locator('[data-testid="instructions-list"]');
    this.servingsInput = page.getByLabel(/servings/i);
    this.prepTime = page.locator('[data-testid="prep-time"]');
    this.cookTime = page.locator('[data-testid="cook-time"]');
    this.description = page.locator('.recipe-description');
    this.signUpButton = page.getByRole('button', { name: /sign up/i });
    this.loginButton = page.getByRole('button', { name: /log in|sign in/i });
  }

  async gotoSharedRecipe(token: string) {
    await super.goto(`/share/${token}`);
  }

  async getIngredients(): Promise<string[]> {
    const items = await this.ingredientsList.locator('li').allTextContents();
    return items;
  }

  async getInstructions(): Promise<string[]> {
    const items = await this.instructionsList.locator('li').allTextContents();
    return items;
  }

  async changeServings(servings: number) {
    await this.servingsInput.fill(servings.toString());
  }
}
