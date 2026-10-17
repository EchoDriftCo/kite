# RecipeVault Product Roadmap

*Master planning document with detailed feature designs.*

---

## Table of Contents

**Core Features**
1. [Recipe Forking & Remixes](#1-recipe-forking--remixes)
2. [Social Circles](#2-social-circles)
3. [Nutrition Integration](#3-nutrition-integration)
4. [Smart Substitutions & "What Can I Make?"](#4-smart-substitutions--what-can-i-make)
5. [Import & Export](#5-import--export)
6. [Voice & Cooking Mode](#6-voice--cooking-mode)

**Additional Features**
7. [Recipe Collections & Cookbooks](#7-recipe-collections--cookbooks)
8. [Dietary Profiles](#8-dietary-profiles)
9. [Cooking History & Stats](#9-cooking-history--stats)
10. [AI Recipe Generation](#10-ai-recipe-generation)
11. [Kitchen Equipment Filtering](#11-kitchen-equipment-filtering)
12. [Grocery Delivery Integration](#12-grocery-delivery-integration)
13. [Recipe Linking (Component Recipes)](#13-recipe-linking-component-recipes)
14. [Recipe Mixing (AI Fusion)](#14-recipe-mixing-ai-fusion)

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

##### Quick Setup (First Time)

On first visit, show scrollable quick-select of common ingredients derived from global recipe database:

```
┌─────────────────────────────────────────────────┐
│  🥫 Stock Your Pantry                           │
│  Select items you typically have on hand        │
├─────────────────────────────────────────────────┤
│                                                 │
│  🥛 DAIRY & EGGS                                │
│  ┌───────────────────────────────────────────┐  │
│  │ ☑ Butter    ☑ Eggs    ☑ Milk             │  │
│  │ ☐ Heavy cream  ☑ Cheddar  ☐ Parmesan     │  │
│  │ ☐ Sour cream   ☐ Cream cheese  ☐ Yogurt  │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  🥩 PROTEINS                                    │
│  ┌───────────────────────────────────────────┐  │
│  │ ☑ Chicken breast  ☐ Ground beef          │  │
│  │ ☐ Bacon    ☐ Pork chops    ☐ Shrimp      │  │
│  │ ☐ Salmon   ☐ Tofu          ☐ Sausage     │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  🥬 PRODUCE                                     │
│  ┌───────────────────────────────────────────┐  │
│  │ ☑ Onions   ☑ Garlic   ☑ Potatoes         │  │
│  │ ☐ Carrots  ☐ Celery   ☐ Bell peppers     │  │
│  │ ☐ Tomatoes ☐ Lemons   ☐ Mushrooms        │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  🥫 PANTRY STAPLES                              │
│  ┌───────────────────────────────────────────┐  │
│  │ ☑ Olive oil   ☑ Salt    ☑ Black pepper   │  │
│  │ ☑ Flour       ☑ Sugar   ☑ Chicken broth  │  │
│  │ ☐ Rice        ☐ Pasta   ☐ Soy sauce      │  │
│  │ ☐ Honey       ☐ Vinegar ☐ Canned tomatoes│  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  📊 Items ranked by frequency in RecipeVault   │
│                                                 │
│  [Skip for now]              [Add 14 Items]    │
└─────────────────────────────────────────────────┘
```

##### Pantry Management (After Setup)

```
┌─────────────────────────────────────────────┐
│  🥫 My Pantry             [+ Add] [⚡ Quick]│
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

The "⚡ Quick" button reopens the quick-select view for fast bulk additions.

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

### 🚨 5B. Whisk Migration Tool (URGENT — 35-Day Deadline)

**Status:** Not started  
**Deadline:** April 30, 2026 (Whisk shutdown)  
**Priority:** CRITICAL — First-mover advantage for displaced users  
**Competitive Intel:** No other app has shipped this yet (as of March 26, 2026)

#### Business Context

- **Whisk shutting down April 30, 2026** — Samsung Food rebrand failed, users abandoning platform
- **35 days remaining** to capture displaced user base
- **Zero competitors have migration tools** — whoever ships first wins this cohort
- **Stringer flagged March 18** — window actively closing
- **Reddit/social:** Whisk users actively searching for alternatives now

#### User Story

"I have 500 recipes in Whisk. Samsung Food is a disaster and I don't trust it. I need to export my library before April 30 and move to a new app. RecipeVault says they can import my entire Whisk library in 60 seconds."

#### Technical Approach

Whisk export format unknown — need to:
1. **Reverse-engineer Whisk export** (JSON? CSV? proprietary?)
2. **Test with real Whisk account** or find sample exports online
3. **Build parser** for Whisk → RecipeVault schema
4. **Handle edge cases:** missing fields, image URLs, malformed data

**Deliverable:** Simple upload form that accepts Whisk exports and batch-imports recipes.

#### Landing Page Copy

**Headline:** "Switching from Whisk? Import everything in 60 seconds."

**Subhead:** "Whisk is shutting down April 30. We've got you covered. Drag your export file, import your recipes, and get cooking."

**CTA:** "Import from Whisk (Free)" → Upload form

**SEO keywords:** Whisk alternative, Whisk shutdown, migrate from Whisk, Samsung Food alternative

#### Metrics to Track

- Landing page visits (organic + Reddit/social)
- Upload attempts
- Successful imports (# users, # recipes)
- Conversion: Whisk import → active RecipeVault user
- Social mentions: "Whisk alternative" + "RecipeVault"

#### Implementation Estimate

- **Research/reverse-engineer Whisk format:** 2-4 hours
- **Backend parser + import logic:** 4-6 hours
- **Frontend upload UI:** 2-3 hours
- **Landing page copy + SEO:** 1-2 hours
- **Testing with real Whisk data:** 2-3 hours
- **Total:** ~15-18 hours (2-week sprint window)

#### Design Decisions Needed

- Batch vs. one-at-a-time import preview?
- Image handling (Whisk URLs may expire post-shutdown)
- Category/tag mapping (Whisk → RecipeVault taxonomy)
- Duplicate detection (if user re-imports)

**Next Step:** Kovacs + Vega — design the import flow and UI, then build.

---

### 5C. Paprika Import

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

### 5E. Multi-Image Import

**Status:** Planned (user feedback from beta tester Tiana, Feb 2026)

#### Problem

Users want to import recipes from physical cookbooks or printed pages that span multiple pages. Currently, image import only supports a single photo, forcing users to either:
- Manually combine images before uploading
- Type out the second page

#### Solution

Allow uploading 2-3 images that get stitched/combined before OCR processing.

#### User Flow

```
┌─────────────────────────────────────────────────┐
│  Import Recipe from Images                      │
├─────────────────────────────────────────────────┤
│                                                 │
│  Upload photos of your recipe:                  │
│                                                 │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐         │
│  │  📷 1   │  │  📷 2   │  │  + Add  │         │
│  │ (drag   │  │         │  │         │         │
│  │  to     │  │         │  │         │         │
│  │ reorder)│  │         │  │         │         │
│  └─────────┘  └─────────┘  └─────────┘         │
│                                                 │
│  ℹ️ Images will be processed in order shown    │
│                                                 │
│  [Cancel]              [Process Images]         │
│                                                 │
└─────────────────────────────────────────────────┘
```

#### Technical Approach

1. Accept 1-4 images via upload
2. Allow drag-to-reorder for correct page sequence
3. Options for processing:
   - **Sequential OCR**: Process each image separately, concatenate results
   - **Visual stitch**: Combine images vertically, send as single large image to Gemini
4. Parse combined text as single recipe

#### API Design

```
# Multi-image import
POST /api/recipes/import/images
  Body: multipart form with multiple images + order metadata
  Returns: Parsed recipe for preview
```

#### Implementation Estimate

- Backend: 4-6 hours
- Frontend: 6-8 hours
- Tests: 2-3 hours

---

### 5F. Video Import

**Status:** Priority — competitive gap (Umami, FoodiePrep, Forkee all have this)

#### Overview

Import recipes from TikTok, Instagram Reels, and YouTube cooking videos. Users paste a URL and get a structured recipe extracted from video content.

#### Why Now

- **Competitive pressure:** Umami added video-to-recipe import in Oct 2025. FoodiePrep, Forkee, CookBook, and PantryPilot also support it. RecipeVault is behind.
- **User demand:** Video recipes are the dominant discovery channel (TikTok, IG Reels, YouTube Shorts). Users find recipes in video form and need to save them as text.
- **Market trend:** "Video-first recipe capture" identified by HUSTLE as the #1 feature trend in recipe app space for 2026.

#### Technical Approach

```
User pastes URL → Backend downloads audio → Whisper transcription →
Gemini extraction (structured recipe) → User preview/edit → Save
```

**Step 1: Audio Extraction**
- Use `yt-dlp` to download audio from YouTube, TikTok, Instagram
- Supports most video platforms without API keys
- Extract audio-only (smaller, faster)

**Step 2: Speech-to-Text**
- Gemini audio input (preferred — already integrated, handles long audio)
- Fallback: OpenAI Whisper API if needed for accuracy
- Handle: background music, multiple speakers, varying audio quality

**Step 3: Recipe Extraction**
- Send transcript to Gemini with recipe extraction prompt
- Extract: title, ingredients (with quantities), instructions, times, servings
- Handle: informal language ("a good amount of salt"), visual-only steps ("as you can see")

**Step 4: Supplemental Data**
- Also extract on-screen text/captions if available (yt-dlp can pull subtitles)
- Merge spoken + captioned content for better accuracy
- Pull video thumbnail as recipe image

#### Data Model

Extends existing import infrastructure:

```csharp
// New import source type
public enum ImportSourceType {
    Url,        // Existing
    Html,       // Existing  
    Image,      // Existing
    Paprika,    // Existing
    Video       // NEW
}

// Video-specific metadata stored in existing ImportJob
// ResultsJson includes: videoUrl, platform, duration, transcriptConfidence
```

#### API Design

```
# Import from video URL
POST /api/recipes/import/video
  Body: { 
    "url": "https://www.tiktok.com/@chef/video/123",
    "includeSubtitles": true  // Also extract captions
  }
  Returns: {
    "recipe": { ...parsed recipe for preview... },
    "transcript": "...",
    "confidence": 0.87,
    "platform": "tiktok",
    "duration": "00:01:32",
    "thumbnailUrl": "..."
  }

# Save imported video recipe (uses existing save endpoint)
POST /api/recipes/import/save
  Body: { parsed recipe + user edits + sourceVideoUrl }
```

#### UX Design

```
┌─────────────────────────────────────────────────────┐
│  📹 Import from Video                               │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Paste a video URL:                                 │
│  ┌───────────────────────────────────────────────┐  │
│  │ https://www.tiktok.com/@chef/video/123456     │  │
│  └───────────────────────────────────────────────┘  │
│                                                     │
│  Supported: TikTok, Instagram, YouTube, Shorts     │
│                                                     │
│  [Import Recipe]                                    │
│                                                     │
└─────────────────────────────────────────────────────┘
```

Processing view:
```
┌─────────────────────────────────────────────────────┐
│  📹 Importing from TikTok...                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ✅ Downloading audio...                             │
│  ✅ Transcribing speech...                           │
│  ⏳ Extracting recipe...                             │
│                                                     │
│  This may take 15-30 seconds                        │
│                                                     │
└─────────────────────────────────────────────────────┘
```

#### Challenges & Mitigations

| Challenge | Mitigation |
|-----------|-----------|
| Copyright/TOS for video platforms | Use `yt-dlp` (audio only, not re-hosting), same as users downloading for personal use |
| Long-form video (YouTube) | Segment audio, process in chunks, stitch transcript |
| Informal spoken measurements | Gemini prompt handles "a splash of", "a good pinch" → reasonable defaults |
| Background music / noise | Whisper/Gemini handle this well; flag low-confidence extractions |
| Rate limiting at scale | Queue processing, limit to N imports/day per user (free tier) |

#### Dependencies

- `yt-dlp` installed on server (or containerized)
- Gemini API (already integrated)
- Existing import infrastructure (ImportJob, preview/save flow)

#### Implementation Estimate

- Backend (audio extraction + transcription): 8-12 hours
- Backend (Gemini recipe extraction): 4-6 hours
- Frontend (import UI + preview): 6-8 hours
- Tests: 4-6 hours
- **Total: 22-32 hours**

---

### Implementation Estimate (Section 5 Total)

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

**Multi-Image Import:**
- Backend: 4-6 hours
- Frontend: 6-8 hours
- Tests: 2-3 hours

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

## 7. Recipe Collections & Cookbooks

### Overview

Organize recipes into themed collections. Think "playlists for recipes" — curated groups that make sense to the user.

### User Stories

1. **Create collection** — "Italian Favorites", "Quick Weeknight Dinners", "Holiday Baking"
2. **Add recipes to collections** — One recipe can be in multiple collections
3. **Browse by collection** — Filter recipe list by collection
4. **Share collections** — Share entire collection via link (public) or to circles
5. **Featured collections** — Curated collections from RecipeVault team

### Data Model

```csharp
[Table("Collection")]
public class Collection {
    public int CollectionId { get; private set; }
    public Guid CollectionResourceId { get; private set; }
    
    public int SubjectId { get; private set; }
    
    [Required, StringLength(100)]
    public string Name { get; private set; }
    
    [StringLength(500)]
    public string Description { get; private set; }
    
    [StringLength(1000)]
    public string CoverImageUrl { get; private set; }  // Optional custom cover
    
    public bool IsPublic { get; private set; }
    public bool IsFeatured { get; private set; }  // Admin-curated
    
    public int SortOrder { get; private set; }  // User's preferred display order
    
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }
    
    public virtual Subject Subject { get; private set; }
    public virtual ICollection<CollectionRecipe> CollectionRecipes { get; private set; }
}

[Table("CollectionRecipe")]
public class CollectionRecipe {
    public int CollectionRecipeId { get; private set; }
    public int CollectionId { get; private set; }
    public int RecipeId { get; private set; }
    
    public int SortOrder { get; private set; }  // Order within collection
    public DateTime AddedDate { get; private set; }
    
    public virtual Collection Collection { get; private set; }
    public virtual Recipe Recipe { get; private set; }
}
```

### API Design

```
# Collections
POST   /api/collections                       Create collection
GET    /api/collections                       List my collections
GET    /api/collections/{id}                  Get collection with recipes
PUT    /api/collections/{id}                  Update collection
DELETE /api/collections/{id}                  Delete collection
PUT    /api/collections/reorder               Reorder my collections

# Collection recipes
POST   /api/collections/{id}/recipes          Add recipe to collection
DELETE /api/collections/{id}/recipes/{recipeId}  Remove recipe
PUT    /api/collections/{id}/recipes/reorder  Reorder recipes in collection

# Discovery
GET    /api/collections/featured              Get featured collections
GET    /api/collections/public?search=italian Search public collections

# Quick action from recipe view
POST   /api/recipes/{id}/collections          Add recipe to collection(s)
  Body: { "collectionIds": ["...", "..."] }
```

### UX Design

#### Collection List View

```
┌─────────────────────────────────────────────┐
│  📚 My Collections                   [+ New]│
├─────────────────────────────────────────────┤
│                                             │
│  ┌──────────────┐  ┌──────────────┐         │
│  │  🍝          │  │  ⏱️          │         │
│  │              │  │              │         │
│  │  Italian     │  │  Quick       │         │
│  │  Favorites   │  │  Weeknights  │         │
│  │  12 recipes  │  │  8 recipes   │         │
│  └──────────────┘  └──────────────┘         │
│                                             │
│  ┌──────────────┐  ┌──────────────┐         │
│  │  🎄          │  │  🌱          │         │
│  │              │  │              │         │
│  │  Holiday     │  │  Healthy     │         │
│  │  Baking      │  │  Eats        │         │
│  │  15 recipes  │  │  6 recipes   │         │
│  └──────────────┘  └──────────────┘         │
│                                             │
└─────────────────────────────────────────────┘
```

#### Add to Collection (from recipe)

```
┌─────────────────────────────────────────────┐
│  Add "Spaghetti Carbonara" to Collection    │
├─────────────────────────────────────────────┤
│                                             │
│  ☑ Italian Favorites                        │
│  ☐ Quick Weeknights                         │
│  ☐ Holiday Baking                           │
│  ☑ Date Night Dinners                       │
│                                             │
│  [+ Create New Collection]                  │
│                                             │
│  [Cancel]                    [Save]         │
└─────────────────────────────────────────────┘
```

### Smart Collections (Future)

Auto-generated collections based on rules:

```typescript
interface SmartCollection {
  name: string;
  rules: CollectionRule[];
  // e.g., "High Protein" = recipes where nutrition.protein > 30g
  // e.g., "Under 30 Minutes" = recipes where totalTime <= 30
  // e.g., "5-Star Recipes" = recipes where rating == 5
}
```

### Implementation Estimate

- Backend: 6-8 hours
- Frontend: 8-12 hours
- Tests: 3-4 hours
- **Total: 17-24 hours**

---

## 8. Dietary Profiles

### Overview

Users set their dietary restrictions and preferences once. The app then:
- Auto-filters recipe discovery
- Warns when recipes conflict with restrictions
- Suggests substitutions automatically
- Filters meal plan suggestions

### User Stories

1. **Set dietary profile** — "I'm lactose intolerant and avoid pork"
2. **See warnings** — Recipe detail shows ⚠️ if it conflicts with profile
3. **Auto-filter discovery** — Public recipe browse respects my restrictions
4. **Household profiles** — Set restrictions for family members
5. **Temporary modes** — "I'm doing Whole30 this month"

### Restriction Types

```
ALLERGIES (serious, always warn)
├─ Peanuts
├─ Tree Nuts
├─ Shellfish
├─ Fish
├─ Eggs
├─ Milk/Dairy
├─ Wheat/Gluten
├─ Soy
└─ Sesame

INTOLERANCES (uncomfortable, warn)
├─ Lactose
├─ Gluten sensitivity
└─ FODMAPs

DIETARY CHOICES (preference, filter)
├─ Vegetarian
├─ Vegan
├─ Pescatarian
├─ Kosher
├─ Halal
├─ Keto/Low-carb
├─ Paleo
└─ Whole30

AVOID INGREDIENTS (custom)
├─ [User-defined list]
└─ e.g., "cilantro", "blue cheese"
```

### Data Model

```csharp
[Table("DietaryProfile")]
public class DietaryProfile {
    public int DietaryProfileId { get; private set; }
    public int SubjectId { get; private set; }
    
    [StringLength(100)]
    public string ProfileName { get; private set; }  // "Me", "Kids", "Guest"
    
    public bool IsDefault { get; private set; }  // Primary profile for filtering
    
    public virtual Subject Subject { get; private set; }
    public virtual ICollection<DietaryRestriction> Restrictions { get; private set; }
    public virtual ICollection<AvoidedIngredient> AvoidedIngredients { get; private set; }
}

[Table("DietaryRestriction")]
public class DietaryRestriction {
    public int DietaryRestrictionId { get; private set; }
    public int DietaryProfileId { get; private set; }
    
    public RestrictionType Type { get; private set; }  // Allergy, Intolerance, Choice
    
    [Required, StringLength(50)]
    public string RestrictionCode { get; private set; }  // "peanuts", "vegan", etc.
    
    public RestrictionSeverity Severity { get; private set; }  // Strict, Flexible
    
    public virtual DietaryProfile DietaryProfile { get; private set; }
}

public enum RestrictionType { Allergy, Intolerance, DietaryChoice }
public enum RestrictionSeverity { Strict, Flexible }

[Table("AvoidedIngredient")]
public class AvoidedIngredient {
    public int AvoidedIngredientId { get; private set; }
    public int DietaryProfileId { get; private set; }
    
    [Required, StringLength(100)]
    public string IngredientName { get; private set; }  // Normalized
    
    [StringLength(200)]
    public string Reason { get; private set; }  // Optional: "tastes like soap"
    
    public virtual DietaryProfile DietaryProfile { get; private set; }
}

// Temporary diet programs
[Table("DietProgram")]
public class DietProgram {
    public int DietProgramId { get; private set; }
    public int SubjectId { get; private set; }
    
    [Required, StringLength(50)]
    public string ProgramCode { get; private set; }  // "whole30", "keto", etc.
    
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public virtual Subject Subject { get; private set; }
}
```

### Conflict Detection

```csharp
public class DietaryConflictService {
    // Map restrictions to ingredient patterns
    private static readonly Dictionary<string, string[]> RestrictionIngredients = new() {
        ["dairy"] = new[] { "milk", "cream", "butter", "cheese", "yogurt", "whey" },
        ["gluten"] = new[] { "flour", "wheat", "barley", "rye", "bread", "pasta" },
        ["peanuts"] = new[] { "peanut", "peanuts", "peanut butter", "peanut oil" },
        ["shellfish"] = new[] { "shrimp", "crab", "lobster", "clam", "mussel", "oyster" },
        // ... etc
    };
    
    public List<DietaryConflict> CheckRecipe(Recipe recipe, DietaryProfile profile) {
        var conflicts = new List<DietaryConflict>();
        
        foreach (var ingredient in recipe.Ingredients) {
            foreach (var restriction in profile.Restrictions) {
                if (IngredientMatchesRestriction(ingredient, restriction)) {
                    conflicts.Add(new DietaryConflict {
                        IngredientIndex = ingredient.SortOrder,
                        IngredientText = ingredient.RawText,
                        RestrictionCode = restriction.RestrictionCode,
                        Severity = restriction.Severity,
                        Type = restriction.Type
                    });
                }
            }
        }
        
        return conflicts;
    }
}
```

### API Design

```
# Profiles
GET    /api/dietary-profiles                  List my profiles
POST   /api/dietary-profiles                  Create profile
PUT    /api/dietary-profiles/{id}             Update profile
DELETE /api/dietary-profiles/{id}             Delete profile
PUT    /api/dietary-profiles/{id}/default     Set as default

# Restrictions
POST   /api/dietary-profiles/{id}/restrictions      Add restriction
DELETE /api/dietary-profiles/{id}/restrictions/{code}  Remove restriction

# Avoided ingredients
POST   /api/dietary-profiles/{id}/avoided           Add avoided ingredient
DELETE /api/dietary-profiles/{id}/avoided/{id}      Remove avoided ingredient

# Conflict check
GET    /api/recipes/{id}/dietary-check              Check recipe vs default profile
  Query: ?profileId=...  (optional, uses default if not specified)
  Returns: { conflicts: [...], canEat: boolean, warnings: [...] }

# Recipe filtering
GET    /api/recipes?respectDietary=true             Filter by dietary profile
GET    /api/recipes/public?respectDietary=true      Public browse with filtering
```

### UX Design

#### Profile Setup

```
┌─────────────────────────────────────────────────┐
│  🥗 Dietary Profile                              │
├─────────────────────────────────────────────────┤
│                                                 │
│  Profile: [Me ▼]  [+ Add Profile]               │
│                                                 │
│  ⚠️ ALLERGIES (always warn)                     │
│  ┌────────────────────────────────────────────┐ │
│  │ ☑ Peanuts    ☐ Tree Nuts    ☐ Shellfish   │ │
│  │ ☐ Fish       ☐ Eggs         ☑ Dairy       │ │
│  │ ☐ Wheat      ☐ Soy          ☐ Sesame      │ │
│  └────────────────────────────────────────────┘ │
│                                                 │
│  🌿 DIETARY CHOICES                             │
│  ┌────────────────────────────────────────────┐ │
│  │ ☐ Vegetarian  ☐ Vegan    ☐ Pescatarian    │ │
│  │ ☐ Kosher      ☐ Halal    ☐ Keto           │ │
│  └────────────────────────────────────────────┘ │
│                                                 │
│  🚫 INGREDIENTS TO AVOID                        │
│  ┌────────────────────────────────────────────┐ │
│  │ cilantro ✕    blue cheese ✕               │ │
│  │ [+ Add ingredient]                         │ │
│  └────────────────────────────────────────────┘ │
│                                                 │
│                               [Save Profile]    │
└─────────────────────────────────────────────────┘
```

#### Recipe Warning Display

```
┌─────────────────────────────────────────────────┐
│  Creamy Mushroom Pasta                          │
│                                                 │
│  ⚠️ DIETARY CONFLICT                            │
│  ┌────────────────────────────────────────────┐ │
│  │ This recipe contains:                      │ │
│  │ • Heavy cream (dairy allergy)              │ │
│  │ • Parmesan cheese (dairy allergy)          │ │
│  │                                            │ │
│  │ [Find Substitutions]  [Cook Anyway]        │ │
│  └────────────────────────────────────────────┘ │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Implementation Estimate

- Backend: 10-14 hours
- Frontend: 10-14 hours
- Tests: 4-6 hours
- **Total: 24-34 hours**

---

## 9. Cooking History & Stats

### Overview

Track cooking activity to provide insights and encourage engagement.

### User Stories

1. **Log when I cook** — Quick "I made this" button
2. **View cooking history** — Calendar of what I've cooked
3. **See stats** — Most cooked recipes, monthly activity, streaks
4. **Personal notes per cook** — "Used less salt", "Kids loved it"
5. **Photo journal** — Attach photos of dishes I've made
6. **Running average rating** — Personal rating averages across all cooks of a recipe
7. **Smart suggestions** — Show previous notes when cooking again ("Last time: add extra cinnamon")
8. **Photo prompts** — If no hero image exists, suggest adding one during logging

### Data Model

```csharp
[Table("CookingLog")]
public class CookingLog {
    public int CookingLogId { get; private set; }
    public Guid CookingLogResourceId { get; private set; }
    
    public int RecipeId { get; private set; }
    public int SubjectId { get; private set; }
    
    public DateTime CookedDate { get; private set; }
    
    public decimal? ScaleFactor { get; private set; }  // If they scaled the recipe
    public int? ServingsMade { get; private set; }
    
    [StringLength(1000)]
    public string Notes { get; private set; }  // Personal notes for this cook
    
    public int? Rating { get; private set; }  // How did it turn out? 1-5
    
    public virtual Recipe Recipe { get; private set; }
    public virtual Subject Subject { get; private set; }
    public virtual ICollection<CookingLogPhoto> Photos { get; private set; }
}

[Table("CookingLogPhoto")]
public class CookingLogPhoto {
    public int CookingLogPhotoId { get; private set; }
    public int CookingLogId { get; private set; }
    
    [Required, StringLength(1000)]
    public string ImageUrl { get; private set; }
    
    [StringLength(200)]
    public string Caption { get; private set; }
    
    public int SortOrder { get; private set; }
    
    public virtual CookingLog CookingLog { get; private set; }
}
```

### Stats Calculations

```csharp
public class CookingStatsDto {
    // All-time
    public int TotalCooks { get; set; }
    public int UniqueRecipes { get; set; }
    public DateTime? FirstCookDate { get; set; }
    
    // Period stats
    public int CooksThisMonth { get; set; }
    public int CooksThisYear { get; set; }
    
    // Streaks
    public int CurrentStreak { get; set; }  // Days in a row with a cook
    public int LongestStreak { get; set; }
    
    // Top recipes
    public List<RecipeCountDto> MostCooked { get; set; }  // Top 10
    public List<RecipeCountDto> RecentFavorites { get; set; }  // Most cooked last 30 days
    
    // Insights
    public string MostActiveDayOfWeek { get; set; }  // "Sunday"
    public decimal AverageCooksPerWeek { get; set; }
}
```

### API Design

```
# Cooking log
POST   /api/cooking-log                       Log a cook
GET    /api/cooking-log                       List cooking history
GET    /api/cooking-log/{id}                  Get specific log entry
PUT    /api/cooking-log/{id}                  Update log entry
DELETE /api/cooking-log/{id}                  Delete log entry

# Photos
POST   /api/cooking-log/{id}/photos           Add photo
DELETE /api/cooking-log/{id}/photos/{photoId} Remove photo

# Stats
GET    /api/cooking-log/stats                 Get cooking stats
GET    /api/cooking-log/calendar?year=2026&month=2  Calendar view

# Recipe-specific
GET    /api/recipes/{id}/cooking-log          History for specific recipe
```

### UX Design

#### Quick Log Button (on recipe)

```
┌─────────────────────────────────────────────┐
│  Mom's Apple Pie                            │
│                                             │
│  [Edit] [Share] [🍳 I Made This]            │
└─────────────────────────────────────────────┘
```

Clicking opens:

```
┌─────────────────────────────────────────────┐
│  Log Your Cook                              │
├─────────────────────────────────────────────┤
│                                             │
│  When: [Today ▼]                            │
│                                             │
│  Servings made: [4____]                     │
│                                             │
│  How did it turn out? (optional)            │
│  ☆ ☆ ☆ ☆ ☆                                  │
│  Your average: ★★★★☆ (4.2 across 5 cooks)   │
│                                             │
│  💡 Last time you noted:                    │
│  "Add extra cinnamon, kids loved it"        │
│                                             │
│  Notes (optional):                          │
│  [_______________________________________]  │
│  [_______________________________________]  │
│                                             │
│  📷 Add photos                              │
│  [Choose Files]                             │
│                                             │
│  ⚠️ This recipe has no hero image.          │
│  [📸 Add a photo as hero image]             │
│                                             │
│  [Cancel]                      [Log Cook]   │
└─────────────────────────────────────────────┘
```

#### Running Average Rating

Each recipe tracks the user's personal average rating across all cooks:

```csharp
// In RecipeDto or separate endpoint
public class RecipePersonalStatsDto {
    public int CookCount { get; set; }
    public decimal? AverageRating { get; set; }  // Average of all logged ratings
    public DateTime? LastCooked { get; set; }
    public string LastNote { get; set; }  // Most recent note for suggestions
}
```

Display on recipe detail:
```
┌─────────────────────────────────────────────┐
│  Mom's Apple Pie                            │
│  ★★★★☆ 4.2 average (5 cooks)               │
│  Last made: Feb 15, 2026                    │
└─────────────────────────────────────────────┘
```

#### Hero Image Prompt

When logging a cook for a recipe without a hero image:

1. Prompt user to optionally add a photo
2. If they add one, offer to set it as the recipe's hero image
3. Great for imported recipes that came from text/URL without images

#### Cooking Stats Dashboard

```
┌─────────────────────────────────────────────────────┐
│  📊 Your Cooking Stats                              │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │     47      │  │     23      │  │     12      │  │
│  │  Total      │  │  Unique     │  │  Current    │  │
│  │  Cooks      │  │  Recipes    │  │  Streak 🔥  │  │
│  └─────────────┘  └─────────────┘  └─────────────┘  │
│                                                     │
│  MOST COOKED                                        │
│  1. 🥧 Mom's Apple Pie (8 times)                    │
│  2. 🍝 Spaghetti Carbonara (6 times)                │
│  3. 🌮 Chicken Tacos (5 times)                      │
│                                                     │
│  ACTIVITY (Last 6 Months)                           │
│  ┌───────────────────────────────────────────────┐  │
│  │  █ █   █ █ █ █   █ █ █   █ █ █ █ █   █ █     │  │
│  │  Sep   Oct       Nov     Dec         Jan Feb │  │
│  └───────────────────────────────────────────────┘  │
│                                                     │
│  💡 You cook most on Sundays!                       │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Implementation Estimate

- Backend: 8-12 hours
- Frontend: 12-16 hours
- Tests: 4-6 hours
- **Total: 24-34 hours**

---

## 10. AI Recipe Generation

### Overview

Generate new recipes from natural language descriptions using Gemini.

### User Stories

1. **Describe what I want** — "A creamy pasta with mushrooms and bacon"
2. **Generate variations** — "Give me 3 different versions"
3. **Specify constraints** — "Make it keto-friendly" or "Under 30 minutes"
4. **Refine results** — "Make it spicier" or "Use chicken instead"
5. **Save generated recipe** — Save to my library with AI attribution

### Generation Modes

```
MODE 1: Free-form description
"A cozy soup for a cold day with whatever vegetables are in season"

MODE 2: Ingredient-driven
"I have chicken thighs, coconut milk, and curry paste"

MODE 3: Style/cuisine
"Thai-inspired appetizer for a dinner party"

MODE 4: Constraint-based
"High protein breakfast under 400 calories that I can meal prep"

MODE 5: Remix existing
"Make my grandma's meatloaf recipe but vegetarian"
```

### Prompt Engineering

```
System: You are a professional chef creating recipes. Generate detailed, 
tested recipes with accurate measurements and clear instructions.

User request: {user_description}

Constraints:
- Dietary: {from dietary profile}
- Time: {if specified}
- Skill level: {if specified}
- Equipment: {from equipment profile}

Generate a complete recipe including:
1. Creative but descriptive title
2. Brief description (2-3 sentences)
3. Prep time and cook time
4. Yield/servings
5. Complete ingredient list with precise measurements
6. Step-by-step instructions
7. Tips and variations
8. Suggested tags (dietary, cuisine, meal type)

Format as JSON matching RecipeVault schema.
```

### API Design

```
# Generate recipe
POST /api/recipes/generate
  Body: {
    "prompt": "creamy mushroom pasta with a kick",
    "constraints": {
      "maxTime": 30,
      "dietary": ["vegetarian"],
      "skillLevel": "beginner"
    },
    "variations": 1  // Number of variations to generate (1-3)
  }
  Returns: Generated recipe(s) for preview

# Refine generated recipe
POST /api/recipes/generate/refine
  Body: {
    "previousRecipe": { ... },
    "refinement": "make it spicier and add a garnish suggestion"
  }
  Returns: Refined recipe

# Save generated recipe
POST /api/recipes/generate/save
  Body: { generated recipe + any user edits }
  Returns: Created RecipeDto
```

### UX Design

#### Generation Interface

```
┌─────────────────────────────────────────────────────┐
│  ✨ Create a Recipe with AI                         │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Describe what you want to make:                    │
│  ┌───────────────────────────────────────────────┐  │
│  │ A creamy pasta dish with mushrooms and bacon  │  │
│  │ that's impressive enough for date night       │  │
│  └───────────────────────────────────────────────┘  │
│                                                     │
│  Constraints (optional):                            │
│  ⏱️ Max time: [Any ▼]                               │
│  👨‍🍳 Skill level: [Any ▼]                            │
│  🥗 Respect my dietary profile: ☑                   │
│                                                     │
│  [Generate Recipe]                                  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

#### Preview Generated Recipe

```
┌─────────────────────────────────────────────────────┐
│  ✨ AI Generated Recipe                             │
├─────────────────────────────────────────────────────┤
│                                                     │
│  CREAMY BACON MUSHROOM FETTUCCINE                   │
│                                                     │
│  Rich, restaurant-quality pasta with crispy bacon,  │
│  sautéed cremini mushrooms, and a silky parmesan    │
│  cream sauce. Perfect for impressing your date.     │
│                                                     │
│  ⏱️ 25 min  |  🍽️ Serves 2  |  👨‍🍳 Intermediate     │
│                                                     │
│  INGREDIENTS                                        │
│  • 8 oz fettuccine                                  │
│  • 4 strips thick-cut bacon, diced                  │
│  • 8 oz cremini mushrooms, sliced                   │
│  • 3 cloves garlic, minced                          │
│  • 1 cup heavy cream                                │
│  • ½ cup parmesan, freshly grated                   │
│  • Fresh thyme, salt, pepper                        │
│                                                     │
│  INSTRUCTIONS                                       │
│  1. Cook pasta according to package...              │
│  2. While pasta cooks, crisp bacon in...            │
│  ...                                                │
│                                                     │
│  ────────────────────────────────────────────────   │
│                                                     │
│  Not quite right?                                   │
│  [🔄 Regenerate] [✏️ Refine: "make it lighter"]     │
│                                                     │
│  [Discard]                    [Save to My Recipes]  │
└─────────────────────────────────────────────────────┘
```

### Cost Management

Gemini API costs per generation. Mitigations:

1. **Rate limiting** — Max 10 generations per day per user (free tier)
2. **Caching** — Cache popular prompts
3. **Premium feature** — Unlimited generation for paid users
4. **Prompt optimization** — Minimize token usage

### Implementation Estimate

- Backend: 8-12 hours
- Frontend: 10-14 hours
- Tests: 4-6 hours
- **Total: 22-32 hours**

---

## 11. Kitchen Equipment Filtering

### Overview

Track what equipment users have and filter recipes accordingly.

### User Stories

1. **Set my equipment** — "I have an Instant Pot, air fryer, and stand mixer"
2. **Filter recipes** — Show only recipes I can make with my equipment
3. **See what I'm missing** — "This recipe needs a food processor"
4. **Equipment suggestions** — "Buy a Dutch oven to unlock 15 recipes"

### Common Equipment

```
APPLIANCES
├─ Instant Pot / Pressure Cooker
├─ Slow Cooker / Crock-Pot
├─ Air Fryer
├─ Stand Mixer
├─ Food Processor
├─ Blender (regular)
├─ Immersion Blender
├─ Bread Machine
├─ Sous Vide
├─ Deep Fryer
├─ Rice Cooker
├─ Waffle Iron
└─ Ice Cream Maker

COOKWARE
├─ Dutch Oven
├─ Cast Iron Skillet
├─ Wok
├─ Grill / Grill Pan
├─ Pizza Stone
├─ Roasting Pan
├─ Steamer Basket
└─ Double Boiler

BAKEWARE
├─ Bundt Pan
├─ Springform Pan
├─ Tart Pan
├─ Pie Dish
├─ Ramekins
├─ Muffin Tin
└─ Loaf Pan

TOOLS
├─ Mandoline
├─ Kitchen Scale
├─ Meat Thermometer
├─ Piping Bags
├─ Torch (for crème brûlée)
└─ Mortar & Pestle
```

### Data Model

```csharp
[Table("Equipment")]
public class Equipment {
    public int EquipmentId { get; private set; }
    
    [Required, StringLength(100)]
    public string Name { get; private set; }
    
    [Required, StringLength(100)]
    public string Code { get; private set; }  // "instant-pot", "air-fryer"
    
    public EquipmentCategory Category { get; private set; }
    
    [StringLength(500)]
    public string Description { get; private set; }
    
    public bool IsCommon { get; private set; }  // Assume most users have it (pots, pans)
}

public enum EquipmentCategory { Appliance, Cookware, Bakeware, Tool }

[Table("UserEquipment")]
public class UserEquipment {
    public int UserEquipmentId { get; private set; }
    public int SubjectId { get; private set; }
    public int EquipmentId { get; private set; }
    
    public DateTime AddedDate { get; private set; }
    
    public virtual Subject Subject { get; private set; }
    public virtual Equipment Equipment { get; private set; }
}

[Table("RecipeEquipment")]
public class RecipeEquipment {
    public int RecipeEquipmentId { get; private set; }
    public int RecipeId { get; private set; }
    public int EquipmentId { get; private set; }
    
    public bool IsRequired { get; private set; }  // vs "helpful but optional"
    
    public virtual Recipe Recipe { get; private set; }
    public virtual Equipment Equipment { get; private set; }
}
```

### Equipment Detection

Auto-detect equipment from recipe instructions:

```csharp
public class EquipmentDetector {
    private static readonly Dictionary<string, string[]> EquipmentPatterns = new() {
        ["instant-pot"] = new[] { "instant pot", "pressure cooker", "pressure cook" },
        ["air-fryer"] = new[] { "air fryer", "air fry", "air-fry" },
        ["slow-cooker"] = new[] { "slow cooker", "crock pot", "crockpot" },
        ["stand-mixer"] = new[] { "stand mixer", "kitchenaid", "fitted with paddle" },
        ["food-processor"] = new[] { "food processor", "pulse until" },
        ["dutch-oven"] = new[] { "dutch oven" },
        ["cast-iron"] = new[] { "cast iron", "cast-iron skillet" },
        // ... etc
    };
    
    public List<string> DetectEquipment(Recipe recipe) {
        var text = string.Join(" ", recipe.Instructions.Select(i => i.Instruction)).ToLower();
        return EquipmentPatterns
            .Where(kv => kv.Value.Any(pattern => text.Contains(pattern)))
            .Select(kv => kv.Key)
            .ToList();
    }
}
```

### API Design

```
# User equipment
GET    /api/equipment                         List all equipment options
GET    /api/my-equipment                      List my equipment
POST   /api/my-equipment                      Add equipment
DELETE /api/my-equipment/{code}               Remove equipment

# Recipe equipment
GET    /api/recipes/{id}/equipment            Get equipment for recipe
POST   /api/recipes/{id}/equipment/detect     Auto-detect from instructions
PUT    /api/recipes/{id}/equipment            Set equipment (manual override)

# Filtering
GET    /api/recipes?hasEquipment=true         Filter by my equipment
GET    /api/recipes/{id}/equipment-check      Check if I can make this
```

### UX Design

#### Equipment Setup (Quick Select)

Scrollable checklist sorted by frequency in global recipe database. Most-used equipment appears first:

```
┌─────────────────────────────────────────────────────┐
│  🍳 My Kitchen Equipment                            │
│  Check the equipment you have                       │
├─────────────────────────────────────────────────────┤
│                                                     │
│  APPLIANCES (sorted by recipe frequency)            │
│  ┌────────────────────────────────────────────────┐ │
│  │ ☑ Blender (847 recipes)                        │ │
│  │ ☑ Food Processor (612 recipes)                 │ │
│  │ ☑ Stand Mixer (534 recipes)                    │ │
│  │ ☑ Instant Pot (423 recipes)                    │ │
│  │ ☑ Air Fryer (389 recipes)                      │ │
│  │ ☑ Slow Cooker (356 recipes)                    │ │
│  │ ☐ Sous Vide (89 recipes)                       │ │
│  │ ☐ Bread Machine (67 recipes)                   │ │
│  │ ☐ Deep Fryer (45 recipes)                      │ │
│  │ ☐ Ice Cream Maker (34 recipes)                 │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  COOKWARE                                           │
│  ┌────────────────────────────────────────────────┐ │
│  │ ☑ Cast Iron Skillet (723 recipes)              │ │
│  │ ☑ Dutch Oven (456 recipes)                     │ │
│  │ ☐ Wok (234 recipes)                            │ │
│  │ ☐ Grill/Grill Pan (198 recipes)                │ │
│  │ ☐ Pizza Stone (67 recipes)                     │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  BAKEWARE                                           │
│  ┌────────────────────────────────────────────────┐ │
│  │ ☑ Muffin Tin (312 recipes)                     │ │
│  │ ☑ Pie Dish (234 recipes)                       │ │
│  │ ☐ Bundt Pan (145 recipes)                      │ │
│  │ ☐ Springform Pan (123 recipes)                 │ │
│  │ ☐ Tart Pan (89 recipes)                        │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  📊 Sorted by usage in RecipeVault recipes         │
│                                                     │
│                                    [Save Equipment] │
└─────────────────────────────────────────────────────┘
```

#### Recipe Filter Toggle

```
┌─────────────────────────────────────────────┐
│  [My Recipes ▼]                             │
│                                             │
│  Filters:                                   │
│  ☑ Only recipes I have equipment for        │
│  ☐ Under 30 minutes                         │
│  ☐ Vegetarian                               │
└─────────────────────────────────────────────┘
```

### Implementation Estimate

- Backend: 8-10 hours
- Frontend: 8-10 hours
- Tests: 3-4 hours
- **Total: 19-24 hours**

---

## 12. Grocery Delivery Integration

### Overview

Send ingredient lists directly to grocery delivery services (Instacart, Walmart, Amazon Fresh).

### User Stories

1. **Add to cart** — One-click add all recipe ingredients to Instacart
2. **Meal plan shopping** — Add entire meal plan's grocery list to cart
3. **Smart matching** — Match ingredients to actual products
4. **Store selection** — Choose preferred store/service
5. **Price comparison** — See prices across services (future)

### Integration Options

#### Option A: Instacart Partner API

- Official API for partners
- Requires business agreement
- Best UX (deep integration)
- Revenue share model

#### Option B: Universal Affiliate Links

- Use Instacart's public "add to cart" URL scheme
- No API agreement needed
- Less seamless (opens Instacart website)
- Can include affiliate tracking

```
https://www.instacart.com/store/recipes?ingredients=chicken+breast,olive+oil,garlic
```

#### Option C: Multiple Service Links

Provide links to multiple services:
- Instacart
- Walmart Grocery
- Amazon Fresh
- Shipt

User chooses their preferred service.

### Data Model

```csharp
[Table("UserGroceryPreference")]
public class UserGroceryPreference {
    public int UserGroceryPreferenceId { get; private set; }
    public int SubjectId { get; private set; }
    
    public GroceryService PreferredService { get; private set; }
    
    [StringLength(100)]
    public string PreferredStore { get; private set; }  // "Costco", "Whole Foods"
    
    [StringLength(20)]
    public string ZipCode { get; private set; }
    
    public virtual Subject Subject { get; private set; }
}

public enum GroceryService { Instacart, Walmart, AmazonFresh, Shipt, Manual }
```

### API Design

```
# Preferences
GET    /api/grocery/preferences               Get my preferences
PUT    /api/grocery/preferences               Update preferences

# Cart building
POST   /api/grocery/cart/from-recipe/{id}     Build cart from recipe
POST   /api/grocery/cart/from-mealplan/{id}   Build cart from meal plan
POST   /api/grocery/cart/from-list            Build cart from grocery list

# Service links
GET    /api/grocery/checkout-url              Get checkout URL for service
  Query: ?service=instacart&items=...
  Returns: { url: "https://instacart.com/..." }
```

### UX Design

#### On Recipe View

```
┌─────────────────────────────────────────────────────┐
│  INGREDIENTS                           [🛒 Shop]    │
│                                                     │
│  • 2 lbs chicken breast                             │
│  • 1 tbsp olive oil                                 │
│  • 4 cloves garlic                                  │
│  • ...                                              │
└─────────────────────────────────────────────────────┘
```

Clicking Shop:

```
┌─────────────────────────────────────────────────────┐
│  Shop for Ingredients                               │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Add all ingredients to:                            │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  🥕 Instacart                                │    │
│  │     Opens Instacart with ingredients added  │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  🟦 Walmart Grocery                          │    │
│  │     Opens Walmart with ingredients          │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  📦 Amazon Fresh                             │    │
│  │     Opens Amazon with ingredients           │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  [Set Default Service]                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Ingredient Normalization

Challenge: Recipe says "2 lbs chicken breast" but Instacart needs product search.

```csharp
public class GroceryItemMapper {
    public string NormalizeForSearch(string ingredientText) {
        // "2 lbs boneless skinless chicken breast, cut into cubes"
        // → "chicken breast boneless skinless"
        
        // Remove quantities
        // Remove preparations (diced, chopped, etc.)
        // Keep key descriptors (boneless, organic, etc.)
        
        return normalizedSearchTerm;
    }
}
```

### Monetization

- Instacart affiliate program: commission on orders
- Potential for Instacart partnership (better integration, higher commission)
- Premium feature: price comparison across services

### Implementation Estimate

- Backend: 6-8 hours
- Frontend: 6-8 hours
- Tests: 2-4 hours
- **Total: 14-20 hours**

---

## 13. Recipe Linking (Component Recipes)

### Overview

Link recipes together as components. A teriyaki sauce recipe can be linked as an ingredient in multiple main dishes. Think of it as recipe composition — building blocks that can be reused.

### User Stories

1. **Link recipe as ingredient** — "Teriyaki Sauce (see my recipe)" as an ingredient
2. **Navigate between recipes** — Click linked ingredient to view component recipe
3. **Combine times** — Total time includes component prep (optional)
4. **Inline expansion** — Optionally expand component recipe inline
5. **Track dependencies** — Know which recipes use a component

### Use Cases

```
SAUCES & MARINADES
├─ Custom teriyaki sauce → used in 5 stir-fry recipes
├─ Pizza sauce → used in pizza, calzones, dipping
└─ Salad dressing → used in multiple salads

BASICS & BUILDING BLOCKS
├─ Pie crust → used in apple pie, quiche, pot pie
├─ Roux → used in mac and cheese, gravy, gumbo
└─ Pizza dough → used in pizza, breadsticks, calzones

PREP COMPONENTS
├─ Caramelized onions → burgers, french onion soup, flatbread
├─ Roasted garlic → dozens of recipes
└─ Pickled red onions → tacos, sandwiches, salads
```

### Data Model

```csharp
[Table("RecipeLink")]
public class RecipeLink {
    public int RecipeLinkId { get; private set; }
    
    public int ParentRecipeId { get; private set; }  // The main recipe
    public int LinkedRecipeId { get; private set; }  // The component recipe
    
    public int IngredientIndex { get; private set; }  // Which ingredient it replaces/augments
    
    [StringLength(200)]
    public string DisplayText { get; private set; }  // "My Teriyaki Sauce" or custom
    
    public bool IncludeInTotalTime { get; private set; }  // Add component time to parent
    public decimal? PortionUsed { get; private set; }  // Use 1/2 batch of sauce
    
    public virtual Recipe ParentRecipe { get; private set; }
    public virtual Recipe LinkedRecipe { get; private set; }
}
```

### Ingredient Display

When an ingredient is linked to another recipe:

```
INGREDIENTS

• 2 lbs chicken thighs, cubed
• 1 cup Teriyaki Sauce 🔗           ← Link indicator
• 2 cups jasmine rice
• 1 bunch green onions, sliced
```

Clicking the 🔗 opens a popover or navigates to the linked recipe.

### API Design

```
# Links
POST   /api/recipes/{id}/links                Create link to component recipe
GET    /api/recipes/{id}/links                Get linked recipes (components)
DELETE /api/recipes/{id}/links/{linkId}       Remove link
PUT    /api/recipes/{id}/links/{linkId}       Update link (portion, display text)

# Reverse lookup
GET    /api/recipes/{id}/used-in              Recipes that use this as a component

# Search for linkable recipes
GET    /api/recipes/linkable?query=sauce      Search my recipes for linking
```

### UX Design

#### Adding a Link (in recipe edit)

```
┌─────────────────────────────────────────────────────┐
│  Edit Ingredient                                    │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Ingredient: [1 cup teriyaki sauce_______________]  │
│                                                     │
│  🔗 Link to another recipe?                         │
│  ┌────────────────────────────────────────────────┐ │
│  │ Search my recipes...                           │ │
│  │                                                │ │
│  │ 🍶 My Teriyaki Sauce                           │ │
│  │ 🥫 Quick Teriyaki Glaze                        │ │
│  │ 🍯 Honey Teriyaki Marinade                     │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  Options:                                           │
│  ☑ Include component prep time in total            │
│  Portion: [Full batch ▼]                           │
│                                                     │
│  [Cancel]                         [Link Recipe]    │
└─────────────────────────────────────────────────────┘
```

#### Recipe View with Linked Ingredient

```
┌─────────────────────────────────────────────────────┐
│  Teriyaki Chicken                                   │
│  ⏱️ 25 min (+ 15 min for sauce)                     │
├─────────────────────────────────────────────────────┤
│                                                     │
│  INGREDIENTS                                        │
│                                                     │
│  ○ 2 lbs chicken thighs                            │
│  ○ 1 cup My Teriyaki Sauce 🔗                      │
│    └─ [View Recipe] [Expand Here]                  │
│  ○ 2 cups jasmine rice                             │
│                                                     │
└─────────────────────────────────────────────────────┘
```

#### "Used In" Display (on component recipe)

```
┌─────────────────────────────────────────────────────┐
│  My Teriyaki Sauce                                  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  📌 Used in 5 recipes:                              │
│  • Teriyaki Chicken                                │
│  • Beef Stir Fry                                   │
│  • Teriyaki Salmon                                 │
│  • + 2 more                                        │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Grocery List Integration

When generating grocery lists:
- Detect linked recipes
- Include component ingredients
- Adjust quantities based on portion used
- De-duplicate across multiple links

### Implementation Estimate

- Backend: 8-12 hours
- Frontend: 10-14 hours
- Tests: 4-6 hours
- **Total: 22-32 hours**

---

## 14. Recipe Mixing (AI Fusion)

### Overview

AI-powered recipe fusion that combines elements from two recipes based on user intent. Take the best of both worlds.

### User Stories

1. **Mix two recipes** — Combine Recipe A and Recipe B into something new
2. **Specify intent** — "I like the sauce from A but the protein prep from B"
3. **Surprise me mode** — Let AI find interesting combinations
4. **Review and edit** — Preview the fusion before saving
5. **Save as fork** — Mixed recipe links back to both parents

### Mix Modes

```
MODE 1: GUIDED MIXING
User specifies what they want from each recipe:
"Take the spice blend from Recipe A, but use the cooking method from Recipe B"
"Keep A's ingredients but follow B's technique"
"Combine A's sauce with B's protein"

MODE 2: SURPRISE ME
AI analyzes both recipes and creates an interesting fusion:
- Identifies complementary elements
- Balances flavors and techniques
- Creates something novel but coherent

MODE 3: BEST OF BOTH
AI picks the "best" elements from each:
- Higher-rated components (if rating data exists)
- More detailed instructions
- Better technique descriptions
```

### Data Model

```csharp
// Mixed recipes store their lineage
public class Recipe {
    // Existing fields...
    
    // For mixed recipes (extends forking)
    public int? MixedFromRecipeAId { get; private set; }
    public int? MixedFromRecipeBId { get; private set; }
    
    [StringLength(500)]
    public string MixIntent { get; private set; }  // User's mixing instructions
}
```

### Gemini Prompt Structure

```
You are a professional chef creating fusion recipes.

RECIPE A: "{title_a}"
{full recipe A details}

RECIPE B: "{title_b}"
{full recipe B details}

USER INTENT: "{user_intent}"
(or "Create an interesting fusion that combines the best of both recipes")

Create a new recipe that:
1. Honors the user's intent (if specified)
2. Results in a coherent, cookable dish
3. Balances flavors and techniques from both sources
4. Includes clear attribution of which elements came from which recipe

Format as JSON with:
- title (creative fusion name)
- description (explaining the fusion)
- ingredients (with notes on origin: "from A", "from B", "combined")
- instructions
- mixNotes (explaining what was combined and why)
```

### API Design

```
# Mix recipes
POST /api/recipes/mix
  Body: {
    "recipeAId": "...",
    "recipeBId": "...",
    "intent": "Take the marinade from A but cook it like B",
    // OR
    "mode": "surprise"  // Let AI decide
  }
  Returns: Preview of mixed recipe

# Refine mix
POST /api/recipes/mix/refine
  Body: {
    "mixedRecipe": { ... },
    "refinement": "Make it spicier"
  }
  Returns: Refined mixed recipe

# Save mixed recipe
POST /api/recipes/mix/save
  Body: { mixed recipe }
  Returns: Created RecipeDto with dual parentage
```

### UX Design

#### Initiate Mix

From recipe view, "Mix with..." option:

```
┌─────────────────────────────────────────────────────┐
│  Grandma's Meatloaf                                 │
│                                                     │
│  [Edit] [Fork] [🔀 Mix with...]                     │
└─────────────────────────────────────────────────────┘
```

#### Select Second Recipe

```
┌─────────────────────────────────────────────────────┐
│  Mix "Grandma's Meatloaf" with...                   │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Search: [korean bbq_________________________]      │
│                                                     │
│  ┌────────────────────────────────────────────────┐ │
│  │ 🍖 Korean BBQ Beef                             │ │
│  │    Gochujang glaze, sesame, green onions      │ │
│  │    [Select]                                   │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
│  ┌────────────────────────────────────────────────┐ │
│  │ 🥩 Korean Bulgogi                              │ │
│  │    Soy-marinated beef, pear, garlic           │ │
│  │    [Select]                                   │ │
│  └────────────────────────────────────────────────┘ │
│                                                     │
└─────────────────────────────────────────────────────┘
```

#### Specify Intent

```
┌─────────────────────────────────────────────────────┐
│  Mix Recipes                                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Recipe A: Grandma's Meatloaf                       │
│  Recipe B: Korean BBQ Beef                          │
│                                                     │
│  How do you want to mix them?                       │
│                                                     │
│  ○ 🎯 GUIDED                                        │
│    Tell us what you want from each recipe:          │
│    ┌──────────────────────────────────────────────┐ │
│    │ Use the meatloaf base from A but add the    │ │
│    │ Korean BBQ glaze and flavors from B         │ │
│    └──────────────────────────────────────────────┘ │
│                                                     │
│  ○ 🎲 SURPRISE ME                                   │
│    Let AI create an interesting fusion              │
│                                                     │
│  ○ ⭐ BEST OF BOTH                                  │
│    AI picks the strongest elements from each        │
│                                                     │
│  [Cancel]                          [Create Mix]     │
└─────────────────────────────────────────────────────┘
```

#### Preview Mixed Recipe

```
┌─────────────────────────────────────────────────────┐
│  ✨ AI Mixed Recipe                                 │
├─────────────────────────────────────────────────────┤
│                                                     │
│  KOREAN BBQ MEATLOAF                                │
│                                                     │
│  A fusion of comfort food and Korean flavors —      │
│  classic meatloaf glazed with gochujang and         │
│  topped with sesame and green onions.               │
│                                                     │
│  🔗 Mixed from:                                     │
│  • Grandma's Meatloaf (base, technique)            │
│  • Korean BBQ Beef (glaze, toppings)               │
│                                                     │
│  INGREDIENTS                                        │
│  • 1.5 lbs ground beef (from A)                    │
│  • 1/2 cup breadcrumbs (from A)                    │
│  • 3 tbsp gochujang (from B)                       │
│  • 2 tbsp soy sauce (from B)                       │
│  • 1 tbsp sesame oil (from B)                      │
│  • ... (combined)                                   │
│                                                     │
│  INSTRUCTIONS                                       │
│  1. Preheat oven to 350°F (from A)                 │
│  2. Mix beef with Korean seasonings...              │
│  ...                                                │
│                                                     │
│  ────────────────────────────────────────────────   │
│                                                     │
│  Not quite right?                                   │
│  [🔄 Regenerate] [✏️ Refine: "less spicy"]          │
│                                                     │
│  [Discard]                   [Save to My Recipes]   │
└─────────────────────────────────────────────────────┘
```

### Attribution Display

On saved mixed recipes, show dual lineage:

```
┌─────────────────────────────────────────────────────┐
│  Korean BBQ Meatloaf                                │
│  ↳ Mixed from "Grandma's Meatloaf" + "Korean BBQ"  │
└─────────────────────────────────────────────────────┘
```

### Implementation Estimate

- Backend: 10-14 hours
- Frontend: 12-16 hours
- Tests: 4-6 hours
- **Total: 26-36 hours**

---

## Summary: Total Estimates

### Core Features

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

**Core Features Subtotal: 232-319 hours**

### Additional Features

| Feature | Backend | Frontend | Tests | Total |
|---------|---------|----------|-------|-------|
| Collections/Cookbooks | 6-8h | 8-12h | 3-4h | 17-24h |
| Dietary Profiles | 10-14h | 10-14h | 4-6h | 24-34h |
| Cooking History & Stats | 8-12h | 12-16h | 4-6h | 24-34h |
| AI Recipe Generation | 8-12h | 10-14h | 4-6h | 22-32h |
| Kitchen Equipment | 8-10h | 8-10h | 3-4h | 19-24h |
| Grocery Delivery | 6-8h | 6-8h | 2-4h | 14-20h |
| Recipe Linking | 8-12h | 10-14h | 4-6h | 22-32h |
| Recipe Mixing | 10-14h | 12-16h | 4-6h | 26-36h |

**Additional Features Subtotal: 168-236 hours**

---

**Grand Total: 400-555 hours** (~10-14 weeks of focused development)

---

## Dependencies

```
Recipe Forking
     │
     ├──► Smart Substitutions (requires fork to save)
     │
     ├──► Social Circles (fork shared recipes)
     │
     ├──► AI Recipe Generation (save as fork)
     │
     └──► Recipe Mixing (mixed recipes extend forking)

Recipe Linking ──► Grocery List (include component ingredients)
     │
     └──► Nutrition (aggregate linked recipe nutrients)

Pantry ◄── Shopping Suggestions
  │
  ├──► "What Can I Make?" filter
  │
  └──► Grocery Delivery (shop for missing items)

Browser Extension ──► Batch Import (uses same parsing)

Cooking Mode ◄── Voice Control (optional enhancement)
     │
     └──► Cooking History (auto-log when completing cook)

Cooking History ──► Recipe hero images (prompt to add photo)

Dietary Profiles
     │
     ├──► Recipe filtering (everywhere)
     │
     ├──► Smart Substitutions (auto-suggest for restrictions)
     │
     ├──► AI Recipe Generation (respect constraints)
     │
     └──► Recipe Mixing (respect constraints in fusion)

Collections ──► Social Circles (share collections to circles)

Kitchen Equipment ──► Recipe filtering
     │
     ├──► AI Recipe Generation (respect equipment constraints)
     │
     └──► Recipe Mixing (respect equipment in fusion)
```

---

*This document is the source of truth for feature planning. Update as designs evolve.*
