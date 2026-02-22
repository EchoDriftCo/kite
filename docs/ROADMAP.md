# RecipeVault Product Roadmap

*Master planning document with detailed feature designs.*

---

## Table of Contents

1. [Recipe Forking & Remixes](#1-recipe-forking--remixes)
2. [Social Circles](#2-social-circles)
3. [Nutrition Integration](#3-nutrition-integration)
4. [Smart Substitutions & "What Can I Make?"](#4-smart-substitutions--what-can-i-make)
5. [Import & Export](#5-import--export)
6. [Voice & Cooking Mode](#6-voice--cooking-mode)

---

## Priority Order (Recommended)

| Phase | Feature | Rationale |
|-------|---------|-----------|
| 1 | Recipe Forking | Foundation for remixes, substitutions, social features |
| 2 | Smart Substitutions | High user value, builds on forking |
| 3 | Import/Export | User acquisition, reduces friction |
| 4 | Social Circles | Viral growth, sharing is core to recipes |
| 5 | Nutrition | Differentiator, complex integration |
| 6 | Voice/Cooking Mode | Polish feature, delightful UX |

---

## 1. Recipe Forking & Remixes

**Status:** Design complete — see [design/recipe-forking.md](design/recipe-forking.md)

### Summary

Users can "fork" any recipe (their own or public) to create a personal copy that maintains a reference to the original. Enables:

- Customizing shared recipes
- Creating variations (e.g., "Gluten-Free Version")
- Preserving originals while experimenting
- Foundation for substitution feature

### Key Decisions

- Fork links to **immediate parent**, not root (keeps it simple)
- Forks start **private** regardless of original's visibility
- Original deletion → fork becomes "orphaned" (still works, shows "original no longer available")
- **No merge/sync** from original (v1) — user manually re-forks if they want updates

### Data Model

```csharp
public class Recipe {
    public int? ForkedFromRecipeId { get; private set; }
    public virtual Recipe ForkedFromRecipe { get; private set; }
    public virtual ICollection<Recipe> Forks { get; private set; }
}
```

### API

```
POST /api/recipes/{id}/fork
  Body: { "title": "Optional new title" }
  Returns: Full RecipeDto of fork

GET /api/recipes/{id}/forks
  Returns: Paginated list of public forks (for discoverability)
```

### Implementation Estimate

- Backend: 4-6 hours
- Frontend: 4-6 hours
- Tests: 2-3 hours

---

## 2. Social Circles

### Overview

Private sharing groups for family and friends. Recipes can be shared to circles without making them fully public. Think "Mom's Cookbook" as a shared collection.

### User Stories

1. **Create a circle** — "Sunday Dinner Club" with family members
2. **Invite members** — Send invite via email or shareable link
3. **Share recipe to circle** — Recipe appears in circle's feed, not copied
4. **Browse circle recipes** — See all recipes shared to circles you're in
5. **Request a recipe** — Ask a circle member to share a specific recipe
6. **Leave circle** — Remove yourself, lose access to circle recipes

### Privacy Model

```
┌─────────────────────────────────────────────────────────┐
│                    VISIBILITY LEVELS                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│   Private ──► Circles ──► Public                        │
│      │           │           │                          │
│   Only me    Selected     Everyone                      │
│              groups                                     │
│                                                         │
│   Recipe can be in multiple circles simultaneously      │
│   "Circles" is additive - doesn't replace private/public│
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Data Model

```csharp
[Table("Circle")]
public class Circle {
    public int CircleId { get; private set; }
    public Guid CircleResourceId { get; private set; }
    
    [Required, StringLength(100)]
    public string Name { get; private set; }  // "Family Recipes"
    
    [StringLength(500)]
    public string Description { get; private set; }
    
    public int OwnerSubjectId { get; private set; }
    public virtual Subject Owner { get; private set; }
    
    public DateTime CreatedDate { get; private set; }
    
    public virtual ICollection<CircleMember> Members { get; private set; }
    public virtual ICollection<CircleRecipe> SharedRecipes { get; private set; }
}

[Table("CircleMember")]
public class CircleMember {
    public int CircleMemberId { get; private set; }
    public int CircleId { get; private set; }
    public int SubjectId { get; private set; }
    
    public CircleRole Role { get; private set; }  // Owner, Admin, Member
    public DateTime JoinedDate { get; private set; }
    public DateTime? InvitedDate { get; private set; }
    public MemberStatus Status { get; private set; }  // Pending, Active, Left
    
    public virtual Circle Circle { get; private set; }
    public virtual Subject Subject { get; private set; }
}

public enum CircleRole { Owner, Admin, Member }
public enum MemberStatus { Pending, Active, Left }

[Table("CircleRecipe")]
public class CircleRecipe {
    public int CircleRecipeId { get; private set; }
    public int CircleId { get; private set; }
    public int RecipeId { get; private set; }
    
    public int SharedBySubjectId { get; private set; }
    public DateTime SharedDate { get; private set; }
    
    public virtual Circle Circle { get; private set; }
    public virtual Recipe Recipe { get; private set; }
    public virtual Subject SharedBy { get; private set; }
}

// Invites for non-users or pending members
[Table("CircleInvite")]
public class CircleInvite {
    public int CircleInviteId { get; private set; }
    public Guid InviteToken { get; private set; }
    public int CircleId { get; private set; }
    
    [StringLength(255)]
    public string InviteeEmail { get; private set; }  // Null if link-based
    
    public int InvitedBySubjectId { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime ExpiresDate { get; private set; }
    public InviteStatus Status { get; private set; }  // Pending, Accepted, Expired, Revoked
}
```

### API Design

```
# Circles
POST   /api/circles                    Create circle
GET    /api/circles                    List my circles (owned + member of)
GET    /api/circles/{id}               Get circle details
PUT    /api/circles/{id}               Update circle (name, description)
DELETE /api/circles/{id}               Delete circle (owner only)

# Members
POST   /api/circles/{id}/invite        Invite by email or generate link
GET    /api/circles/{id}/members       List members
DELETE /api/circles/{id}/members/{subjectId}  Remove member (admin+) or leave (self)
PUT    /api/circles/{id}/members/{subjectId}  Change role (owner only)

# Invites
POST   /api/circles/join/{inviteToken}  Accept invite
GET    /api/circles/invite/{token}      Get invite details (for preview)

# Sharing
POST   /api/circles/{id}/recipes        Share recipe to circle
DELETE /api/circles/{id}/recipes/{recipeId}  Unshare recipe
GET    /api/circles/{id}/recipes        List recipes in circle (paginated)

# Cross-circle discovery
GET    /api/recipes?source=circles      All recipes from all my circles
```

### UX Flows

#### Create Circle

```
┌─────────────────────────────────────────────┐
│  Create a Circle                            │
│                                             │
│  Name: [Family Recipes____________]         │
│                                             │
│  Description (optional):                    │
│  [Grandma's classics and new favorites___]  │
│  [_______________________________________]  │
│                                             │
│  [Cancel]              [Create Circle]      │
└─────────────────────────────────────────────┘
```

#### Invite Members

```
┌─────────────────────────────────────────────┐
│  Invite to "Family Recipes"                 │
│                                             │
│  Email: [mom@email.com________________]     │
│         [Send Invite]                       │
│                                             │
│  ─── or ───                                 │
│                                             │
│  Share invite link:                         │
│  ┌────────────────────────────────────┐     │
│  │ recipevault.io/join/abc123xyz      │ 📋  │
│  └────────────────────────────────────┘     │
│  Link expires in 7 days                     │
│                                             │
│  Current members:                           │
│  • You (Owner)                              │
│  • dad@email.com (Pending)                  │
│                                             │
└─────────────────────────────────────────────┘
```

#### Share Recipe to Circle

On recipe detail, share button opens:

```
┌─────────────────────────────────────────────┐
│  Share "Mom's Apple Pie"                    │
│                                             │
│  Share to circles:                          │
│  ☑ Family Recipes                           │
│  ☐ Sunday Dinner Club                       │
│  ☐ College Friends                          │
│                                             │
│  [Cancel]                    [Share]        │
└─────────────────────────────────────────────┘
```

#### Browse Circle Recipes

New tab/filter in recipe list:

```
┌─────────────────────────────────────────────┐
│  [My Recipes] [Circles] [Public]            │
├─────────────────────────────────────────────┤
│                                             │
│  Filter by circle: [All circles ▼]          │
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │ 🥧 Grandma's Apple Pie              │    │
│  │ Shared by Mom · Family Recipes      │    │
│  │ ★★★★★ · 45 min                      │    │
│  │                   [Fork to My Recipes]   │
│  └─────────────────────────────────────┘    │
│                                             │
└─────────────────────────────────────────────┘
```

### Permissions Matrix

| Action | Owner | Admin | Member |
|--------|-------|-------|--------|
| View recipes | ✅ | ✅ | ✅ |
| Fork recipes | ✅ | ✅ | ✅ |
| Share recipe | ✅ | ✅ | ✅ |
| Unshare own recipe | ✅ | ✅ | ✅ |
| Unshare others' recipe | ✅ | ✅ | ❌ |
| Invite members | ✅ | ✅ | ❌ |
| Remove members | ✅ | ✅ | ❌ |
| Change roles | ✅ | ❌ | ❌ |
| Edit circle info | ✅ | ✅ | ❌ |
| Delete circle | ✅ | ❌ | ❌ |

### Implementation Estimate

- Backend: 12-16 hours
- Frontend: 16-20 hours
- Tests: 6-8 hours

---

## 3. Nutrition Integration

### Overview

Automatic nutritional analysis using USDA FoodData Central. Parse ingredients, match to USDA database, calculate per-serving macros.

### User Stories

1. **View nutrition** — See calories, macros, vitamins per serving
2. **Dietary warnings** — Flag allergens and dietary conflicts
3. **Meal plan nutrition** — See daily/weekly totals for planned meals
4. **Filter by nutrition** — Find recipes under 500 calories, high protein, etc.

### USDA FoodData Central Integration

**API:** https://fdc.nal.usda.gov/api-guide.html  
**Rate Limits:** 3,600 requests/hour with API key (free)  
**Data:** 300,000+ food items with full nutrient profiles

#### Search Flow

```
User ingredient: "2 cups chicken breast, diced"
                        ↓
┌─────────────────────────────────────────────┐
│  1. Parse ingredient                         │
│     quantity: 2                              │
│     unit: cups                               │
│     item: chicken breast                     │
│     preparation: diced                       │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│  2. Search USDA FoodData Central            │
│     GET /fdc/v1/foods/search?query=chicken+breast
│                                             │
│     Results:                                │
│     - Chicken, broilers, breast, meat only  │
│     - Chicken breast, rotisserie            │
│     - Chicken breast, deli                  │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│  3. Match & select best result              │
│     Use AI to pick best match from results  │
│     FDC ID: 171077                          │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│  4. Convert units & calculate               │
│     2 cups chicken ≈ 280g                   │
│     Per 100g: 165 cal, 31g protein...       │
│     This ingredient: 462 cal, 87g protein   │
└─────────────────────────────────────────────┘
```

### Data Model

```csharp
[Table("IngredientNutrition")]
public class IngredientNutrition {
    public int IngredientNutritionId { get; private set; }
    public int RecipeIngredientId { get; private set; }
    
    // USDA reference
    public int? FdcId { get; private set; }  // USDA FoodData Central ID
    public string MatchedFoodName { get; private set; }
    public decimal MatchConfidence { get; private set; }  // 0-1
    
    // Calculated values (per ingredient amount)
    public decimal? Calories { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbsGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public decimal? FiberGrams { get; private set; }
    public decimal? SugarGrams { get; private set; }
    public decimal? SodiumMg { get; private set; }
    
    // Conversion used
    public decimal GramsUsed { get; private set; }  // Amount in grams for calculation
    
    public DateTime CalculatedDate { get; private set; }
    public bool IsManualOverride { get; private set; }
    
    public virtual RecipeIngredient RecipeIngredient { get; private set; }
}

[Table("RecipeNutrition")]
public class RecipeNutrition {
    public int RecipeNutritionId { get; private set; }
    public int RecipeId { get; private set; }
    
    // Per-serving totals
    public decimal? CaloriesPerServing { get; private set; }
    public decimal? ProteinPerServing { get; private set; }
    public decimal? CarbsPerServing { get; private set; }
    public decimal? FatPerServing { get; private set; }
    public decimal? FiberPerServing { get; private set; }
    public decimal? SugarPerServing { get; private set; }
    public decimal? SodiumPerServing { get; private set; }
    
    // Coverage metrics
    public int IngredientsMatched { get; private set; }
    public int IngredientsTotal { get; private set; }
    public decimal CoveragePercent { get; private set; }
    
    public DateTime CalculatedDate { get; private set; }
    public bool IsStale { get; private set; }  // Recipe changed since calculation
    
    public virtual Recipe Recipe { get; private set; }
}
```

### Unit Conversion

Critical challenge: converting recipe units to grams for USDA lookup.

```csharp
public class UnitConverter {
    // Standard conversions (ingredient-agnostic)
    private static readonly Dictionary<string, decimal> ToGrams = new() {
        ["g"] = 1m,
        ["gram"] = 1m,
        ["grams"] = 1m,
        ["kg"] = 1000m,
        ["oz"] = 28.35m,
        ["ounce"] = 28.35m,
        ["lb"] = 453.59m,
        ["pound"] = 453.59m,
    };
    
    // Volume conversions need density (ingredient-specific)
    // USDA provides "portion" data we can use
    private static readonly Dictionary<string, decimal> ToMl = new() {
        ["ml"] = 1m,
        ["l"] = 1000m,
        ["tsp"] = 4.93m,
        ["tbsp"] = 14.79m,
        ["cup"] = 236.59m,
        ["pint"] = 473.18m,
        ["quart"] = 946.35m,
    };
}
```

For volume → grams, use USDA's portion data:
- API returns `foodPortions` with serving sizes
- "1 cup, diced" = X grams for specific foods
- Fall back to AI estimation if no portion data

### API Design

```
# Analyze single recipe
POST /api/recipes/{id}/nutrition/analyze
  Triggers USDA matching + calculation
  Returns: RecipeNutritionDto

# Get cached nutrition (fast)
GET /api/recipes/{id}/nutrition
  Returns: RecipeNutritionDto or 404 if not calculated

# Override ingredient match
PUT /api/recipes/{id}/ingredients/{index}/nutrition
  Body: { "fdcId": 171077, "gramsUsed": 280 }
  Manual override for bad AI matches

# Search USDA (for manual matching UI)
GET /api/nutrition/search?query=chicken+breast
  Proxy to USDA API, returns top matches

# Meal plan nutrition
GET /api/mealplans/{id}/nutrition
  Aggregates nutrition for all recipes in plan
```

### UX Design

#### Recipe Detail - Nutrition Panel

```
┌─────────────────────────────────────────────┐
│  📊 Nutrition (per serving)                 │
├─────────────────────────────────────────────┤
│                                             │
│  Calories      385      ████████░░  19% DV  │
│  Protein       28g      ██████████  56% DV  │
│  Carbs         42g      ██████░░░░  14% DV  │
│  Fat           12g      █████░░░░░  15% DV  │
│  Fiber          4g      ████░░░░░░  14% DV  │
│  Sodium       580mg     ████████░░  25% DV  │
│                                             │
│  ℹ️ Based on 7/8 ingredients matched (88%)  │
│  [Improve accuracy]                         │
│                                             │
└─────────────────────────────────────────────┘
```

#### Ingredient Match Review

```
┌─────────────────────────────────────────────────┐
│  Review Ingredient Matches                       │
├─────────────────────────────────────────────────┤
│                                                 │
│  ✅ 2 cups chicken breast, diced                │
│     → Chicken, broilers, breast, meat only      │
│     280g · 462 cal                              │
│                                                 │
│  ⚠️ 1 cup mystery sauce                         │
│     → No match found                    [Fix]   │
│                                                 │
│  ✅ 3 tbsp olive oil                            │
│     → Oil, olive, salad or cooking              │
│     42g · 371 cal                               │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Caching Strategy

1. **USDA food data** — Cache FDC responses for 30 days (data rarely changes)
2. **Ingredient matches** — Cache ingredient→FDC mapping per user (learns preferences)
3. **Recipe nutrition** — Calculated on-demand, invalidate when recipe changes
4. **Meal plan nutrition** — Calculate on view, no persistence

### Implementation Estimate

- Backend (USDA integration): 8-12 hours
- Backend (nutrition service): 8-10 hours
- Frontend: 12-16 hours
- Tests: 6-8 hours

---

## 4. Smart Substitutions & "What Can I Make?"

### Overview

Two related features powered by AI:

1. **Smart Substitutions** — "I don't have X, what can I use instead?"
2. **What Can I Make?** — "I have these ingredients, what recipes work?"

### 4A. Smart Substitutions

**Status:** Design complete — see [design/ingredient-substitutions.md](design/ingredient-substitutions.md)

#### Summary

- User selects ingredients to substitute OR dietary constraints
- Gemini analyzes recipe context and suggests 2-3 alternatives per ingredient
- User picks options and applies → creates a fork with substitutions

#### Key Decisions

- Requires forking (substitutions create modified copy)
- Cache substitution results for 24 hours
- Two modes: specific ingredients OR dietary constraint scan

### 4B. Pantry & "What Can I Make?"

#### User Stories

1. **Maintain pantry** — Track what ingredients I have on hand
2. **Filter recipes** — Show recipes I can make with current pantry
3. **Missing ingredients** — Show what I'm missing for each recipe
4. **Shopping suggestions** — "Buy X to unlock 5 more recipes"

#### Data Model

```csharp
[Table("PantryItem")]
public class PantryItem {
    public int PantryItemId { get; private set; }
    public int SubjectId { get; private set; }
    
    [Required, StringLength(200)]
    public string ItemName { get; private set; }  // Normalized name
    
    [StringLength(100)]
    public string Category { get; private set; }  // Dairy, Produce, Pantry, etc.
    
    public decimal? Quantity { get; private set; }
    public string Unit { get; private set; }
    
    public DateTime? ExpirationDate { get; private set; }
    public DateTime AddedDate { get; private set; }
    public DateTime? LastUpdated { get; private set; }
    
    public virtual Subject Subject { get; private set; }
}

// For ingredient matching
[Table("IngredientAlias")]
public class IngredientAlias {
    public int IngredientAliasId { get; private set; }
    
    [Required, StringLength(200)]
    public string CanonicalName { get; private set; }  // "chicken breast"
    
    [Required, StringLength(200)]  
    public string Alias { get; private set; }  // "chicken breasts", "boneless chicken"
}
```

#### Ingredient Matching

Challenge: "chicken breast" in recipe must match "chicken" in pantry.

```
Recipe ingredient: "2 lbs boneless skinless chicken breasts"
Pantry item: "chicken breast"

Matching approach:
1. Normalize both strings (lowercase, remove plurals)
2. Extract core item from recipe ingredient (AI or rules)
3. Check IngredientAlias table for known mappings
4. Fuzzy match with confidence threshold
5. AI fallback for complex cases
```

#### API Design

```
# Pantry management
GET    /api/pantry                    List pantry items
POST   /api/pantry                    Add item
PUT    /api/pantry/{id}               Update item
DELETE /api/pantry/{id}               Remove item
POST   /api/pantry/bulk               Add multiple items (from grocery list)

# Quick actions
POST   /api/pantry/scan               AI parse pantry from text/photo
DELETE /api/pantry/expired            Remove expired items

# Recipe filtering
GET    /api/recipes?canMake=true      Recipes I can make with pantry
GET    /api/recipes?maxMissing=2      Recipes missing ≤2 ingredients
GET    /api/recipes/{id}/availability Check specific recipe vs pantry
```

#### Recipe Availability Response

```json
{
  "recipeResourceId": "...",
  "canMake": false,
  "availableIngredients": 6,
  "totalIngredients": 8,
  "missingIngredients": [
    {
      "index": 2,
      "text": "1 cup heavy cream",
      "normalizedItem": "heavy cream",
      "substitutes": ["half and half", "coconut cream"]
    },
    {
      "index": 5,
      "text": "fresh thyme",
      "normalizedItem": "thyme",
      "substitutes": ["dried thyme (1/3 amount)"]
    }
  ]
}
```

#### UX Design

##### Pantry Management

```
┌─────────────────────────────────────────────┐
│  🥫 My Pantry                    [+ Add]    │
├─────────────────────────────────────────────┤
│  Filter: [All ▼]  Sort: [Category ▼]        │
│                                             │
│  🥛 DAIRY                                   │
│  ├─ Milk (1 gallon) · Exp: Feb 28           │
│  ├─ Butter (1 lb)                           │
│  └─ Cheddar cheese (8 oz)                   │
│                                             │
│  🥩 PROTEIN                                 │
│  ├─ Chicken breast (2 lbs) · Exp: Feb 25 ⚠️ │
│  └─ Ground beef (1 lb)                      │
│                                             │
│  🥫 PANTRY                                  │
│  ├─ Olive oil                               │
│  ├─ All-purpose flour                       │
│  └─ ... (12 more)                           │
│                                             │
└─────────────────────────────────────────────┘
```

##### Recipe List with Availability

```
┌─────────────────────────────────────────────┐
│  [My Recipes] [Can Make ✓]                  │
├─────────────────────────────────────────────┤
│                                             │
│  ┌────────────────────────────────────────┐ │
│  │ ✅ Chicken Stir Fry                    │ │
│  │    All ingredients available           │ │
│  └────────────────────────────────────────┘ │
│                                             │
│  ┌────────────────────────────────────────┐ │
│  │ 🟡 Beef Stroganoff                     │ │
│  │    Missing: sour cream, egg noodles    │ │
│  └────────────────────────────────────────┘ │
│                                             │
│  ┌────────────────────────────────────────┐ │
│  │ 🔴 Shrimp Scampi                       │ │
│  │    Missing: shrimp, white wine, + 2    │ │
│  └────────────────────────────────────────┘ │
│                                             │
└─────────────────────────────────────────────┘
```

##### Shopping Suggestions

```
┌─────────────────────────────────────────────┐
│  🛒 Shopping Suggestions                    │
├─────────────────────────────────────────────┤
│                                             │
│  Buy these to unlock more recipes:          │
│                                             │
│  • Heavy cream → unlocks 5 recipes          │
│  • Shrimp → unlocks 3 recipes               │
│  • Fresh basil → unlocks 3 recipes          │
│                                             │
│  [Add all to grocery list]                  │
│                                             │
└─────────────────────────────────────────────┘
```

### Implementation Estimate

**Substitutions:**
- Backend: 8-10 hours
- Frontend: 8-10 hours
- Tests: 4-6 hours

**Pantry/What Can I Make:**
- Backend: 12-16 hours
- Frontend: 12-16 hours
- Tests: 6-8 hours

---

## 5. Import & Export

### Overview

Multiple import sources and a browser extension for seamless recipe capture.

### 5A. Browser Extension

#### Features

1. **Detect recipe pages** — Identify recipe content on any website
2. **One-click import** — Parse and save to RecipeVault
3. **Preview before save** — Review parsed data, edit if needed
4. **Quick save** — For logged-in users, save directly

#### Technical Approach

```
┌─────────────────────────────────────────────┐
│  BROWSER EXTENSION ARCHITECTURE             │
├─────────────────────────────────────────────┤
│                                             │
│  Content Script (runs on recipe pages)      │
│  ├─ Detect JSON-LD schema.org/Recipe        │
│  ├─ Fallback: heuristic DOM scanning        │
│  ├─ Send to background script               │
│                                             │
│  Background Script                          │
│  ├─ Manage auth state                       │
│  ├─ Call RecipeVault API                    │
│  ├─ Handle offline queue                    │
│                                             │
│  Popup UI                                   │
│  ├─ Login/logout                            │
│  ├─ Preview parsed recipe                   │
│  ├─ Edit before save                        │
│  ├─ Success/error feedback                  │
│                                             │
└─────────────────────────────────────────────┘
```

#### Schema.org Detection

Most recipe sites use JSON-LD:

```html
<script type="application/ld+json">
{
  "@type": "Recipe",
  "name": "Chocolate Chip Cookies",
  "recipeIngredient": ["2 cups flour", "1 cup sugar", ...],
  "recipeInstructions": [...]
}
</script>
```

Extension extracts this structured data directly — no AI needed.

#### Fallback Parsing

For sites without schema.org:
1. Send page HTML to RecipeVault API
2. API uses Gemini to extract recipe
3. Return structured data to extension

#### API Design

```
# Import from URL (server-side fetch + parse)
POST /api/recipes/import/url
  Body: { "url": "https://example.com/recipe" }
  Returns: Parsed recipe for preview

# Import from HTML (extension sends page content)
POST /api/recipes/import/html  
  Body: { "html": "...", "sourceUrl": "..." }
  Returns: Parsed recipe for preview

# Save imported recipe
POST /api/recipes/import/save
  Body: { parsed recipe + any user edits }
  Returns: Created RecipeDto
```

### 5B. Paprika Import

Paprika exports to `.paprikarecipes` format (gzipped JSON).

```csharp
public class PaprikaRecipe {
    public string name { get; set; }
    public string ingredients { get; set; }  // Newline-separated
    public string directions { get; set; }   // Newline-separated
    public string source { get; set; }
    public string source_url { get; set; }
    public string servings { get; set; }
    public string prep_time { get; set; }
    public string cook_time { get; set; }
    public string notes { get; set; }
    public string photo_data { get; set; }  // Base64
    public string[] categories { get; set; }
}
```

#### Import Flow

```
┌─────────────────────────────────────────────┐
│  Import from Paprika                        │
├─────────────────────────────────────────────┤
│                                             │
│  1. Export from Paprika                     │
│     File → Export → All Recipes             │
│     Creates: recipes.paprikarecipes         │
│                                             │
│  2. Upload file here                        │
│     [Choose File] recipes.paprikarecipes    │
│                                             │
│  3. Review & import                         │
│     Found 47 recipes                        │
│     ☑ Select all                            │
│     ☐ Grandma's Cookies                     │
│     ☐ Quick Pasta                           │
│     ...                                     │
│                                             │
│  [Cancel]           [Import Selected]       │
│                                             │
└─────────────────────────────────────────────┘
```

### 5C. Batch Import (Async Processing)

For large imports (cookbooks, bulk URLs):

```csharp
[Table("ImportJob")]
public class ImportJob {
    public int ImportJobId { get; private set; }
    public Guid ImportJobResourceId { get; private set; }
    public int SubjectId { get; private set; }
    
    public ImportJobType Type { get; private set; }  // Paprika, UrlBatch, Cookbook
    public ImportJobStatus Status { get; private set; }  // Pending, Processing, Complete, Failed
    
    public int TotalItems { get; private set; }
    public int ProcessedItems { get; private set; }
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }
    
    public DateTime CreatedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    
    public string ResultsJson { get; private set; }  // Details per item
}
```

#### API Design

```
# Start batch import
POST /api/import/batch
  Body: multipart form with file
  Returns: { importJobId: "...", status: "pending" }

# Check status
GET /api/import/{jobId}
  Returns: ImportJobDto with progress

# Get results
GET /api/import/{jobId}/results
  Returns: Per-item success/failure details

# Cancel job
DELETE /api/import/{jobId}
```

### 5D. Export

```
# Export single recipe
GET /api/recipes/{id}/export?format=json|paprika|pdf|text

# Export all recipes
POST /api/export/all
  Body: { "format": "paprika", "includeImages": true }
  Returns: { exportJobId: "..." }  // Async for large exports

# Download export
GET /api/export/{jobId}/download
```

### Implementation Estimate

**Browser Extension:**
- Extension code: 16-24 hours
- Backend endpoints: 6-8 hours
- Chrome + Firefox: +4 hours each

**Paprika Import:**
- Backend: 6-8 hours
- Frontend: 4-6 hours

**Batch Processing:**
- Backend (queue system): 8-12 hours
- Frontend: 4-6 hours

**Export:**
- Backend: 6-8 hours
- Frontend: 2-4 hours

---

## 6. Voice & Cooking Mode

### Overview

Hands-free cooking experience with voice control and step-by-step guidance.

### 6A. Cooking Mode

Transform recipe view into a step-by-step cooking interface.

#### Features

1. **Large text display** — Easy to read from across kitchen
2. **Step-by-step navigation** — Next/previous with voice or tap
3. **Built-in timers** — Extracted from instructions, start with voice
4. **Keep screen awake** — Prevent screen timeout while cooking
5. **Ingredient checklist** — Mark off as you prep

#### UX Design

```
┌─────────────────────────────────────────────────────┐
│  🍳 COOKING: Mom's Apple Pie          [Exit] ▪️▪️▪️ │
├─────────────────────────────────────────────────────┤
│                                                     │
│                    Step 3 of 8                      │
│                                                     │
│  ┌───────────────────────────────────────────────┐  │
│  │                                               │  │
│  │     Roll out the dough on a floured          │  │
│  │     surface to about ⅛ inch thick.           │  │
│  │     Transfer to a 9-inch pie dish.           │  │
│  │                                               │  │
│  └───────────────────────────────────────────────┘  │
│                                                     │
│                                                     │
│           [◀ PREV]          [NEXT ▶]                │
│                                                     │
│  ─────────────────────────────────────────────────  │
│                                                     │
│  📋 Ingredients        ⏱️ Timers                    │
│  ☑ 2 cups flour       [Start 45:00] Bake pie       │
│  ☑ 1 cup butter       [Start 10:00] Rest dough     │
│  ☐ 6 apples, sliced                                │
│  ☐ 1 cup sugar                                     │
│                                                     │
├─────────────────────────────────────────────────────┤
│  🎤 "Hey RecipeVault, next step"                    │
└─────────────────────────────────────────────────────┘
```

#### Timer Extraction

Parse instructions for time references:

```
Input: "Bake at 350°F for 45 minutes until golden brown"

Extracted:
- Action: "Bake"
- Duration: 45 minutes  
- Temperature: 350°F
- Completion indicator: "golden brown"
```

Use AI or regex patterns:
- "for X minutes/hours"
- "X minutes until"
- "let rest X minutes"
- "cook X-Y minutes"

### 6B. Voice Control

#### Commands

| Command | Action |
|---------|--------|
| "Next step" / "Next" | Go to next instruction |
| "Previous" / "Go back" | Go to previous instruction |
| "Read step" / "What's this step?" | Read current step aloud |
| "Start timer" / "Set timer for X" | Start relevant timer |
| "How much [ingredient]?" | Read ingredient quantity |
| "List ingredients" | Read all ingredients |
| "What step am I on?" | Announce step number |

#### Implementation Options

**Option A: Web Speech API (Browser-native)**
- Free, no API costs
- Works offline
- Limited accuracy
- Browser-dependent

**Option B: Cloud Speech (Google/Azure/OpenAI Whisper)**
- Better accuracy
- API costs per request
- Requires internet
- More complex integration

**Recommendation:** Start with Web Speech API, add cloud fallback for complex queries.

```typescript
// Web Speech API integration
const recognition = new webkitSpeechRecognition();
recognition.continuous = true;
recognition.interimResults = false;

recognition.onresult = (event) => {
  const transcript = event.results[event.results.length - 1][0].transcript;
  handleVoiceCommand(transcript.toLowerCase());
};

function handleVoiceCommand(command: string) {
  if (command.includes('next')) {
    goToNextStep();
  } else if (command.includes('previous') || command.includes('back')) {
    goToPreviousStep();
  } else if (command.includes('timer')) {
    startTimer();
  } else if (command.includes('read')) {
    speakCurrentStep();
  }
  // ... etc
}
```

### 6C. Text-to-Speech

Read steps and ingredients aloud.

```typescript
function speakCurrentStep() {
  const utterance = new SpeechSynthesisUtterance(currentStep.instruction);
  utterance.rate = 0.9;  // Slightly slower for clarity
  utterance.pitch = 1;
  speechSynthesis.speak(utterance);
}
```

**Enhancement:** Use ElevenLabs for higher quality voice (costs per character).

### 6D. Implementation Architecture

```
┌─────────────────────────────────────────────────────┐
│                  COOKING MODE                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│  CookingModeService                                 │
│  ├─ parseTimers(instructions): Timer[]             │
│  ├─ extractTemperatures(instructions): Temp[]      │
│  └─ prepareSteps(recipe): CookingStep[]            │
│                                                     │
│  VoiceService                                       │
│  ├─ startListening()                               │
│  ├─ stopListening()                                │
│  ├─ parseCommand(transcript): Command              │
│  └─ speak(text, options?)                          │
│                                                     │
│  TimerService                                       │
│  ├─ createTimer(name, seconds)                     │
│  ├─ startTimer(id)                                 │
│  ├─ pauseTimer(id)                                 │
│  ├─ resetTimer(id)                                 │
│  └─ onTimerComplete(callback)                      │
│                                                     │
│  WakeLockService                                    │
│  ├─ requestWakeLock()                              │
│  └─ releaseWakeLock()                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### API Design

Mostly frontend, minimal backend:

```
# Timer extraction (can be done client-side or server)
POST /api/recipes/{id}/cooking-data
  Returns: {
    steps: CookingStep[],
    timers: Timer[],
    temperatures: Temperature[]
  }
```

### Implementation Estimate

**Cooking Mode UI:**
- Frontend: 16-20 hours
- Timer system: 4-6 hours
- Tests: 4-6 hours

**Voice Control:**
- Web Speech integration: 8-12 hours
- Command parser: 4-6 hours
- Text-to-speech: 2-4 hours

**Total Voice + Cooking:** 40-55 hours

---

## Summary: Total Estimates

| Feature | Backend | Frontend | Tests | Total |
|---------|---------|----------|-------|-------|
| Recipe Forking | 4-6h | 4-6h | 2-3h | 10-15h |
| Social Circles | 12-16h | 16-20h | 6-8h | 34-44h |
| Nutrition | 16-22h | 12-16h | 6-8h | 34-46h |
| Smart Substitutions | 8-10h | 8-10h | 4-6h | 20-26h |
| Pantry/What Can I Make | 12-16h | 12-16h | 6-8h | 30-40h |
| Browser Extension | 6-8h | 16-24h | 4-6h | 26-38h |
| Paprika Import | 6-8h | 4-6h | 2-3h | 12-17h |
| Batch Processing | 8-12h | 4-6h | 2-4h | 14-22h |
| Export | 6-8h | 2-4h | 2-3h | 10-15h |
| Voice/Cooking Mode | 4-6h | 32-40h | 6-10h | 42-56h |

**Grand Total: 232-319 hours** (roughly 6-8 weeks of focused development)

---

## Dependencies

```
Recipe Forking
     │
     ├──► Smart Substitutions (requires fork to save)
     │
     └──► Social Circles (fork shared recipes)

Pantry ◄── Shopping Suggestions
  │
  └──► "What Can I Make?" filter

Browser Extension ──► Batch Import (uses same parsing)

Cooking Mode ◄── Voice Control (optional enhancement)
```

---

*This document is the source of truth for feature planning. Update as designs evolve.*
