import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class CircleDetailPage extends BasePage {
  readonly circleTitle: Locator;
  readonly description: Locator;
  readonly backButton: Locator;
  readonly menuButton: Locator;
  readonly editButton: Locator;
  readonly deleteButton: Locator;
  readonly leaveButton: Locator;
  readonly recipesTab: Locator;
  readonly membersTab: Locator;
  readonly inviteButton: Locator;
  readonly recipeCards: Locator;
  readonly membersList: Locator;
  readonly emptyState: Locator;

  constructor(page: Page) {
    super(page);
    this.circleTitle = page.locator('h1').first();
    this.description = page.locator('.description');
    this.backButton = page.getByRole('button', { name: /arrow_back/i }).first();
    this.menuButton = page.getByRole('button', { name: /more_vert/i });
    this.editButton = page.getByRole('menuitem', { name: /edit circle/i });
    this.deleteButton = page.getByRole('menuitem', { name: /delete circle/i });
    this.leaveButton = page.getByRole('button', { name: /leave circle/i });
    this.recipesTab = page.getByRole('tab', { name: /recipes/i });
    this.membersTab = page.getByRole('tab', { name: /members/i });
    this.inviteButton = page.getByRole('button', { name: /invite members/i });
    this.recipeCards = page.locator('.recipe-card');
    this.membersList = page.locator('.members-list');
    this.emptyState = page.locator('.empty-state');
  }

  async gotoCircle(circleId: string) {
    await super.goto(`/circles/${circleId}`);
  }

  async goBack() {
    await this.backButton.click();
  }

  async openMenu() {
    await this.menuButton.click();
  }

  async editCircle() {
    await this.openMenu();
    await this.editButton.click();
  }

  async deleteCircle() {
    await this.openMenu();
    await this.deleteButton.click();
  }

  async leaveCircle() {
    await this.leaveButton.click();
  }

  async switchToRecipesTab() {
    await this.recipesTab.click();
  }

  async switchToMembersTab() {
    await this.membersTab.click();
  }

  async inviteMembers() {
    await this.switchToMembersTab();
    await this.inviteButton.click();
  }

  async getRecipeCount(): Promise<number> {
    return await this.recipeCards.count();
  }

  async getMemberCount(): Promise<number> {
    await this.switchToMembersTab();
    return await this.membersList.locator('.member-item').count();
  }

  async viewRecipe(index: number) {
    await this.recipeCards.nth(index).click();
  }
}
