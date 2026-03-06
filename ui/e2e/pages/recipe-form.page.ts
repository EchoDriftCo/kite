import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class RecipeFormPage extends BasePage {
  readonly titleInput: Locator;
  readonly descriptionInput: Locator;
  readonly servingsInput: Locator;
  readonly prepTimeInput: Locator;
  readonly cookTimeInput: Locator;
  readonly sourceInput: Locator;
  readonly imageUrlInput: Locator;
  readonly publicToggle: Locator;
  readonly addIngredientButton: Locator;
  readonly addInstructionButton: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;
  readonly importButton: Locator;
  readonly ingredientsList: Locator;
  readonly instructionsList: Locator;
  readonly deleteButton: Locator;

  constructor(page: Page) {
    super(page);
    this.titleInput = page.getByLabel('Recipe Title');
    this.descriptionInput = page.getByLabel('Description');
    this.servingsInput = page.getByLabel('Servings');
    this.prepTimeInput = page.getByLabel('Prep Time');
    this.cookTimeInput = page.getByLabel('Cook Time');
    this.sourceInput = page.getByLabel('Source');
    this.imageUrlInput = page.getByLabel('Image URL');
    this.publicToggle = page.getByRole('checkbox', { name: /make this recipe public/i });
    this.addIngredientButton = page.getByRole('button', { name: /add ingredient/i }).first();
    this.addInstructionButton = page.getByRole('button', { name: /add instruction/i }).first();
    this.saveButton = page.getByRole('button', { name: /create recipe|update recipe/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
    this.importButton = page.getByRole('button', { name: /import recipe/i });
    this.ingredientsList = page.locator('.ingredients-list');
    this.instructionsList = page.locator('.instructions-list');
    this.deleteButton = page.getByRole('button', { name: /delete/i });
  }

  async gotoNew() {
    await super.goto('/recipes/new');
  }

  async gotoEdit(recipeId: string) {
    await super.goto(`/recipes/${recipeId}/edit`);
  }

  async fillBasicInfo(title: string, description: string, servings: number) {
    await this.titleInput.fill(title);
    await this.descriptionInput.fill(description);
    await this.servingsInput.fill(servings.toString());
  }

  async fillTimes(prepMinutes: number, cookMinutes: number) {
    await this.prepTimeInput.fill(prepMinutes.toString());
    await this.cookTimeInput.fill(cookMinutes.toString());
  }

  async addIngredient(quantity: string, unit: string, item: string, preparation?: string) {
    await this.addIngredientButton.click();
    
    const rows = this.ingredientsList.locator('.ingredient-row');
    const lastRow = rows.last();
    
    await lastRow.getByLabel('Qty').fill(quantity);
    await lastRow.getByLabel('Unit').fill(unit);
    await lastRow.getByLabel('Ingredient').fill(item);
    
    if (preparation) {
      await lastRow.getByLabel('Preparation').fill(preparation);
    }
  }

  async addInstruction(text: string) {
    await this.addInstructionButton.click();
    
    const rows = this.instructionsList.locator('.instruction-row');
    const lastRow = rows.last();
    const stepNumber = await rows.count();
    
    await lastRow.getByLabel(`Step ${stepNumber}`).fill(text);
  }

  async togglePublic() {
    await this.publicToggle.click();
  }

  async save() {
    await this.saveButton.click();
  }

  async cancel() {
    await this.cancelButton.click();
  }

  async openImportDialog() {
    await this.importButton.click();
  }

  async getIngredientCount(): Promise<number> {
    return await this.ingredientsList.locator('.ingredient-row').count();
  }

  async getInstructionCount(): Promise<number> {
    return await this.instructionsList.locator('.instruction-row').count();
  }
}
