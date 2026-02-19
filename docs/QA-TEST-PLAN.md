# RecipeVault QA Test Plan

**App URL:** https://myrecipevault.io  
**Version:** 1.0.0  
**Last Updated:** 2026-02-18

---

## Prerequisites

- [ ] Modern browser (Chrome, Firefox, Safari, or Edge)
- [ ] Test account credentials (or ability to sign up via Supabase)
- [ ] Sample recipe images for upload testing
- [ ] Sample recipe URLs for import testing

---

## 1. Authentication

### 1.1 Login Page
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 1.1.1 | Access login page | Navigate to https://myrecipevault.io | Redirected to login page | |
| 1.1.2 | Login with valid credentials | Enter valid email/password, click Login | Redirected to recipes list | |
| 1.1.3 | Login with invalid credentials | Enter invalid email/password, click Login | Error message displayed | |
| 1.1.4 | Login with empty fields | Click Login without entering credentials | Validation error shown | |
| 1.1.5 | Social login (if enabled) | Click Google/GitHub login button | OAuth flow completes, redirected to app | |

### 1.2 Session Management
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 1.2.1 | Session persistence | Login, close browser, reopen app | Still logged in | |
| 1.2.2 | Logout | Click logout button/link | Redirected to login, session cleared | |
| 1.2.3 | Protected routes | Try accessing /recipes without login | Redirected to login page | |

---

## 2. Recipes

### 2.1 Recipe List
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.1.1 | View recipe list | Login and navigate to /recipes | List of user's recipes displayed | |
| 2.1.2 | Empty state | New account with no recipes | Helpful empty state message shown | |
| 2.1.3 | Pagination | Add 15+ recipes, navigate pages | Pagination works correctly | |
| 2.1.4 | Search by title | Enter search term in search box | Results filtered by title | |
| 2.1.5 | Filter by tag | Select a tag filter | Only recipes with that tag shown | |
| 2.1.6 | Filter by favorites | Toggle favorites filter | Only favorite recipes shown | |
| 2.1.7 | Sort recipes | Change sort order (title, date, rating) | List reorders correctly | |

### 2.2 Create Recipe (Manual)
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.2.1 | Navigate to create | Click "New Recipe" or "+" button | Recipe form displayed | |
| 2.2.2 | Required fields | Submit with empty title | Validation error on title | |
| 2.2.3 | Add basic info | Fill title, description, yield, times | Fields accept input correctly | |
| 2.2.4 | Add ingredients | Add multiple ingredients with qty/unit/item | Ingredients list populated | |
| 2.2.5 | Reorder ingredients | Drag ingredients to reorder | Order updates correctly | |
| 2.2.6 | Remove ingredient | Click delete on an ingredient | Ingredient removed from list | |
| 2.2.7 | Add instructions | Add multiple instruction steps | Instructions list populated | |
| 2.2.8 | Reorder instructions | Drag instructions to reorder | Step numbers update | |
| 2.2.9 | Add tags | Add/create tags for recipe | Tags associated with recipe | |
| 2.2.10 | Save recipe | Click Save/Create | Recipe saved, redirected to detail view | |

### 2.3 Import Recipe (AI Parser)
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.3.1 | Import from URL | Paste recipe URL, click Import | Recipe fields auto-populated | |
| 2.3.2 | Import from image | Upload recipe image/photo | AI extracts recipe data | |
| 2.3.3 | Import from text | Paste recipe text, click Parse | Recipe fields populated | |
| 2.3.4 | Edit parsed result | Modify auto-parsed fields | Changes saved correctly | |
| 2.3.5 | Invalid URL | Enter non-recipe URL | Appropriate error message | |
| 2.3.6 | Unsupported image | Upload non-recipe image | Error or low confidence warning | |

### 2.4 Recipe Detail View
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.4.1 | View recipe | Click recipe in list | Full recipe details displayed | |
| 2.4.2 | Ingredients display | View ingredients section | Quantities, units, items shown correctly | |
| 2.4.3 | Instructions display | View instructions section | Steps numbered and formatted | |
| 2.4.4 | Recipe image | Recipe with image | Image displayed prominently | |
| 2.4.5 | Time display | Recipe with prep/cook times | Times formatted nicely (e.g., "30 min") | |
| 2.4.6 | Source link | Recipe with source URL | Clickable link to original | |
| 2.4.7 | Tags display | Recipe with tags | Tags shown and clickable | |

### 2.5 Edit Recipe
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.5.1 | Navigate to edit | Click Edit on recipe detail | Form pre-populated with recipe data | |
| 2.5.2 | Modify fields | Change title and ingredients | Fields update correctly | |
| 2.5.3 | Save changes | Click Save | Changes persisted, back to detail view | |
| 2.5.4 | Cancel edit | Click Cancel | No changes saved, back to detail view | |

### 2.6 Delete Recipe
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.6.1 | Delete recipe | Click Delete, confirm | Recipe removed from list | |
| 2.6.2 | Cancel delete | Click Delete, cancel confirmation | Recipe not deleted | |

### 2.7 Recipe Actions
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 2.7.1 | Toggle favorite | Click favorite/heart icon | Favorite status toggles, persists | |
| 2.7.2 | Set rating | Click star rating (1-5) | Rating saved and displayed | |
| 2.7.3 | Share recipe | Click Share, copy link | Share link generated | |
| 2.7.4 | View shared recipe | Open share link (logged out) | Recipe viewable without login | |
| 2.7.5 | Print recipe | Click Print | Print-friendly view opens | |

---

## 3. Meal Plans

### 3.1 Meal Plan List
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 3.1.1 | View meal plans | Navigate to /meal-plans | List of meal plans displayed | |
| 3.1.2 | Empty state | No meal plans exist | Helpful empty state shown | |
| 3.1.3 | Create new plan | Click "New Meal Plan" | Meal plan form displayed | |

### 3.2 Create Meal Plan
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 3.2.1 | Set plan name | Enter meal plan name | Name accepted | |
| 3.2.2 | Set date range | Select start and end dates | Dates validated (end >= start) | |
| 3.2.3 | Add recipe to day | Drag/add recipe to a day slot | Recipe appears in meal plan | |
| 3.2.4 | Add multiple meals | Add breakfast, lunch, dinner | Multiple meals per day supported | |
| 3.2.5 | Remove recipe from plan | Click remove on planned recipe | Recipe removed from day | |
| 3.2.6 | Save meal plan | Click Save | Plan saved, redirected to detail | |

### 3.3 Meal Plan Detail
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 3.3.1 | View meal plan | Click meal plan in list | Calendar/list view of planned meals | |
| 3.3.2 | Navigate to recipe | Click recipe in meal plan | Opens recipe detail | |
| 3.3.3 | Edit meal plan | Click Edit | Edit form with existing data | |

### 3.4 Grocery List
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 3.4.1 | Generate grocery list | Click "Grocery List" on meal plan | Aggregated ingredients list shown | |
| 3.4.2 | Quantities combined | Same ingredient in multiple recipes | Quantities summed correctly | |
| 3.4.3 | Check off items | Click checkbox on ingredient | Item marked as checked | |
| 3.4.4 | Uncheck items | Click checked item | Item unchecked | |
| 3.4.5 | Clear checked | Click "Clear checked" | Checked items removed from view | |
| 3.4.6 | Print grocery list | Click Print | Print-friendly list | |

---

## 4. Tags

### 4.1 Tag Management
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 4.1.1 | Create tag | Add new tag while editing recipe | Tag created and applied | |
| 4.1.2 | Apply existing tag | Select existing tag on recipe | Tag applied to recipe | |
| 4.1.3 | Remove tag from recipe | Click X on tag | Tag removed from recipe | |
| 4.1.4 | Filter by tag | Click tag to filter recipes | Shows all recipes with that tag | |

---

## 5. Error Handling & Edge Cases

### 5.1 Network Errors
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 5.1.1 | Offline mode | Disable network, try to save | Graceful error message | |
| 5.1.2 | Slow network | Throttle to slow 3G, perform actions | Loading indicators shown | |
| 5.1.3 | API timeout | Simulate API timeout | Timeout error displayed | |

### 5.2 Data Validation
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 5.2.1 | XSS prevention | Enter `<script>alert('xss')</script>` in title | Script not executed, text escaped | |
| 5.2.2 | Long text | Enter 10,000 character description | Handled gracefully (truncate or allow) | |
| 5.2.3 | Special characters | Use emoji 🍕 and unicode in fields | Characters saved and displayed correctly | |
| 5.2.4 | Negative numbers | Enter -5 for yield | Validation error or converted to positive | |

### 5.3 Concurrent Access
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 5.3.1 | Edit conflict | Edit same recipe in two tabs, save both | Last save wins or conflict warning | |

---

## 6. Performance

| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 6.1 | Initial load time | Clear cache, load app | < 3 seconds to interactive | |
| 6.2 | Recipe list (100 items) | Account with 100 recipes | List loads in < 2 seconds | |
| 6.3 | Image upload | Upload 5MB image | Completes in < 10 seconds | |
| 6.4 | AI parsing | Parse recipe from image | Completes in < 30 seconds | |

---

## 7. Mobile/Responsive

| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 7.1 | Mobile layout | View on 375px width | Layout adapts, no horizontal scroll | |
| 7.2 | Touch interactions | Use touch to navigate | All interactions work | |
| 7.3 | Mobile recipe form | Create recipe on mobile | Form usable on small screen | |
| 7.4 | Tablet layout | View on iPad-sized screen | Layout uses available space well | |

---

## 8. Monitoring & Feedback

### 8.1 Sentry Integration
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 8.1.1 | Error capture | Trigger JS error in console | Error appears in Sentry dashboard | |
| 8.1.2 | Feedback button | Click bug report FAB | Sentry feedback form opens | |
| 8.1.3 | Submit feedback | Fill and submit feedback form | Feedback recorded in Sentry | |

### 8.2 Version Endpoint
| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 8.2.1 | Version API | GET /api/v1/version | Returns version info JSON | |

---

## 9. API Health

| # | Test Case | Steps | Expected Result | Pass/Fail |
|---|-----------|-------|-----------------|-----------|
| 9.1 | Health endpoint | GET /api/health | Returns healthy status | |
| 9.2 | Database connectivity | Check health includes DB check | Database status shown | |

---

## Test Summary

| Section | Total | Passed | Failed | Blocked |
|---------|-------|--------|--------|---------|
| 1. Authentication | 8 | | | |
| 2. Recipes | 31 | | | |
| 3. Meal Plans | 14 | | | |
| 4. Tags | 4 | | | |
| 5. Error Handling | 7 | | | |
| 6. Performance | 4 | | | |
| 7. Mobile/Responsive | 4 | | | |
| 8. Monitoring | 3 | | | |
| 9. API Health | 2 | | | |
| **TOTAL** | **77** | | | |

---

## Notes & Issues Found

_Document any bugs or issues discovered during testing:_

| Issue # | Description | Severity | Status |
|---------|-------------|----------|--------|
| | | | |

---

## Sign-off

- **Tester:** _______________
- **Date:** _______________
- **Build/Version:** 1.0.0
- **Environment:** Production (myrecipevault.io)
