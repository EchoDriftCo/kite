import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class RecipeGeneratorPage extends BasePage {
  readonly promptInput: Locator;
  readonly generateButton: Locator;
  readonly maxTimeSelect: Locator;
  readonly skillLevelSelect: Locator;
  readonly constraintsPanel: Locator;
  readonly loadingSpinner: Locator;
  readonly recipePreview: Locator;
  readonly saveButton: Locator;
  readonly regenerateButton: Locator;
  readonly refineButton: Locator;
  readonly startOverButton: Locator;
  readonly quotaBadge: Locator;
  readonly refinementInput: Locator;
  readonly applyRefinementButton: Locator;

  constructor(page: Page) {
    super(page);
    this.promptInput = page.getByLabel('Describe your recipe');
    this.generateButton = page.getByRole('button', { name: /generate recipe/i });
    this.maxTimeSelect = page.getByLabel('Max Time');
    this.skillLevelSelect = page.getByLabel('Skill Level');
    this.constraintsPanel = page.locator('.constraints-panel');
    this.loadingSpinner = page.locator('mat-spinner');
    this.recipePreview = page.locator('.recipe-preview');
    this.saveButton = page.getByRole('button', { name: /save to my recipes/i });
    this.regenerateButton = page.getByRole('button', { name: /regenerate/i });
    this.refineButton = page.getByRole('button', { name: /refine/i });
    this.startOverButton = page.getByRole('button', { name: /start over/i });
    this.quotaBadge = page.locator('.quota-badge');
    this.refinementInput = page.getByLabel(/how would you like to refine/i);
    this.applyRefinementButton = page.getByRole('button', { name: /apply refinement/i });
  }

  async goto() {
    await super.goto('/recipes/generate');
  }

  async enterPrompt(text: string) {
    await this.promptInput.fill(text);
  }

  async expandConstraints() {
    const isExpanded = await this.constraintsPanel.getAttribute('aria-expanded');
    if (isExpanded !== 'true') {
      await this.constraintsPanel.click();
    }
  }

  async setMaxTime(minutes: number) {
    await this.expandConstraints();
    await this.maxTimeSelect.selectOption({ label: `${minutes} minutes` });
  }

  async setSkillLevel(level: string) {
    await this.expandConstraints();
    await this.skillLevelSelect.selectOption({ label: level });
  }

  async selectDietaryConstraint(constraint: string) {
    await this.expandConstraints();
    await this.page.getByRole('checkbox', { name: constraint }).check();
  }

  async generate() {
    await this.generateButton.click();
  }

  async save() {
    await this.saveButton.click();
  }

  async regenerate() {
    await this.regenerateButton.click();
  }

  async refine(text: string) {
    await this.refineButton.click();
    await this.refinementInput.fill(text);
    await this.applyRefinementButton.click();
  }

  async startOver() {
    await this.startOverButton.click();
  }

  async isLoading(): Promise<boolean> {
    return await this.loadingSpinner.isVisible().catch(() => false);
  }

  async hasPreview(): Promise<boolean> {
    return await this.recipePreview.isVisible().catch(() => false);
  }

  async getQuotaRemaining(): Promise<number> {
    const text = await this.quotaBadge.textContent() || '0';
    const match = text.match(/(\d+)/);
    return match ? parseInt(match[1]) : 0;
  }
}
