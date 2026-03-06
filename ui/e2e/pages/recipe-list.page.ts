import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class RecipeListPage extends BasePage {
  readonly searchInput: Locator;
  readonly addRecipeButton: Locator;
  readonly recipeCards: Locator;
  readonly filterButton: Locator;
  readonly sortDropdown: Locator;
  readonly viewToggle: Locator;
  readonly generateRecipeButton: Locator;
  readonly mixRecipeButton: Locator;

  constructor(page: Page) {
    super(page);
    this.searchInput = page.getByPlaceholder(/search recipes/i);
    this.addRecipeButton = page.getByRole('button', { name: /add recipe/i });
    this.recipeCards = page.locator('[data-testid="recipe-card"]');
    this.filterButton = page.getByRole('button', { name: /filter/i });
    this.sortDropdown = page.getByLabel('Sort by');
    this.viewToggle = page.getByRole('button', { name: /view/i });
    this.generateRecipeButton = page.getByRole('button', { name: /generate/i });
    this.mixRecipeButton = page.getByRole('button', { name: /mix/i });
  }

  async goto() {
    await super.goto('/recipes');
  }

  async searchRecipes(query: string) {
    await this.searchInput.fill(query);
  }

  async clickAddRecipe() {
    await this.addRecipeButton.click();
  }

  async clickRecipe(recipeName: string) {
    await this.page.getByText(recipeName).first().click();
  }

  async getRecipeCount(): Promise<number> {
    return await this.recipeCards.count();
  }

  async openFilters() {
    await this.filterButton.click();
  }

  async changeSort(sortOption: string) {
    await this.sortDropdown.selectOption(sortOption);
  }
}
