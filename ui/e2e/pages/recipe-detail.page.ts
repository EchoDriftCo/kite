import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class RecipeDetailPage extends BasePage {
  readonly recipeTitle: Locator;
  readonly recipeImage: Locator;
  readonly ingredientsList: Locator;
  readonly instructionsList: Locator;
  readonly cookingModeButton: Locator;
  readonly editButton: Locator;
  readonly deleteButton: Locator;
  readonly shareButton: Locator;
  readonly favoriteButton: Locator;
  readonly nutritionPanel: Locator;
  readonly servingsInput: Locator;
  readonly prepTime: Locator;
  readonly cookTime: Locator;
  readonly totalTime: Locator;

  constructor(page: Page) {
    super(page);
    this.recipeTitle = page.locator('h1').first();
    this.recipeImage = page.locator('img[alt*="recipe"]').first();
    this.ingredientsList = page.locator('[data-testid="ingredients-list"]');
    this.instructionsList = page.locator('[data-testid="instructions-list"]');
    this.cookingModeButton = page.getByRole('button', { name: /cooking mode/i });
    this.editButton = page.getByRole('button', { name: /edit/i });
    this.deleteButton = page.getByRole('button', { name: /delete/i });
    this.shareButton = page.getByRole('button', { name: /share/i });
    this.favoriteButton = page.getByRole('button', { name: /favorite/i });
    this.nutritionPanel = page.locator('[data-testid="nutrition-panel"]');
    this.servingsInput = page.getByLabel(/servings/i);
    this.prepTime = page.locator('[data-testid="prep-time"]');
    this.cookTime = page.locator('[data-testid="cook-time"]');
    this.totalTime = page.locator('[data-testid="total-time"]');
  }

  async gotoRecipe(recipeId: string) {
    await super.goto(`/recipes/${recipeId}`);
  }

  async startCookingMode() {
    await this.cookingModeButton.click();
  }

  async editRecipe() {
    await this.editButton.click();
  }

  async shareRecipe() {
    await this.shareButton.click();
  }

  async toggleFavorite() {
    await this.favoriteButton.click();
  }

  async changeServings(servings: number) {
    await this.servingsInput.fill(servings.toString());
  }

  async getIngredients(): Promise<string[]> {
    const items = await this.ingredientsList.locator('li').allTextContents();
    return items;
  }

  async getInstructions(): Promise<string[]> {
    const items = await this.instructionsList.locator('li').allTextContents();
    return items;
  }
}
