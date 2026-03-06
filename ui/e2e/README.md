# RecipeVault E2E Tests

Comprehensive end-to-end test suite for the RecipeVault Angular application using Playwright.

## 🏗️ Architecture

The test suite follows the **Page Object Model (POM)** pattern for maintainability and reusability.

### Directory Structure

```
e2e/
├── fixtures/          # Test setup and utilities
│   └── auth.setup.ts  # Authentication setup
├── pages/             # Page Object Models
│   ├── base.page.ts            # Base page with common functionality
│   ├── login.page.ts           # Login page
│   ├── recipe-list.page.ts     # Recipe list page
│   ├── recipe-detail.page.ts   # Recipe detail page
│   ├── circles.page.ts         # Circles page
│   ├── dietary-profiles.page.ts # Dietary profiles page
│   ├── meal-plans.page.ts      # Meal plans page
│   ├── cooking-history.page.ts # Cooking history page
│   ├── cooking-stats.page.ts   # Cooking stats page
│   └── equipment.page.ts       # Equipment page
├── tests/             # Test specifications
│   ├── login.spec.ts           # Login tests
│   ├── recipe-list.spec.ts     # Recipe list tests
│   ├── recipe-detail.spec.ts   # Recipe detail tests
│   ├── circles.spec.ts         # Circles tests
│   ├── dietary-profiles.spec.ts # Dietary profiles tests
│   ├── meal-plans.spec.ts      # Meal plans tests
│   ├── cooking-history.spec.ts # Cooking history tests
│   ├── cooking-stats.spec.ts   # Cooking stats tests
│   ├── equipment.spec.ts       # Equipment tests
│   └── navigation.spec.ts      # Navigation tests
└── snapshots/         # Visual regression snapshots
```

## 🚀 Getting Started

### Prerequisites

- Node.js 18+ installed
- npm or yarn
- Test account credentials for Supabase

### Installation

1. Install Playwright and dependencies:
   ```bash
   cd ui
   npm install --legacy-peer-deps
   ```

2. Install Playwright browsers:
   ```bash
   npx playwright install
   ```

### Configuration

Set up test credentials as environment variables:

```bash
# Windows (PowerShell)
$env:TEST_USER_EMAIL="test@recipevault.io"
$env:TEST_USER_PASSWORD="YourTestPassword123!"

# Linux/Mac
export TEST_USER_EMAIL="test@recipevault.io"
export TEST_USER_PASSWORD="YourTestPassword123!"
```

For CI/CD, add these as secrets in your GitHub repository:
- `TEST_USER_EMAIL`
- `TEST_USER_PASSWORD`

## 🧪 Running Tests

### All Tests
```bash
npm run test:e2e
```

### UI Mode (Interactive)
```bash
npm run test:e2e:ui
```

### Debug Mode
```bash
npm run test:e2e:debug
```

### Specific Test File
```bash
npx playwright test e2e/tests/login.spec.ts
```

### Specific Browser
```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project="Mobile Chrome"
npx playwright test --project="Mobile Safari"
```

### Headed Mode (See Browser)
```bash
npx playwright test --headed
```

### View Report
```bash
npm run test:e2e:report
```

## 📊 Test Coverage

### Pages Covered

- ✅ Login page (form validation, authentication)
- ✅ Recipe list (grid display, search, filters)
- ✅ Recipe detail (ingredients, steps, cooking mode)
- ✅ Circles (list, create, invite)
- ✅ Dietary Profiles (allergens, preferences)
- ✅ Meal Plans (list, create, detail)
- ✅ Cooking History (log, filters)
- ✅ Cooking Stats (metrics, time ranges)
- ✅ Equipment (list, add, manage)
- ✅ Navigation (desktop and mobile)

### Test Types

1. **Functional Tests** - Verify features work as expected
2. **Visual Regression Tests** - Detect unintended UI changes
3. **Mobile Tests** - Ensure responsive design works
4. **Cross-browser Tests** - Test on Chrome, Firefox, and WebKit

## 🎭 Browser Matrix

| Browser | Desktop | Mobile |
|---------|---------|--------|
| Chromium | ✅ | ✅ (Pixel 5) |
| Firefox | ✅ | ❌ |
| WebKit | ❌ | ✅ (iPhone 12) |

## 📸 Visual Snapshots

Visual regression tests capture screenshots of key pages. Snapshots are stored in `e2e/snapshots/`.

To update snapshots after intentional UI changes:
```bash
npx playwright test --update-snapshots
```

## 🔐 Authentication

The test suite uses a global authentication setup that:
1. Logs in once before all tests
2. Saves the authentication state to `.auth/user.json`
3. Reuses this state across all tests

This approach:
- Speeds up test execution
- Reduces Supabase API calls
- Avoids rate limiting

## 🐛 Debugging

### Screenshot on Failure
Screenshots are automatically captured on test failure and saved to `test-results/`.

### Video Recording
Videos are recorded for failed tests (configured in `playwright.config.ts`).

### Playwright Inspector
```bash
npm run test:e2e:debug
```

### Console Logs
```bash
npx playwright test --reporter=list
```

## 🚢 CI/CD Integration

Tests run automatically on:
- Push to `main` branch
- Pull requests to `main`

GitHub Actions workflow: `.github/workflows/e2e.yml`

### Artifacts

The workflow uploads:
- Test reports (HTML)
- Screenshots (on failure)
- Visual snapshots
- Video recordings (on failure)

## 📝 Writing New Tests

### 1. Create a Page Object Model

```typescript
import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class MyNewPage extends BasePage {
  readonly someButton: Locator;

  constructor(page: Page) {
    super(page);
    this.someButton = page.getByRole('button', { name: /click me/i });
  }

  async goto() {
    await super.goto('/my-new-page');
  }

  async clickButton() {
    await this.someButton.click();
  }
}
```

### 2. Create a Test Spec

```typescript
import { test, expect } from '@playwright/test';
import { MyNewPage } from '../pages/my-new-page.page';

test.describe('My New Page', () => {
  let myPage: MyNewPage;

  test.use({ storageState: '.auth/user.json' });

  test.beforeEach(async ({ page }) => {
    myPage = new MyNewPage(page);
    await myPage.goto();
  });

  test('should do something', async () => {
    await myPage.clickButton();
    await expect(myPage.someButton).toBeDisabled();
  });
});
```

## 🔧 Configuration

Edit `playwright.config.ts` to customize:
- Base URL
- Timeouts
- Retries
- Browsers and devices
- Screenshot/video settings
- Reporters

## 📚 Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Page Object Model Pattern](https://playwright.dev/docs/pom)
- [Playwright Test Fixtures](https://playwright.dev/docs/test-fixtures)

## 🤝 Contributing

When adding new features:
1. Create corresponding Page Object Models
2. Write comprehensive test specs
3. Include visual regression tests for key pages
4. Update this README with new test coverage

## 📄 License

This test suite is part of RecipeVault and shares its license.
