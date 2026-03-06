# E2E Test Setup Guide

Quick setup guide for running Playwright E2E tests on RecipeVault.

## 🚀 Quick Start

### 1. Install Dependencies (if not already done)
```bash
cd ui
npm install --legacy-peer-deps
```

### 2. Install Playwright Browsers
```bash
npx playwright install
```

### 3. Set Up Test Credentials

You need a test account in Supabase to run the E2E tests.

**Option A: Environment Variables (Recommended for local dev)**

Windows PowerShell:
```powershell
$env:TEST_USER_EMAIL="your-test-email@example.com"
$env:TEST_USER_PASSWORD="YourTestPassword123!"
```

Linux/Mac:
```bash
export TEST_USER_EMAIL="your-test-email@example.com"
export TEST_USER_PASSWORD="YourTestPassword123!"
```

**Option B: Update auth.setup.ts**

Edit `e2e/fixtures/auth.setup.ts` and change the default credentials (line 16-17):
```typescript
const email = process.env.TEST_USER_EMAIL || 'your-test-email@example.com';
const password = process.env.TEST_USER_PASSWORD || 'YourTestPassword123!';
```

### 4. Run Tests

```bash
# Run all tests
npm run test:e2e

# Run with UI (recommended for first time)
npm run test:e2e:ui

# Run in debug mode
npm run test:e2e:debug
```

## 📋 Test Account Requirements

Your test account should have:
- ✅ Valid email/password in Supabase Auth
- ✅ At least one recipe (optional, but recommended)
- ✅ Access to create circles, meal plans, etc.

## 🎯 Running Specific Tests

```bash
# Just login tests
npx playwright test e2e/tests/login.spec.ts

# Just recipe tests
npx playwright test e2e/tests/recipe-list.spec.ts
npx playwright test e2e/tests/recipe-detail.spec.ts

# Run on specific browser
npx playwright test --project=chromium
npx playwright test --project="Mobile Chrome"
```

## 🐛 Troubleshooting

### "Test account credentials not set"
- Make sure environment variables are set in your current terminal session
- Or update the default credentials in `auth.setup.ts`

### "Authentication failed"
- Verify your test account exists in Supabase
- Check that the credentials are correct
- Make sure the Supabase project is accessible

### "Could not connect to server"
- Make sure the Angular dev server is running (`npm start`)
- Or let Playwright start it automatically (configured in `playwright.config.ts`)

### Visual snapshot failures
- Run `npx playwright test --update-snapshots` to regenerate baseline snapshots
- This is normal after intentional UI changes

## 📊 CI/CD Setup

For GitHub Actions, add these secrets to your repository:
1. Go to Settings > Secrets and variables > Actions
2. Add `TEST_USER_EMAIL`
3. Add `TEST_USER_PASSWORD`

The workflow is configured in `.github/workflows/e2e.yml`.

## 📚 Learn More

See the full documentation in `e2e/README.md`.

## 🆘 Need Help?

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Discord](https://discord.com/invite/playwright-807756831384403968)
- Check existing tests in `e2e/tests/` for examples
