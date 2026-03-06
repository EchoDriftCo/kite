import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class RecipeMixerPage extends BasePage {
  readonly recipeAInput: Locator;
  readonly recipeBInput: Locator;
  readonly modeSelect: Locator;
  readonly intentInput: Locator;
  readonly mixButton: Locator;
  readonly loadingSpinner: Locator;
  readonly preview: Locator;
  readonly saveButton: Locator;
  readonly regenerateButton: Locator;
  readonly resetButton: Locator;
  readonly refinementInput: Locator;
  readonly refineButton: Locator;

  constructor(page: Page) {
    super(page);
    this.recipeAInput = page.getByLabel('First Recipe');
    this.recipeBInput = page.getByLabel('Second Recipe');
    this.modeSelect = page.getByLabel('Mixing Mode');
    this.intentInput = page.getByLabel('Your Intent');
    this.mixButton = page.getByRole('button', { name: /mix recipes/i });
    this.loadingSpinner = page.locator('mat-spinner');
    this.preview = page.locator('.preview-section');
    this.saveButton = page.getByRole('button', { name: /save recipe/i });
    this.regenerateButton = page.getByRole('button', { name: /regenerate/i });
    this.resetButton = page.getByRole('button', { name: /start over/i });
    this.refinementInput = page.getByLabel(/refine this recipe/i);
    this.refineButton = page.getByRole('button', { name: /refine/i });
  }

  async goto() {
    await super.goto('/recipes/mix');
  }

  async selectRecipeA(recipeName: string) {
    await this.recipeAInput.fill(recipeName);
    await this.page.getByRole('option', { name: recipeName }).click();
  }

  async selectRecipeB(recipeName: string) {
    await this.recipeBInput.fill(recipeName);
    await this.page.getByRole('option', { name: recipeName }).click();
  }

  async setMode(mode: 'bestOfBoth' | 'guided' | 'surprise') {
    const modeLabel = mode === 'bestOfBoth' ? 'Best of Both' : 
                      mode === 'guided' ? 'Guided (with intent)' : 'Surprise Me!';
    await this.modeSelect.selectOption({ label: modeLabel });
  }

  async setIntent(text: string) {
    await this.intentInput.fill(text);
  }

  async mix() {
    await this.mixButton.click();
  }

  async save() {
    await this.saveButton.click();
  }

  async regenerate() {
    await this.regenerateButton.click();
  }

  async reset() {
    await this.resetButton.click();
  }

  async refine(text: string) {
    await this.refinementInput.fill(text);
    await this.refineButton.click();
  }

  async isLoading(): Promise<boolean> {
    return await this.loadingSpinner.isVisible().catch(() => false);
  }

  async hasPreview(): Promise<boolean> {
    return await this.preview.isVisible().catch(() => false);
  }

  async isMixButtonEnabled(): Promise<boolean> {
    return await this.mixButton.isEnabled();
  }
}
