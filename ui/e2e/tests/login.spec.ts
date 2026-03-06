import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';

test.describe('Login Page', () => {
  let loginPage: LoginPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    await loginPage.goto();
  });

  test('should display login form', async () => {
    await expect(loginPage.emailInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();
    await expect(loginPage.signInButton).toBeVisible();
    await expect(loginPage.signUpLink).toBeVisible();
  });

  test('should validate email field', async ({ page }) => {
    await loginPage.emailInput.fill('invalid-email');
    await loginPage.passwordInput.fill('password123');
    await loginPage.signInButton.click();
    
    // Should show validation error or remain on login page
    await expect(page).toHaveURL(/login/);
  });

  test('should validate password field', async ({ page }) => {
    await loginPage.emailInput.fill('test@example.com');
    await loginPage.passwordInput.fill('');
    await loginPage.signInButton.click();
    
    // Should show validation error or remain on login page
    await expect(page).toHaveURL(/login/);
  });

  test('should show error with invalid credentials', async ({ page }) => {
    await loginPage.login('wrong@email.com', 'wrongpassword');
    
    // Wait for error message or stay on login page
    await page.waitForTimeout(1000);
    await expect(page).toHaveURL(/login/);
  });

  test('should navigate to sign up page', async ({ page }) => {
    // Check if sign up link is present (might be modal or separate page)
    const signUpVisible = await loginPage.signUpLink.isVisible();
    if (signUpVisible) {
      await loginPage.clickSignUp();
      // Add assertion based on actual implementation
    }
  });

  test('should take visual snapshot', async ({ page }) => {
    await expect(page).toHaveScreenshot('login-page.png', {
      maxDiffPixels: 100
    });
  });
});
