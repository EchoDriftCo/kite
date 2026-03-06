import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CookingModePage extends BasePage {
  readonly recipeTitle: Locator;
  readonly voiceToggle: Locator;
  readonly exitButton: Locator;
  readonly currentStepText: Locator;
  readonly stepProgress: Locator;
  readonly previousButton: Locator;
  readonly nextButton: Locator;
  readonly readStepButton: Locator;
  readonly timerChips: Locator;
  readonly activeTimersPanel: Locator;
  readonly ingredientsPanel: Locator;
  readonly voiceStatus: Locator;

  constructor(page: Page) {
    super(page);
    this.recipeTitle = page.locator('.recipe-title');
    this.voiceToggle = page.getByRole('button', { name: /voice control/i });
    this.exitButton = page.getByRole('button', { name: /exit cooking mode/i });
    this.currentStepText = page.locator('.step-instruction');
    this.stepProgress = page.locator('.step-progress');
    this.previousButton = page.getByRole('button', { name: /previous/i });
    this.nextButton = page.getByRole('button', { name: /next/i });
    this.readStepButton = page.getByRole('button', { name: /read step aloud/i });
    this.timerChips = page.locator('.timer-chip');
    this.activeTimersPanel = page.locator('.active-timers-panel');
    this.ingredientsPanel = page.locator('.ingredients-panel');
    this.voiceStatus = page.locator('.voice-status');
  }

  async gotoCookingMode(recipeId: string) {
    await super.goto(`/recipes/${recipeId}/cook`);
  }

  async toggleVoice() {
    await this.voiceToggle.click();
  }

  async exit() {
    await this.exitButton.click();
  }

  async nextStep() {
    await this.nextButton.click();
  }

  async previousStep() {
    await this.previousButton.click();
  }

  async readStep() {
    await this.readStepButton.click();
  }

  async startTimer(timerLabel: string) {
    await this.page.getByRole('button', { name: timerLabel }).click();
  }

  async getCurrentStep(): Promise<string> {
    return await this.currentStepText.textContent() || '';
  }

  async getStepProgress(): Promise<string> {
    return await this.stepProgress.textContent() || '';
  }

  async isNextButtonDisabled(): Promise<boolean> {
    return await this.nextButton.isDisabled();
  }

  async isPreviousButtonDisabled(): Promise<boolean> {
    return await this.previousButton.isDisabled();
  }

  async getActiveTimerCount(): Promise<number> {
    const visible = await this.activeTimersPanel.isVisible().catch(() => false);
    if (!visible) return 0;
    return await this.activeTimersPanel.locator('.active-timer').count();
  }
}
