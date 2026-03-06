import { test as setup, expect } from '@playwright/test';
import path from 'path';

const authFile = path.join(__dirname, '../../.auth/user.json');

/**
 * Authentication setup for Playwright tests
 * 
 * This setup authenticates with Supabase using a test account and saves the session.
 * Other tests can reuse this authentication state to avoid logging in for every test.
 * 
 * Configure test credentials via environment variables:
 * - TEST_USER_EMAIL
 * - TEST_USER_PASSWORD
 */
setup('authenticate', async ({ page }) => {
  const email = process.env.TEST_USER_EMAIL || 'test@recipevault.io';
  const password = process.env.TEST_USER_PASSWORD || 'TestPassword123!';

  await page.goto('/login');
  
  // Fill in login form
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  
  // Click sign in button
  await page.getByRole('button', { name: /sign in/i }).click();
  
  // Wait for navigation to recipes page (successful login)
  await page.waitForURL('/recipes', { timeout: 10000 });
  
  // Verify we're authenticated by checking for user-specific content
  await expect(page.getByRole('navigation')).toBeVisible();
  
  // Save authentication state
  await page.context().storageState({ path: authFile });
});
