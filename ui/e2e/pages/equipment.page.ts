import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class EquipmentPage extends BasePage {
  readonly equipmentList: Locator;
  readonly addEquipmentButton: Locator;
  readonly equipmentNameInput: Locator;
  readonly equipmentTypeSelect: Locator;
  readonly saveButton: Locator;
  readonly deleteButton: Locator;

  constructor(page: Page) {
    super(page);
    this.equipmentList = page.locator('[data-testid="equipment-list"]');
    this.addEquipmentButton = page.getByRole('button', { name: /add equipment/i });
    this.equipmentNameInput = page.getByLabel(/equipment name/i);
    this.equipmentTypeSelect = page.getByLabel(/equipment type/i);
    this.saveButton = page.getByRole('button', { name: /save/i });
    this.deleteButton = page.getByRole('button', { name: /delete/i });
  }

  async goto() {
    await super.goto('/equipment');
  }

  async clickAddEquipment() {
    await this.addEquipmentButton.click();
  }

  async addEquipment(name: string, type: string) {
    await this.clickAddEquipment();
    await this.equipmentNameInput.fill(name);
    await this.equipmentTypeSelect.selectOption(type);
    await this.saveButton.click();
  }

  async getEquipmentCount(): Promise<number> {
    const items = await this.equipmentList.locator('li, [data-testid="equipment-item"]');
    return await items.count();
  }

  async deleteEquipment(equipmentName: string) {
    await this.page.getByText(equipmentName).first().click();
    await this.deleteButton.click();
  }
}
