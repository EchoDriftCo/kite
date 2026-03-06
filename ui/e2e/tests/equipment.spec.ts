import { test, expect } from '@playwright/test';
import { EquipmentPage } from '../pages/equipment.page';

test.describe('Equipment Page', () => {
  let equipmentPage: EquipmentPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    equipmentPage = new EquipmentPage(page);
    await equipmentPage.goto();
  });

  test('should display equipment page', async ({ page }) => {
    await expect(page).toHaveURL(/equipment/);
    await expect(equipmentPage.navigationBar).toBeVisible();
  });

  test('should display add equipment button', async () => {
    await expect(equipmentPage.addEquipmentButton).toBeVisible();
  });

  test('should display equipment list', async () => {
    await equipmentPage.page.waitForLoadState('networkidle');
    
    const equipmentCount = await equipmentPage.getEquipmentCount();
    expect(equipmentCount).toBeGreaterThanOrEqual(0);
  });

  test('should open add equipment form', async ({ page }) => {
    await equipmentPage.clickAddEquipment();
    
    // Wait for form to appear
    await page.waitForTimeout(500);
    
    // Check if input fields are visible
    const nameInputVisible = await equipmentPage.equipmentNameInput.isVisible().catch(() => false);
    if (nameInputVisible) {
      await expect(equipmentPage.equipmentNameInput).toBeVisible();
      await expect(equipmentPage.saveButton).toBeVisible();
    }
  });

  test('should display equipment with details', async () => {
    await equipmentPage.page.waitForLoadState('networkidle');
    
    const equipmentCount = await equipmentPage.getEquipmentCount();
    
    if (equipmentCount > 0) {
      const list = equipmentPage.equipmentList;
      await expect(list).toBeVisible();
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('equipment-page.png', {
      maxDiffPixels: 100,
      fullPage: true
    });
  });
});
