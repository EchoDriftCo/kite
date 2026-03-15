# Onboarding

## Overview

Guide new users through their first experience with RecipeVault via a feature-focused, mobile-first onboarding flow. The tour showcases RecipeVault's killer features — recipe scaling, cooking mode with timers, ingredient substitutions, recipe forking, and multiple import methods. Each step saves independently, and the flow is skippable at any point.

**Design Philosophy:** First impressions matter. Users should think "oh shit, this app does WAY more than I expected" within the first 60 seconds.

---

## Use Cases

1. **Brand new user** — Signs up for the first time, sees an empty recipe list, needs to understand what makes RecipeVault special
2. **Returning user who skipped** — Wants to re-run the onboarding tour from settings
3. **User with dietary needs** — Prompted to set up dietary profile with clear value proposition (conflict warnings)
4. **User migrating from another app** — Prompted to import existing recipes (Paprika or URL)
5. **Mobile-first user** — Full-screen experience on mobile with swipe gestures and large touch targets

---

## Data Model

### UserAccount Changes

```csharp
public class UserAccount
{
    // Existing fields...
    public int UserAccountId { get; set; }
    public Guid UserAccountResourceId { get; set; }
    public Guid SubjectId { get; set; }
    public AccountTier AccountTier { get; set; }
    public DateTime? TierChangedDate { get; set; }

    // Onboarding fields
    public bool HasCompletedOnboarding { get; set; }
    public DateTime? OnboardingCompletedDate { get; set; }
    
    // Track partial progress
    public string OnboardingProgressJson { get; set; }  // {"dietaryProfileSet": true, "samplesAdded": false}

    public void CompleteOnboarding()
    {
        HasCompletedOnboarding = true;
        OnboardingCompletedDate = DateTime.UtcNow;
    }

    public void ResetOnboarding()
    {
        HasCompletedOnboarding = false;
        OnboardingCompletedDate = null;
        OnboardingProgressJson = null;
    }
}
```

### System Account (Deterministic UUID)

**System Account ID:** `d290f1ee-6c54-5f96-8b2f-9f14e72c8c39`

This UUID is generated using **UUID v5** (name-based, SHA-1):
- Namespace: DNS namespace (`6ba7b810-9dad-11d1-80b4-00c04fd430c8`)
- Name: `recipevault.io/system`

```csharp
// Generation (documentation only — value is fixed)
var dnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
var systemAccountId = GuidUtility.Create(dnsNamespace, "recipevault.io/system", version: 5);
// Result: d290f1ee-6c54-5f96-8b2f-9f14e72c8c39
```

**Why this matters:** Using `Guid.Empty` (`00000000-0000-0000-0000-000000000000`) is a common attack vector. A deterministic UUID is reproducible across environments while being collision-resistant and not exploitable.

### Recipe Changes (Sample Tracking)

```csharp
public class Recipe
{
    // Existing fields...
    
    public bool IsSampleRecipe { get; private set; }
    
    public void MarkAsSample()
    {
        IsSampleRecipe = true;
    }
}
```

### Sample Recipe Design

Each sample recipe demonstrates a specific RecipeVault feature:

| Recipe | Showcases | Details |
|--------|-----------|---------|
| **Thai Basil Chicken (Pad Kra Pao)** | **Ingredient Substitutions** | Pre-populated with 5+ substitution options (Thai basil → regular basil, fish sauce → soy sauce, etc.) |
| **Shakshuka with Feta** | **Cooking Mode & Timers** | 8 detailed step-by-step instructions, 3 embedded timers (simmer 5 min, poach eggs 6 min, rest 2 min) |
| **Korean Bibimbap Bowl** | **Nutritional Data** | Complete nutrition breakdown: 520 cal/serving, macros, vitamins, minerals |
| **Spicy Bibimbap (Fork)** | **Recipe Forking** | A fork of Korean Bibimbap with gochujang sauce upgrade, demonstrates fork relationship UI |
| **One-Pan Lemon Herb Salmon** | **Recipe Scaling** | Default 4 servings, easy to scale to 2 or 6, demonstrates quantity recalculation |
| **Moroccan Chickpea Stew** | **Tags & Collections** | Tagged with 7+ tags (Vegan, Moroccan, Stew, High-Fiber, One-Pot, Gluten-Free), in 2 collections (Vegan Favorites, Quick Weeknight Dinners) |

**Why these recipes?**
- They're interesting (not boring "chicken and rice")
- They represent diverse cuisines
- They make people want to cook
- Each demonstrates a unique RecipeVault capability

---

## API Design

### Get Onboarding Status

```
GET /api/v1/user/onboarding-status
```

Response:

```json
{
    "hasCompletedOnboarding": false,
    "recipeCount": 0,
    "hasDietaryProfile": false,
    "hasImportedRecipes": false,
    "progress": {
        "dietaryProfileSet": false,
        "samplesAdded": false,
        "tourCompleted": false
    }
}
```

### Complete Onboarding

```
POST /api/v1/user/complete-onboarding
```

Response: `204 No Content`

### Reset Onboarding

```
POST /api/v1/user/reset-onboarding
```

Response: `204 No Content`

### Update Onboarding Progress

```
PATCH /api/v1/user/onboarding-progress
```

Body:

```json
{
    "dietaryProfileSet": true
}
```

Response: `204 No Content`

**Why:** Each step saves independently. If user closes at step 2, step 1's choices are preserved.

### Add Sample Recipes

```
POST /api/v1/user/add-sample-recipes
```

Forks system sample recipes (owned by system account `d290f1ee-6c54-5f96-8b2f-9f14e72c8c39`) into the user's library. All forked recipes are marked with `IsSampleRecipe = true`.

Response:

```json
{
    "recipesAdded": 6,
    "recipes": [
        {
            "recipeResourceId": "...",
            "title": "Thai Basil Chicken (Pad Kra Pao)",
            "showcases": "ingredient-substitutions"
        }
        // ... 5 more
    ]
}
```

### Remove Sample Recipes

```
DELETE /api/v1/user/sample-recipes
```

Deletes all recipes where `IsSampleRecipe = true` for the current user.

Response:

```json
{
    "recipesRemoved": 6
}
```

---

## UX Design

### Mobile-First Principles

- **< 768px:** Full-screen sheet, swipe gestures, 48px minimum touch targets
- **≥ 768px:** Centered dialog (600px width)
- **Tooltips on mobile:** Always center-bottom card (never attached to element)
- **Tooltips on desktop:** Collision-detected positioning with viewport boundary checks

### Step 1: Welcome Screen

**Mobile (<768px):**

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│                                                     │
│              Welcome to RecipeVault                 │
│                                                     │
│       Your recipes. Smarter than you thought.       │
│                                                     │
│    ┌─────────────────────────────────────────┐     │
│    │                                         │     │
│    │     [ Recipe cards illustration ]      │     │
│    │                                         │     │
│    └─────────────────────────────────────────┘     │
│                                                     │
│                                                     │
│    ┌─────────────────────────────────────────┐     │
│    │                                         │     │
│    │          Get Started (1 of 4)           │     │  ← 48px tall
│    │                                         │     │
│    └─────────────────────────────────────────┘     │
│                                                     │
│                  Skip setup ›                       │  ← 48px target
│                                                     │
│                                                     │
│              ●○○○                                   │  ← Progress dots
│                                                     │
└─────────────────────────────────────────────────────┘
       ← Swipe to advance →
```

**Desktop (≥768px):**

```
┌─────────────────────────────────────────────────────┐
│   Welcome to RecipeVault                     Step 1/4│
├─────────────────────────────────────────────────────┤
│                                                     │
│       Your recipes. Smarter than you thought.       │
│                                                     │
│  ┌───────────────────────────────────────────────┐  │
│  │                                               │  │
│  │     [ Illustration: Recipe cards with         │  │
│  │       scaling controls, timer badges,         │  │
│  │       fork indicators ]                       │  │
│  │                                               │  │
│  └───────────────────────────────────────────────┘  │
│                                                     │
│                  [Get Started]                      │
│                                                     │
│                  Skip setup ›                       │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Step 2: Dietary Preferences

**Why this matters:** Don't just say "helps us flag recipes." Show the user EXACTLY what they get.

**Mobile & Desktop:**

```
┌─────────────────────────────────────────────────────┐
│   Set Your Dietary Preferences            Step 2/4  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  We'll watch your back when browsing recipes.       │
│                                                     │
│  ⚠️ ALLERGIES (we'll show warnings)                 │
│  ┌────────────────────────────────────────────────┐ │
│  │ ☐ Peanuts    ☐ Tree Nuts    ☐ Shellfish       │ │
│  │ ☐ Fish       ☐ Eggs         ☐ Dairy           │ │
│  │ ☐ Wheat      ☐ Soy          ☐ Sesame          │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  🌿 DIETARY CHOICES (we'll filter recipes)          │
│  ┌────────────────────────────────────────────────┐ │
│  │ ☐ Vegetarian  ☐ Vegan    ☐ Keto               │ │
│  │ ☐ Kosher      ☐ Halal    ☐ Paleo              │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  🚫 AVOID SPECIFIC INGREDIENTS                      │
│  ┌────────────────────────────────────────────────┐ │
│  │ [Add ingredient to avoid...]                   │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  ──────────────────────────────────────────────────│
│                                                     │
│  💡 HERE'S WHAT YOU'LL SEE:                         │
│                                                     │
│  ┌────────────────────────────────────────────────┐ │
│  │  ⚠️ DIETARY CONFLICT                           │ │
│  │  This recipe contains dairy (your profile:     │ │
│  │  Dairy-Free)                                   │ │
│  │                                                │ │
│  │  • Heavy cream (dairy allergen)                │ │
│  │  • Parmesan cheese (dairy allergen)            │ │
│  │                                                │ │
│  │  [Find Substitutions]  [Cook Anyway]           │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  [Back]  [Skip]                  [Save & Continue] │
│                                                     │
│                   ●●○○                              │
└─────────────────────────────────────────────────────┘
```

**Key change:** The mock conflict warning shows users EXACTLY what value they're getting. Not vague promises — concrete UI they'll see in action.

**Immediate save:** When "Save & Continue" is clicked, dietary profile is saved immediately via `PATCH /api/v1/user/onboarding-progress`. If user closes the dialog, their preferences are preserved.

### Step 3: Add Recipes

**Mobile & Desktop:**

```
┌─────────────────────────────────────────────────────┐
│   Let's Add Some Recipes                  Step 3/4  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Choose how you'd like to get started:              │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  📦 Try Sample Recipes                       │   │  ← 48px min
│  │  6 delicious recipes showcasing RecipeVault  │   │
│  │  features (cooking mode, scaling, & more)    │   │
│  │                                              │   │
│  │  [Add 6 Samples]                             │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  🔗 Import from URL                          │   │
│  │  Paste a recipe link from any website        │   │
│  │                                              │   │
│  │  [Import from URL]                           │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  📱 Import from Paprika                      │   │
│  │  Migrate your existing Paprika collection    │   │
│  │                                              │   │
│  │  [Upload .paprikarecipes]                    │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ℹ️ Photo import coming soon                       │
│                                                     │
│  [Back]  [Skip]                       [Continue]    │
│                                                     │
│                   ●●●○                              │
└─────────────────────────────────────────────────────┘
```

**Key changes:**
- **Sample recipes button is prominent** — most users will click this
- **Only stable import methods** — URL and Paprika. No experimental image import in onboarding.
- **Photo import teased as "coming soon"** — sets expectations, doesn't over-promise
- **Each action saves immediately** — clicking "Add 6 Samples" calls the API and marks progress

### Step 4: Feature Tour

After step 3, the dialog closes and an **interactive tooltip tour** showcases killer features using the sample recipes.

**Tour Stops (5 total):**

1. **Recipe Scaling** (uses "One-Pan Lemon Herb Salmon" sample)
2. **Cooking Mode** (uses "Shakshuka with Feta" sample)
3. **Ingredient Substitutions** (uses "Thai Basil Chicken" sample)
4. **Recipe Forking** (uses "Spicy Bibimbap" fork sample)
5. **Import Options** (points to Add Recipe button)

#### Tour Stop 1: Recipe Scaling

**Desktop positioning:**

```
┌─────────────────────────────────────────────────────┐
│  [My Recipes] [Discover] [+]                        │
│                                                     │
│  ┌────────────────────────────────────────────┐    │
│  │  One-Pan Lemon Herb Salmon                 │    │
│  │  ★★★★★  25 min  🏷️ Sample                  │    │
│  │                                            │    │
│  │  Servings: [2] [4] [6]  ← 📍              │    │
│  │             ▲▲▲                            │    │
│  └────────────────┼───────────────────────────┘    │
│                   │                                │
│     ┌─────────────┴──────────────────────────┐     │
│     │ 🎯 Recipe Scaling                      │     │
│     │                                        │     │
│     │ Change serving sizes and watch         │     │
│     │ ingredient quantities adjust           │     │
│     │ automatically. Perfect for cooking     │     │
│     │ for 2 or feeding a crowd.              │     │
│     │                                        │     │
│     │ Try it: Click [2] or [6]               │     │
│     │                                        │     │
│     │            [Next (1/5)]  [Skip Tour]   │     │
│     └────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────┘
```

**Mobile positioning:**

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│  ┌────────────────────────────────────────────┐    │
│  │  One-Pan Lemon Herb Salmon                 │    │
│  │  ★★★★★  25 min  🏷️ Sample                  │    │
│  │                                            │    │
│  │  Servings: [2] [4] [6]                     │    │
│  │                                            │    │
│  └────────────────────────────────────────────┘    │
│                                                     │
│                                                     │
│                                                     │
│                                                     │
│  ┌──────────────────────────────────────────────┐  │
│  │ 🎯 Recipe Scaling                            │  │  ← Center-bottom
│  │                                              │  │    card
│  │ Change serving sizes and watch ingredient   │  │
│  │ quantities adjust automatically.             │  │
│  │                                              │  │
│  │ Try it: Tap [2] or [6] above                │  │
│  │                                              │  │
│  │           [Next (1/5)]  [Skip Tour]          │  │  ← 48px tall
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

#### Tour Stop 2: Cooking Mode

Opens "Shakshuka with Feta" and highlights the cooking mode button:

```
┌─────────────────────────────────────────────────────┐
│  Shakshuka with Feta                 [Edit] [Share] │
│                                                     │
│  [🍳 Start Cooking] ← 📍                            │
│         ▲                                           │
│         │                                           │
│  ┌──────┴──────────────────────────────────────┐   │
│  │ 🎯 Cooking Mode                              │   │
│  │                                              │   │
│  │ Step-by-step view with built-in timers.     │   │
│  │ Keep your screen awake while you cook.      │   │
│  │                                              │   │
│  │ This recipe has 3 timers ready to go:       │   │
│  │ • Simmer: 5 min                              │   │
│  │ • Poach eggs: 6 min                          │   │
│  │ • Rest: 2 min                                │   │
│  │                                              │   │
│  │ Try it: Click [Start Cooking]                │   │
│  │                                              │   │
│  │           [Next (2/5)]  [Skip Tour]          │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

#### Tour Stop 3: Ingredient Substitutions

Opens "Thai Basil Chicken" and highlights an ingredient with substitutions:

```
┌─────────────────────────────────────────────────────┐
│  Thai Basil Chicken (Pad Kra Pao)                   │
│                                                     │
│  INGREDIENTS                                        │
│                                                     │
│  ○ 1 lb ground chicken                             │
│  ○ 3 tbsp fish sauce [🔄 Substitutes] ← 📍        │
│                        ▲                           │
│         ┌──────────────┴───────────────────────┐   │
│         │ 🎯 Ingredient Substitutions           │   │
│         │                                       │   │
│         │ Out of an ingredient? We've got       │   │
│         │ alternatives ready.                   │   │
│         │                                       │   │
│         │ This recipe has 5 substitutions       │   │
│         │ pre-configured:                       │   │
│         │ • Fish sauce → Soy sauce + lime       │   │
│         │ • Thai basil → Regular basil          │   │
│         │ • Bird's eye chili → Red pepper flakes│   │
│         │                                       │   │
│         │ Try it: Click [Substitutes]           │   │
│         │                                       │   │
│         │      [Next (3/5)]  [Skip Tour]        │   │
│         └───────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

#### Tour Stop 4: Recipe Forking

Shows "Spicy Bibimbap (Fork)" recipe card:

```
┌─────────────────────────────────────────────────────┐
│  ┌────────────────────────────────────────────┐    │
│  │  Spicy Bibimbap                            │    │
│  │  ↳ Forked from "Korean Bibimbap Bowl"      │ ← 📍
│  │  ★★★★★  35 min  🏷️ Sample                  │    │
│  └────────────────────────────────────────────┘    │
│                   ▲                                │
│       ┌───────────┴────────────────────────────┐   │
│       │ 🎯 Recipe Forking                      │   │
│       │                                        │   │
│       │ Found a great recipe but want to tweak│   │
│       │ it? Fork it to create your own version│   │
│       │ while keeping the original.            │   │
│       │                                        │   │
│       │ This "Spicy Bibimbap" is a fork with   │   │
│       │ upgraded gochujang sauce. Your changes │   │
│       │ don't affect the original.             │   │
│       │                                        │   │
│       │ Try it: Open any recipe and click      │   │
│       │ [Fork Recipe]                          │   │
│       │                                        │   │
│       │       [Next (4/5)]  [Skip Tour]        │   │
│       └────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

#### Tour Stop 5: Import Options

Points to the Add Recipe button:

```
┌─────────────────────────────────────────────────────┐
│  [My Recipes] [Discover] [+] ← 📍                   │
│                              ▲                      │
│          ┌───────────────────┴─────────────────┐   │
│          │ 🎯 Multiple Import Options           │   │
│          │                                      │   │
│          │ Add recipes from anywhere:           │   │
│          │                                      │   │
│          │ • 🔗 Paste any recipe URL            │   │
│          │ • 📱 Import from Paprika             │   │
│          │ • ✍️ Create manually                 │   │
│          │ • 📸 Photo import (coming soon)      │   │
│          │                                      │   │
│          │ Try it: Click [+] to add a recipe    │   │
│          │                                      │   │
│          │     [Finish Tour]  [Skip]            │   │
│          └──────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

**After tour completes:** Mark `tourCompleted: true` via API, then mark `hasCompletedOnboarding: true`.

### Tooltip Positioning Logic

```typescript
interface TooltipPosition {
  top: number;
  left: number;
  placement: 'above' | 'below' | 'left' | 'right' | 'center-bottom';
}

function calculateTooltipPosition(
  targetElement: HTMLElement,
  tooltipWidth: number,
  tooltipHeight: number,
  isMobile: boolean
): TooltipPosition {
  
  // Mobile: always center-bottom card
  if (isMobile) {
    return {
      top: window.innerHeight - tooltipHeight - 20,
      left: (window.innerWidth - tooltipWidth) / 2,
      placement: 'center-bottom'
    };
  }
  
  // Desktop: collision-detected positioning
  const rect = targetElement.getBoundingClientRect();
  const viewportWidth = window.innerWidth;
  const viewportHeight = window.innerHeight;
  const margin = 12; // pixels from element
  
  // Preferred: below
  let top = rect.bottom + margin;
  let left = rect.left;
  let placement: 'above' | 'below' | 'left' | 'right' = 'below';
  
  // Check bottom overflow
  if (top + tooltipHeight > viewportHeight) {
    // Try above
    top = rect.top - tooltipHeight - margin;
    placement = 'above';
    
    // Still overflows? Try right
    if (top < 0) {
      top = rect.top;
      left = rect.right + margin;
      placement = 'right';
      
      // Still overflows? Try left
      if (left + tooltipWidth > viewportWidth) {
        left = rect.left - tooltipWidth - margin;
        placement = 'left';
      }
    }
  }
  
  // Check horizontal overflow and adjust
  if (left + tooltipWidth > viewportWidth) {
    left = viewportWidth - tooltipWidth - 20;
  }
  if (left < 0) {
    left = 20;
  }
  
  // Check vertical overflow (final fallback)
  if (top + tooltipHeight > viewportHeight) {
    top = viewportHeight - tooltipHeight - 20;
  }
  if (top < 0) {
    top = 20;
  }
  
  return { top, left, placement };
}
```

### Sample Recipe Badges

Recipes added during onboarding are marked with `IsSampleRecipe = true`.

**In recipe list:**

```
┌─────────────────────────────────────────────────────┐
│  ┌────────────────────────────────────────────┐    │
│  │  Thai Basil Chicken (Pad Kra Pao)          │    │
│  │  ★★★★★  30 min  🏷️ Sample                  │ ← Badge
│  └────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

**In settings:**

```
┌─────────────────────────────────────────────────────┐
│  Settings                                           │
│                                                     │
│  Getting Started                                    │
│  ────────────────                                   │
│  Re-run onboarding tour          [Start Tour]       │
│  Remove all sample recipes       [Remove Samples]   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Remove confirmation:**

```
┌─────────────────────────────────────────────────────┐
│  Remove Sample Recipes?                             │
├─────────────────────────────────────────────────────┤
│                                                     │
│  This will delete 6 sample recipes from your        │
│  library. Any edits you've made will be lost.       │
│                                                     │
│  [Cancel]                            [Remove All]   │
└─────────────────────────────────────────────────────┘
```

---

## Implementation

### Database Migration

```sql
-- Add onboarding fields
ALTER TABLE "UserAccount" 
  ADD COLUMN "HasCompletedOnboarding" BOOLEAN NOT NULL DEFAULT FALSE,
  ADD COLUMN "OnboardingCompletedDate" TIMESTAMP WITH TIME ZONE NULL,
  ADD COLUMN "OnboardingProgressJson" TEXT NULL;

-- Add sample recipe tracking
ALTER TABLE "Recipe" 
  ADD COLUMN "IsSampleRecipe" BOOLEAN NOT NULL DEFAULT FALSE;

-- Create index for efficient sample recipe queries
CREATE INDEX "IX_Recipe_IsSampleRecipe" ON "Recipe" ("IsSampleRecipe") 
  WHERE "IsSampleRecipe" = TRUE;
```

### System Account Seed

```sql
-- Insert system subject (deterministic UUID)
INSERT INTO "Subject" (
  "SubjectId", 
  "Name", 
  "GivenName", 
  "FamilyName", 
  "UserPrincipalName"
)
VALUES (
  'd290f1ee-6c54-5f96-8b2f-9f14e72c8c39',
  'RecipeVault System',
  'System',
  'RecipeVault',
  'system@recipevault.io'
)
ON CONFLICT ("SubjectId") DO NOTHING;

-- Create UserAccount for system (if needed by auth)
INSERT INTO "UserAccount" (
  "UserAccountResourceId",
  "SubjectId",
  "AccountTier",
  "HasCompletedOnboarding"
)
VALUES (
  gen_random_uuid(),
  'd290f1ee-6c54-5f96-8b2f-9f14e72c8c39',
  0, -- Free tier
  TRUE
)
ON CONFLICT DO NOTHING;
```

### Sample Recipe Seed Data

**Implementation approach:** Use EF Core seed data in migration. Full recipe JSON for all 6 samples is defined in `SampleRecipeSeedData.cs`.

**Example (Thai Basil Chicken):**

```csharp
public static class SampleRecipeSeedData
{
    private static readonly Guid SystemSubjectId = 
        new Guid("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39");
    
    public static void Seed(ModelBuilder modelBuilder)
    {
        var recipes = new[]
        {
            CreateThaiBasilChicken(),
            CreateShakshuka(),
            CreateBibimbap(),
            CreateSpicyBibimbapFork(),
            CreateLemonSalmon(),
            CreateChickpeaStew()
        };
        
        modelBuilder.Entity<Recipe>().HasData(recipes);
        // Also seed ingredients, instructions, tags, etc.
    }
    
    private static Recipe CreateThaiBasilChicken()
    {
        return new Recipe
        {
            RecipeId = 1000001,
            RecipeResourceId = new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
            SubjectId = SystemSubjectId,
            Title = "Thai Basil Chicken (Pad Kra Pao)",
            Description = "Spicy, savory Thai stir-fry showcasing RecipeVault's ingredient substitution feature. Pre-loaded with 5 substitution options.",
            ServingCount = 4,
            PrepTimeMinutes = 15,
            CookTimeMinutes = 15,
            TotalTimeMinutes = 30,
            Visibility = RecipeVisibility.Public,
            IsSampleRecipe = true,
            CreatedDate = DateTime.UtcNow,
            CreatedSubjectId = SystemSubjectId,
            // Ingredients and instructions seeded separately
        };
    }
    
    // ... other sample recipes
}
```

**Substitutions seed (for Thai Basil Chicken):**

```csharp
// Seeded via RecipeIngredientSubstitution table
new RecipeIngredientSubstitution
{
    RecipeIngredientSubstitutionId = 100001,
    RecipeIngredientId = 1000001, // "3 tbsp fish sauce"
    OriginalIngredient = "fish sauce",
    SubstituteIngredient = "soy sauce + lime juice",
    SubstituteQuantity = "3 tbsp soy sauce + 1 tbsp lime juice",
    Notes = "Soy sauce provides saltiness, lime adds tang to mimic fish sauce",
    ConfidenceScore = 0.85m
},
new RecipeIngredientSubstitution
{
    RecipeIngredientSubstitutionId = 100002,
    RecipeIngredientId = 1000002, // "1 cup Thai basil"
    OriginalIngredient = "Thai basil",
    SubstituteIngredient = "regular basil + mint",
    SubstituteQuantity = "3/4 cup basil + 1/4 cup mint",
    Notes = "Regular basil lacks the licorice note; mint helps bridge the gap",
    ConfidenceScore = 0.75m
}
// ... 3 more substitutions
```

**Nutritional data seed (for Korean Bibimbap):**

```csharp
new RecipeNutrition
{
    RecipeNutritionId = 100001,
    RecipeId = 1000003, // Korean Bibimbap
    CaloriesPerServing = 520,
    ProteinPerServing = 28,
    CarbsPerServing = 62,
    FatPerServing = 18,
    FiberPerServing = 8,
    SugarPerServing = 12,
    SodiumPerServing = 680,
    IngredientsMatched = 12,
    IngredientsTotal = 12,
    CoveragePercent = 100,
    CalculatedDate = DateTime.UtcNow,
    IsStale = false
}
```

**Fork relationship (Spicy Bibimbap):**

```csharp
// Spicy Bibimbap is a fork of Korean Bibimbap
new Recipe
{
    RecipeId = 1000004,
    RecipeResourceId = new Guid("f0e1d2c3-b4a5-9687-7654-321098fedcba"),
    SubjectId = SystemSubjectId,
    Title = "Spicy Bibimbap",
    Description = "A fork of Korean Bibimbap with upgraded gochujang sauce for extra heat.",
    ForkedFromRecipeId = 1000003, // Points to Korean Bibimbap
    IsSampleRecipe = true,
    // ... other fields
}
```

### OnboardingService

```csharp
public class OnboardingService : IOnboardingService
{
    private readonly IUserAccountRepository userAccountRepository;
    private readonly IRecipeRepository recipeRepository;
    private readonly IRecipeService recipeService;
    private readonly ISubjectPrincipal subjectPrincipal;
    private readonly IDietaryProfileService dietaryProfileService;
    private readonly ILogger<OnboardingService> logger;

    private static readonly Guid SystemSubjectId = 
        new Guid("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39");

    public async Task<OnboardingStatusDto> GetOnboardingStatusAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var account = await userAccountRepository
            .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
        var recipeCount = await recipeRepository
            .GetCountByOwnerAsync(subjectId).ConfigureAwait(false);
        var hasProfile = await dietaryProfileService
            .HasProfileAsync(subjectId).ConfigureAwait(false);

        var progress = string.IsNullOrEmpty(account.OnboardingProgressJson)
            ? new OnboardingProgress()
            : JsonSerializer.Deserialize<OnboardingProgress>(account.OnboardingProgressJson);

        return new OnboardingStatusDto
        {
            HasCompletedOnboarding = account.HasCompletedOnboarding,
            RecipeCount = recipeCount,
            HasDietaryProfile = hasProfile,
            HasImportedRecipes = recipeCount > 0,
            Progress = progress
        };
    }

    public async Task UpdateOnboardingProgressAsync(OnboardingProgress progress)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var account = await userAccountRepository
            .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
        
        account.OnboardingProgressJson = JsonSerializer.Serialize(progress);
        await userAccountRepository.UpdateAsync(account).ConfigureAwait(false);
        
        logger.LogInformation("Updated onboarding progress for user {SubjectId}: {Progress}",
            subjectId, account.OnboardingProgressJson);
    }

    public async Task<AddSampleRecipesResultDto> AddSampleRecipesAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        
        // Get all system sample recipes
        var sampleRecipes = await recipeRepository
            .GetSampleRecipesAsync(SystemSubjectId).ConfigureAwait(false);

        // Check which samples the user already has (by fork reference)
        var existingForks = await recipeRepository
            .GetForkSourceIdsAsync(subjectId).ConfigureAwait(false);

        var results = new List<AddedRecipeDto>();
        foreach (var sample in sampleRecipes)
        {
            if (existingForks.Contains(sample.RecipeId))
                continue; // Already forked this sample

            var forked = await recipeService
                .ForkRecipeAsync(sample.RecipeResourceId).ConfigureAwait(false);
            
            // Mark as sample
            forked.MarkAsSample();
            await recipeRepository.UpdateAsync(forked).ConfigureAwait(false);
            
            results.Add(new AddedRecipeDto
            {
                RecipeResourceId = forked.RecipeResourceId,
                Title = forked.Title,
                Showcases = GetFeatureShowcase(sample.RecipeId)
            });
        }

        logger.LogInformation("Added {Count} sample recipes for user {SubjectId}",
            results.Count, subjectId);
        
        return new AddSampleRecipesResultDto
        {
            RecipesAdded = results.Count,
            Recipes = results
        };
    }

    public async Task RemoveSampleRecipesAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var sampleRecipes = await recipeRepository
            .GetSampleRecipesByOwnerAsync(subjectId).ConfigureAwait(false);
        
        foreach (var recipe in sampleRecipes)
        {
            await recipeService.DeleteRecipeAsync(recipe.RecipeResourceId)
                .ConfigureAwait(false);
        }
        
        logger.LogInformation("Removed {Count} sample recipes for user {SubjectId}",
            sampleRecipes.Count, subjectId);
    }

    public async Task CompleteOnboardingAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var account = await userAccountRepository
            .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
        account.CompleteOnboarding();
        await userAccountRepository.UpdateAsync(account).ConfigureAwait(false);
    }

    public async Task ResetOnboardingAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var account = await userAccountRepository
            .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
        account.ResetOnboarding();
        await userAccountRepository.UpdateAsync(account).ConfigureAwait(false);
    }

    private string GetFeatureShowcase(int recipeId)
    {
        // Map sample recipe IDs to features they showcase
        return recipeId switch
        {
            1000001 => "ingredient-substitutions",
            1000002 => "cooking-mode",
            1000003 => "nutrition",
            1000004 => "recipe-forking",
            1000005 => "recipe-scaling",
            1000006 => "tags-collections",
            _ => "general"
        };
    }
}
```

### Controller

```csharp
[ApiVersion("1")]
[Produces("application/json")]
[ApiController]
[Route("api/v{version:apiVersion}/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IOnboardingFacade facade;
    private readonly IMapper mapper;

    [HttpGet("onboarding-status")]
    [ProducesResponseType(typeof(OnboardingStatusModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnboardingStatusAsync()
    {
        var status = await facade.GetOnboardingStatusAsync().ConfigureAwait(false);
        return Ok(mapper.Map<OnboardingStatusModel>(status));
    }

    [HttpPatch("onboarding-progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateOnboardingProgressAsync(
        [FromBody] OnboardingProgressModel progress)
    {
        await facade.UpdateOnboardingProgressAsync(
            mapper.Map<OnboardingProgress>(progress)).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("complete-onboarding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompleteOnboardingAsync()
    {
        await facade.CompleteOnboardingAsync().ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("reset-onboarding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetOnboardingAsync()
    {
        await facade.ResetOnboardingAsync().ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("add-sample-recipes")]
    [ProducesResponseType(typeof(AddSampleRecipesResultModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddSampleRecipesAsync()
    {
        var result = await facade.AddSampleRecipesAsync().ConfigureAwait(false);
        return Ok(mapper.Map<AddSampleRecipesResultModel>(result));
    }

    [HttpDelete("sample-recipes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSampleRecipesAsync()
    {
        await facade.RemoveSampleRecipesAsync().ConfigureAwait(false);
        return NoContent();
    }
}
```

### Angular Models

```typescript
export interface OnboardingStatus {
  hasCompletedOnboarding: boolean;
  recipeCount: number;
  hasDietaryProfile: boolean;
  hasImportedRecipes: boolean;
  progress: OnboardingProgress;
}

export interface OnboardingProgress {
  dietaryProfileSet: boolean;
  samplesAdded: boolean;
  tourCompleted: boolean;
}

export interface AddSampleRecipesResult {
  recipesAdded: number;
  recipes: AddedRecipe[];
}

export interface AddedRecipe {
  recipeResourceId: string;
  title: string;
  showcases: string; // 'ingredient-substitutions' | 'cooking-mode' | etc.
}
```

### Angular Onboarding Service

```typescript
@Injectable({ providedIn: 'root' })
export class OnboardingService {
  private http = inject(HttpClient);
  private configService = inject(ConfigService);
  private apiUrl = `${this.configService.apiUrl}/user`;

  getStatus(): Observable<OnboardingStatus> {
    return this.http.get<OnboardingStatus>(`${this.apiUrl}/onboarding-status`);
  }

  updateProgress(progress: Partial<OnboardingProgress>): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/onboarding-progress`, progress);
  }

  completeOnboarding(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/complete-onboarding`, {});
  }

  resetOnboarding(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-onboarding`, {});
  }

  addSampleRecipes(): Observable<AddSampleRecipesResult> {
    return this.http.post<AddSampleRecipesResult>(
      `${this.apiUrl}/add-sample-recipes`, {}
    );
  }

  removeSampleRecipes(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/sample-recipes`);
  }
}
```

### OnboardingDialogComponent

```typescript
@Component({
  selector: 'app-onboarding-dialog',
  standalone: true,
  imports: [
    CommonModule, MatDialogModule, MatButtonModule, MatStepperModule,
    MatCheckboxModule, MatChipsModule, MatInputModule, MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './onboarding-dialog.component.html',
  styleUrl: './onboarding-dialog.component.scss'
})
export class OnboardingDialogComponent implements OnInit {
  currentStep = 0;
  totalSteps = 4;
  
  // Step 2: Dietary
  selectedRestrictions: string[] = [];
  avoidedIngredients: string[] = [];
  avoidedInput = '';
  
  // Step 3: Recipes
  samplesAdded = false;
  
  private dialogRef = inject(MatDialogRef<OnboardingDialogComponent>);
  private onboardingService = inject(OnboardingService);
  private dietaryProfileService = inject(DietaryProfileService);
  private snackBar = inject(MatSnackBar);
  private breakpointObserver = inject(BreakpointObserver);
  
  isMobile = false;
  
  ngOnInit(): void {
    // Detect mobile for full-screen mode
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe(result => {
        this.isMobile = result.matches;
        // Reconfigure dialog for mobile
        if (this.isMobile) {
          this.dialogRef.updateSize('100vw', '100vh');
        } else {
          this.dialogRef.updateSize('600px', 'auto');
        }
      });
  }

  get progressPercent(): number {
    return ((this.currentStep + 1) / this.totalSteps) * 100;
  }

  nextStep(): void {
    if (this.currentStep < this.totalSteps - 1) {
      this.currentStep++;
    } else {
      this.finish();
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  skip(): void {
    this.onboardingService.completeOnboarding().subscribe();
    this.dialogRef.close({ completed: true, startTour: false });
  }

  // Step 2: Dietary
  toggleRestriction(code: string): void {
    const idx = this.selectedRestrictions.indexOf(code);
    if (idx >= 0) {
      this.selectedRestrictions.splice(idx, 1);
    } else {
      this.selectedRestrictions.push(code);
    }
  }

  addAvoidedIngredient(): void {
    const trimmed = this.avoidedInput.trim();
    if (trimmed && !this.avoidedIngredients.includes(trimmed)) {
      this.avoidedIngredients.push(trimmed);
    }
    this.avoidedInput = '';
  }

  removeAvoidedIngredient(name: string): void {
    this.avoidedIngredients = this.avoidedIngredients.filter(i => i !== name);
  }

  saveDietaryPreferences(): void {
    if (this.selectedRestrictions.length === 0 && this.avoidedIngredients.length === 0) {
      // Nothing to save, just advance
      this.nextStep();
      return;
    }

    this.dietaryProfileService.createProfile({
      profileName: 'My Preferences',
      isDefault: true,
      restrictions: this.selectedRestrictions.map(code => ({
        restrictionCode: code,
        restrictionType: this.getRestrictionType(code),
        severity: 'Strict'
      })),
      avoidedIngredients: this.avoidedIngredients.map(name => ({
        ingredientName: name,
        reason: 'Set during onboarding'
      }))
    }).subscribe({
      next: () => {
        // Mark progress immediately
        this.onboardingService.updateProgress({ dietaryProfileSet: true })
          .subscribe();
        this.nextStep();
      }
    });
  }

  private getRestrictionType(code: string): string {
    const allergies = ['peanuts', 'tree-nuts', 'shellfish', 'fish', 'eggs', 'dairy', 'wheat', 'soy', 'sesame'];
    return allergies.includes(code) ? 'Allergy' : 'DietaryChoice';
  }

  // Step 3: Recipes
  addSampleRecipes(): void {
    if (this.samplesAdded) return;
    
    this.onboardingService.addSampleRecipes().subscribe({
      next: result => {
        this.samplesAdded = true;
        this.snackBar.open(
          `${result.recipesAdded} sample recipes added to your library.`,
          'OK',
          { duration: 3000 }
        );
        
        // Mark progress immediately
        this.onboardingService.updateProgress({ samplesAdded: true })
          .subscribe();
      }
    });
  }

  openImportDialog(): void {
    // TODO: Implement URL import dialog
    this.snackBar.open('Import dialog coming soon!', 'OK', { duration: 2000 });
  }

  openPaprikaDialog(): void {
    // TODO: Implement Paprika import dialog
    this.snackBar.open('Paprika import coming soon!', 'OK', { duration: 2000 });
  }

  private finish(): void {
    this.onboardingService.completeOnboarding().subscribe();
    this.dialogRef.close({ completed: true, startTour: true });
  }
}
```

### TourService

```typescript
export interface TourStep {
  recipeResourceId?: string; // Navigate to recipe before showing tooltip
  elementSelector: string;
  title: string;
  message: string;
  actionHint: string; // "Try it: Click [2] or [6]"
  showcases: string; // 'recipe-scaling' | 'cooking-mode' | etc.
}

@Injectable({ providedIn: 'root' })
export class TourService {
  private steps: TourStep[] = [
    {
      recipeResourceId: 'SALMON_RESOURCE_ID', // Injected from config
      elementSelector: '[data-tour="serving-selector"]',
      title: '🎯 Recipe Scaling',
      message: 'Change serving sizes and watch ingredient quantities adjust automatically. Perfect for cooking for 2 or feeding a crowd.',
      actionHint: 'Try it: Click [2] or [6]',
      showcases: 'recipe-scaling'
    },
    {
      recipeResourceId: 'SHAKSHUKA_RESOURCE_ID',
      elementSelector: '[data-tour="cooking-mode-button"]',
      title: '🎯 Cooking Mode',
      message: 'Step-by-step view with built-in timers. Keep your screen awake while you cook.\n\nThis recipe has 3 timers ready to go:\n• Simmer: 5 min\n• Poach eggs: 6 min\n• Rest: 2 min',
      actionHint: 'Try it: Click [Start Cooking]',
      showcases: 'cooking-mode'
    },
    {
      recipeResourceId: 'THAI_BASIL_RESOURCE_ID',
      elementSelector: '[data-tour="substitution-button"]',
      title: '🎯 Ingredient Substitutions',
      message: 'Out of an ingredient? We\'ve got alternatives ready.\n\nThis recipe has 5 substitutions pre-configured:\n• Fish sauce → Soy sauce + lime\n• Thai basil → Regular basil\n• Bird\'s eye chili → Red pepper flakes',
      actionHint: 'Try it: Click [Substitutes]',
      showcases: 'ingredient-substitutions'
    },
    {
      recipeResourceId: 'SPICY_BIBIMBAP_RESOURCE_ID',
      elementSelector: '[data-tour="fork-indicator"]',
      title: '🎯 Recipe Forking',
      message: 'Found a great recipe but want to tweak it? Fork it to create your own version while keeping the original.\n\nThis "Spicy Bibimbap" is a fork with upgraded gochujang sauce. Your changes don\'t affect the original.',
      actionHint: 'Try it: Open any recipe and click [Fork Recipe]',
      showcases: 'recipe-forking'
    },
    {
      elementSelector: '[data-tour="add-recipe-button"]',
      title: '🎯 Multiple Import Options',
      message: 'Add recipes from anywhere:\n\n• 🔗 Paste any recipe URL\n• 📱 Import from Paprika\n• ✍️ Create manually\n• 📸 Photo import (coming soon)',
      actionHint: 'Try it: Click [+] to add a recipe',
      showcases: 'import-options'
    }
  ];

  currentStepIndex = -1;
  active = false;
  
  private router = inject(Router);
  private breakpointObserver = inject(BreakpointObserver);
  
  isMobile = false;

  constructor() {
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe(result => {
        this.isMobile = result.matches;
      });
  }

  async start(): Promise<void> {
    this.currentStepIndex = 0;
    this.active = true;
    await this.navigateToStep();
  }

  async next(): Promise<void> {
    this.currentStepIndex++;
    if (this.currentStepIndex >= this.steps.length) {
      this.end();
    } else {
      await this.navigateToStep();
    }
  }

  end(): void {
    this.currentStepIndex = -1;
    this.active = false;
  }

  get currentStep(): TourStep | null {
    return this.active ? this.steps[this.currentStepIndex] ?? null : null;
  }

  get stepLabel(): string {
    return `${this.currentStepIndex + 1}/${this.steps.length}`;
  }

  private async navigateToStep(): Promise<void> {
    const step = this.currentStep;
    if (!step) return;
    
    // Navigate to recipe if specified
    if (step.recipeResourceId) {
      await this.router.navigate(['/recipes', step.recipeResourceId]);
      // Wait for DOM to settle
      await new Promise(resolve => setTimeout(resolve, 500));
    }
    
    // Skip if target element not found
    if (!document.querySelector(step.elementSelector)) {
      await this.next();
    }
  }
  
  calculateTooltipPosition(tooltipWidth: number, tooltipHeight: number): {
    top: number;
    left: number;
    placement: string;
  } {
    const step = this.currentStep;
    if (!step) return { top: 0, left: 0, placement: 'center-bottom' };
    
    // Mobile: always center-bottom card
    if (this.isMobile) {
      return {
        top: window.innerHeight - tooltipHeight - 20,
        left: (window.innerWidth - tooltipWidth) / 2,
        placement: 'center-bottom'
      };
    }
    
    // Desktop: collision-detected positioning
    const targetEl = document.querySelector(step.elementSelector) as HTMLElement;
    if (!targetEl) return { top: 0, left: 0, placement: 'center-bottom' };
    
    return this.calculateDesktopPosition(targetEl, tooltipWidth, tooltipHeight);
  }
  
  private calculateDesktopPosition(
    targetElement: HTMLElement,
    tooltipWidth: number,
    tooltipHeight: number
  ): { top: number; left: number; placement: string } {
    const rect = targetElement.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const margin = 12;
    
    // Preferred: below
    let top = rect.bottom + margin;
    let left = rect.left;
    let placement = 'below';
    
    // Check bottom overflow
    if (top + tooltipHeight > viewportHeight) {
      // Try above
      top = rect.top - tooltipHeight - margin;
      placement = 'above';
      
      // Still overflows? Try right
      if (top < 0) {
        top = rect.top;
        left = rect.right + margin;
        placement = 'right';
        
        // Still overflows? Try left
        if (left + tooltipWidth > viewportWidth) {
          left = rect.left - tooltipWidth - margin;
          placement = 'left';
        }
      }
    }
    
    // Clamp to viewport
    if (left + tooltipWidth > viewportWidth) {
      left = viewportWidth - tooltipWidth - 20;
    }
    if (left < 0) {
      left = 20;
    }
    if (top + tooltipHeight > viewportHeight) {
      top = viewportHeight - tooltipHeight - 20;
    }
    if (top < 0) {
      top = 20;
    }
    
    return { top, left, placement };
  }
}
```

### TourTooltipComponent

```typescript
@Component({
  selector: 'app-tour-tooltip',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule],
  template: `
    @if (tourService.currentStep; as step) {
      <div class="tour-backdrop" (click)="tourService.end()"></div>
      <div 
        class="tour-tooltip"
        [class.mobile]="tourService.isMobile"
        [ngStyle]="tooltipStyle"
      >
        <div class="tour-title">{{ step.title }}</div>
        <div class="tour-message" [innerText]="step.message"></div>
        <div class="tour-action-hint">{{ step.actionHint }}</div>
        <div class="tour-actions">
          <button mat-button (click)="tourService.end()">Skip Tour</button>
          <button mat-raised-button color="primary" (click)="tourService.next()">
            Next ({{ tourService.stepLabel }})
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .tour-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      z-index: 9998;
    }
    
    .tour-tooltip {
      position: fixed;
      background: white;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
      max-width: 400px;
      z-index: 9999;
      
      &.mobile {
        max-width: calc(100vw - 40px);
        padding: 20px;
      }
    }
    
    .tour-title {
      font-size: 20px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #1976d2;
    }
    
    .tour-message {
      font-size: 15px;
      line-height: 1.5;
      margin-bottom: 16px;
      white-space: pre-line;
    }
    
    .tour-action-hint {
      font-size: 14px;
      font-style: italic;
      color: #666;
      margin-bottom: 20px;
    }
    
    .tour-actions {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      
      button {
        min-height: 48px; /* Touch target */
      }
    }
  `]
})
export class TourTooltipComponent implements AfterViewInit {
  tourService = inject(TourService);
  tooltipStyle: { [key: string]: string } = {};
  
  private elRef = inject(ElementRef);
  
  ngAfterViewInit(): void {
    this.updatePosition();
  }
  
  private updatePosition(): void {
    const el = this.elRef.nativeElement.querySelector('.tour-tooltip') as HTMLElement;
    if (!el) return;
    
    const rect = el.getBoundingClientRect();
    const pos = this.tourService.calculateTooltipPosition(rect.width, rect.height);
    
    this.tooltipStyle = {
      top: `${pos.top}px`,
      left: `${pos.left}px`
    };
  }
}
```

### Onboarding Trigger (AppComponent)

```typescript
export class AppComponent implements OnInit {
  private dialog = inject(MatDialog);
  private onboardingService = inject(OnboardingService);
  private tourService = inject(TourService);
  private authService = inject(AuthService);
  
  ngOnInit(): void {
    this.authService.ready.then(() => {
      if (this.authService.isAuthenticated()) {
        this.checkOnboarding();
      }
    });
  }
  
  private checkOnboarding(): void {
    this.onboardingService.getStatus().subscribe(status => {
      if (!status.hasCompletedOnboarding) {
        this.showOnboardingDialog();
      }
    });
  }
  
  private showOnboardingDialog(): void {
    const dialogRef = this.dialog.open(OnboardingDialogComponent, {
      disableClose: false,
      width: '600px',
      maxWidth: '95vw',
      panelClass: 'onboarding-dialog'
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result?.startTour) {
        // Small delay to let the main UI render
        setTimeout(() => this.tourService.start(), 500);
      }
    });
  }
}
```

---

## Testing

### Backend Unit Tests

**OnboardingService:**

- `GetOnboardingStatusAsync_NewUser_ReturnsFalse`
- `GetOnboardingStatusAsync_ExistingUser_ReturnsProgress`
- `UpdateOnboardingProgressAsync_SavesProgress`
- `AddSampleRecipesAsync_ForksAllSamples_Returns6`
- `AddSampleRecipesAsync_IdempotentCall_ReturnsZero`
- `AddSampleRecipesAsync_MarksSamplesWithFlag`
- `RemoveSampleRecipesAsync_DeletesOnlySamples`
- `CompleteOnboardingAsync_SetsFlag_SetsDate`
- `ResetOnboardingAsync_ClearsFlag_ClearsDate_ClearsProgress`

**System Account:**

- `SystemAccountId_IsValidUUID5`
- `SystemAccountId_NotGuidEmpty`
- `SampleRecipes_OwnedBySystemAccount`

**Sample Recipe Seeding:**

- `SampleRecipes_Count_Is6`
- `ThaiBasilChicken_HasSubstitutions_Count5`
- `Shakshuka_HasTimers_Count3`
- `KoreanBibimbap_HasNutrition_Complete`
- `SpicyBibimbap_IsForkedFrom_KoreanBibimbap`
- `AllSampleRecipes_MarkedWithIsSampleRecipeFlag`

### Frontend Unit Tests

**OnboardingDialogComponent:**

- Dialog opens when `hasCompletedOnboarding: false`
- Dialog does not open when `hasCompletedOnboarding: true`
- Step navigation increments/decrements correctly
- Progress bar updates with step changes
- Skip button completes onboarding, closes dialog, does not start tour
- Step 2: Dietary restrictions toggle adds/removes from selection
- Step 2: Avoided ingredient add/remove works
- Step 2: "Save & Continue" calls API immediately
- Step 3: "Add 6 Samples" calls API, shows snackbar, marks progress
- Step 3: Import buttons trigger appropriate dialogs
- Mobile view: Full-screen dialog on <768px
- Desktop view: Centered dialog on ≥768px
- Swipe gestures advance/retreat steps (mobile only)
- All touch targets ≥48px on mobile

**TourService:**

- Tour starts at step 0
- Tour advances through 5 steps
- Tour navigates to correct recipe before each step
- Tour skips steps if target element not found
- Tour ends after final step
- Mobile tooltip: Always center-bottom
- Desktop tooltip: Collision-detected positioning
- Desktop tooltip: Flips above when overflows bottom
- Desktop tooltip: Flips left/right when overflows sides
- Desktop tooltip: Clamps to viewport bounds

**TourTooltipComponent:**

- Tooltip renders with correct step data
- "Skip Tour" button ends tour
- "Next" button advances to next step
- Tooltip repositions on window resize
- Tooltip uses mobile styling on small screens
- Backdrop click ends tour

### Integration Tests

**Full onboarding flow:**

1. GET `/api/v1/user/onboarding-status` → `hasCompletedOnboarding: false`
2. PATCH `/api/v1/user/onboarding-progress` (dietary profile set) → 204
3. POST `/api/v1/user/add-sample-recipes` → 6 recipes added
4. POST `/api/v1/user/complete-onboarding` → 204
5. GET `/api/v1/user/onboarding-status` → `hasCompletedOnboarding: true`

**Sample recipe verification:**

- 6 sample recipes appear in user's library
- All samples have `IsSampleRecipe: true`
- Sample badges render in recipe list
- DELETE `/api/v1/user/sample-recipes` removes all 6

**Dietary profile integration:**

- Profile created during onboarding is queryable
- Dietary conflicts show on recipes
- Conflict warning UI matches onboarding preview

**Re-trigger from settings:**

- Reset onboarding → dialog shows again on next load
- Reset onboarding → progress cleared

**Partial completion:**

- User closes after step 2 → dietary profile saved
- User returns → step 2 pre-filled with saved preferences
- User closes after adding samples → samples remain

---

## Edge Cases

1. **User closes dialog mid-flow** — Progress is saved per step. If closed at step 2, dietary preferences are preserved. `hasCompletedOnboarding` remains false, so onboarding re-triggers on next login.

2. **User already has recipes when onboarding triggers** — This can happen if they imported via API before visiting UI. Onboarding still shows, but step 3 message could be adjusted (future enhancement).

3. **User already has a dietary profile** — Skip step 2 or pre-fill with existing profile (future enhancement).

4. **Sample recipes already added** — `AddSampleRecipesAsync` checks fork references and skips duplicates. Returns count of 0 if all samples already exist.

5. **Multiple devices** — Once `completeOnboarding` is called, onboarding won't show on any device (server-side flag).

6. **Tour target elements not visible** — Tour skips steps whose elements aren't in DOM. If all steps are skipped, tour ends gracefully.

7. **Mobile user swipes too fast** — Debounce swipe gestures (300ms) to prevent accidental double-advancing.

8. **Tooltip overflows on ultra-small screens** — Mobile mode uses center-bottom card that's always visible. Max width is `calc(100vw - 40px)`.

9. **Sample recipe forked by user, then deleted by system** — User's fork remains intact (orphaned fork). No data loss.

10. **Concurrent sample recipe requests** — If user double-clicks "Add Samples," second request is handled by fork deduplication (returns 0).

11. **System account deleted/corrupted** — Sample recipe queries fail gracefully. Onboarding still works without samples (user can skip).

12. **User navigates away during tour** — Tour service tracks current step. If user navigates manually, tour continues from current step when possible.

---

## Future Enhancements

### Progressive Feature Discovery

- Show contextual tooltips for new features as they're released
- "You just unlocked: Meal Planning!" when user saves their 5th recipe
- Gamification: badges for milestones

### Personalized Sample Recipes

- Use dietary preferences from step 2 to filter/reorder samples
- Vegetarian user gets veggie-focused samples
- Allergy profiles exclude incompatible samples

### Video Walkthrough

- Optional embedded video tour for visual learners
- 90-second overview of killer features
- Hosted on Vimeo/YouTube, embedded in welcome screen

### Onboarding Analytics

- Track completion rates per step
- Identify drop-off points
- A/B test different messaging
- Send data to analytics service (PostHog, Mixpanel, etc.)

### Smart Tour Triggers

- Re-trigger tour for specific features when user seems stuck
- "Looks like you haven't tried cooking mode yet — want a quick tour?"
- Context-aware tooltips based on user behavior

### Multi-Language Support

- Translate onboarding flow
- Sample recipes in user's preferred language
- Localized dietary restriction labels

### Social Onboarding

- "Invite a friend" step
- Share sample recipes immediately to social circles
- Connect to existing contacts who use RecipeVault

---

## Implementation Checklist

### Backend

- [ ] Migration: Add `HasCompletedOnboarding`, `OnboardingCompletedDate`, `OnboardingProgressJson` to `UserAccount`
- [ ] Migration: Add `IsSampleRecipe` to `Recipe`
- [ ] Seed: System account with UUID `d290f1ee-6c54-5f96-8b2f-9f14e72c8c39`
- [ ] Seed: 6 sample recipes with full data (ingredients, instructions, tags, nutritional data, substitutions)
- [ ] Service: `OnboardingService` with all methods
- [ ] Controller: `/api/v1/user/onboarding-status`, `complete-onboarding`, `reset-onboarding`, `add-sample-recipes`, `sample-recipes` DELETE
- [ ] Tests: All unit tests for OnboardingService
- [ ] Tests: Integration tests for full flow

### Frontend

- [ ] Models: `OnboardingStatus`, `OnboardingProgress`, `AddSampleRecipesResult`
- [ ] Service: `OnboardingService` with HTTP methods
- [ ] Component: `OnboardingDialogComponent` with 4 steps
- [ ] Component: Mobile responsiveness (full-screen on <768px, swipe gestures)
- [ ] Component: Dietary profile UI with conflict warning preview
- [ ] Component: Sample recipes button with immediate feedback
- [ ] Service: `TourService` with 5 tour stops
- [ ] Component: `TourTooltipComponent` with collision detection
- [ ] Component: Mobile tooltip (center-bottom card)
- [ ] Component: Desktop tooltip (viewport-aware positioning)
- [ ] Trigger: AppComponent check for onboarding on auth
- [ ] UI: Sample recipe badges in recipe list
- [ ] UI: "Remove all sample recipes" button in settings
- [ ] Tests: All component unit tests
- [ ] Tests: Tour service positioning tests
- [ ] Tests: Mobile/desktop responsive tests

### Documentation

- [x] Complete design document (this file)
- [ ] Sample recipe JSON data files
- [ ] API documentation update
- [ ] User-facing onboarding help article

---

**Document Status:** ✅ Complete, ready for implementation

**Last Updated:** 2026-03-14

**Author:** NORA (AI Agent)
