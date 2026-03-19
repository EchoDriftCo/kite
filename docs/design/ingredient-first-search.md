# Ingredient-First Search: "What's in Your Fridge?"

## Overview

Enable users to search recipes by available ingredients — a reverse-lookup feature that answers "what can I make with what I have?" The system scores recipes at the database level using PostgreSQL functions, supports intelligent word-boundary matching (avoiding false positives like "rice" matching "licorice"), maintains user-specific pantry inventories with configurable staples, weights ingredients by importance (proteins > garnishes), and integrates with dietary profiles and the substitution engine.

### Core Features

1. **Database-level scoring** — PostgreSQL function computes match scores, avoiding in-memory recipe loading
2. **Smart word matching** — Word-boundary tokenization prevents substring false positives
3. **Persistent user pantry** — Users maintain ingredient inventories that persist across sessions
4. **Configurable pantry staples** — Per-user pantry staples seeded from global defaults, fully customizable
5. **Ingredient importance weighting** — Primary ingredients (proteins, starches) weighted 2x, garnishes/seasonings 0.5x
6. **Expanded synonym system** — 50+ synonym mappings (regional terms, plural/singular, common variations)
7. **Client-side autocomplete caching** — 5-minute TTL cache reduces API calls
8. **Graceful pg_trgm fallback** — Works with or without the extension, adjusting fuzzy matching accordingly

### Use Cases

1. **Minimize waste** — User has ingredients nearing expiration. Find recipes using them.
2. **Reduce shopping** — Filter to recipes missing ≤2 ingredients.
3. **Dietary-aware search** — Combine with dietary profiles to exclude allergens.
4. **Substitution-aware results** — Recipe calls for "heavy cream"; user has "coconut cream" (via substitution engine).
5. **Persistent pantry management** — User maintains a long-term ingredient inventory, adding to it as they shop.

---

## Data Model

### UserPantryItem (New)

Replaces the global `PantryStaple` table. Each user maintains their own pantry inventory.

```csharp
public class UserPantryItem : AuditableEntity
{
    public int UserPantryItemId { get; set; }
    public Guid SubjectId { get; set; }  // User owner
    public string IngredientName { get; set; }  // e.g., "chicken breast"
    public bool IsStaple { get; set; }  // true = auto-assumed in searches by default
    public DateTime? ExpirationDate { get; set; }  // optional
}
```

**Indexes:**
- Unique: `(SubjectId, IngredientName)` — no duplicate ingredients per user
- Covering: `(SubjectId, IsStaple)` — fast lookup of user staples

**Seeding:**
When a user first accesses ingredient search, seed their pantry with default staples:
- salt, black pepper, water, olive oil, vegetable oil, butter, all-purpose flour, granulated sugar, garlic, onion

Users can add/remove items, toggle `IsStaple`, and set expiration dates.

### IngredientSynonym (Expanded)

Existing table, expanded with 50+ mappings and a source tracking field.

```csharp
public class IngredientSynonym
{
    public int IngredientSynonymId { get; set; }
    public string CanonicalName { get; set; }  // e.g., "cilantro"
    public string Synonym { get; set; }  // e.g., "coriander leaves"
    public SynonymSource Source { get; set; }  // manual | imported | user_suggested
}

public enum SynonymSource
{
    Manual = 0,        // Hand-curated by devs
    Imported = 1,      // Bulk-imported from crowdsource data
    UserSuggested = 2  // User-submitted suggestions (pending admin review)
}
```

**Full Seed Data (50+ mappings):**

| CanonicalName | Synonym | Source |
|---------------|---------|--------|
| chicken breast | chicken breasts | manual |
| chicken breast | boneless skinless chicken breast | manual |
| chicken breast | chicken cutlet | manual |
| ground beef | hamburger meat | manual |
| ground beef | minced beef | manual |
| ground beef | beef mince | manual |
| bell pepper | sweet pepper | manual |
| bell pepper | capsicum | manual |
| heavy cream | heavy whipping cream | manual |
| heavy cream | whipping cream | manual |
| green onion | scallion | manual |
| green onion | spring onion | manual |
| cilantro | coriander leaves | manual |
| cilantro | chinese parsley | manual |
| eggplant | aubergine | manual |
| zucchini | courgette | manual |
| shrimp | prawns | manual |
| shrimp | prawn | manual |
| corn | maize | manual |
| snow peas | mangetout | manual |
| arugula | rocket | manual |
| lima beans | butter beans | manual |
| garbanzo beans | chickpeas | manual |
| chickpeas | garbanzo beans | manual |
| baking soda | bicarbonate of soda | manual |
| confectioners sugar | powdered sugar | manual |
| confectioners sugar | icing sugar | manual |
| molasses | black treacle | manual |
| cookie | biscuit | manual |
| eggplant | brinjal | manual |
| rutabaga | swede | manual |
| potato | potatoes | manual |
| tomato | tomatoes | manual |
| onion | onions | manual |
| carrot | carrots | manual |
| apple | apples | manual |
| banana | bananas | manual |
| egg | eggs | manual |
| garlic clove | clove of garlic | manual |
| garlic clove | garlic cloves | manual |
| soy sauce | soya sauce | manual |
| soy sauce | shoyu | manual |
| sesame oil | toasted sesame oil | manual |
| rice vinegar | rice wine vinegar | manual |
| white vinegar | distilled vinegar | manual |
| ground cinnamon | cinnamon powder | manual |
| red pepper flakes | crushed red pepper | manual |
| red pepper flakes | chili flakes | manual |
| cayenne pepper | cayenne | manual |
| black pepper | ground black pepper | manual |
| kosher salt | coarse salt | manual |
| sea salt | flaky sea salt | manual |
| peanut butter | smooth peanut butter | manual |
| peanut butter | crunchy peanut butter | manual |
| tahini | sesame paste | manual |

### RecipeIngredient (Existing, No Changes)

```csharp
public class RecipeIngredient
{
    public int RecipeIngredientId { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }
    public string Item { get; set; }  // Parsed ingredient name, e.g., "chicken breast"
    public string RawText { get; set; }  // Original text, e.g., "2 lbs chicken breast"
    public decimal? Quantity { get; set; }
    public string Unit { get; set; }
    public string Notes { get; set; }  // e.g., "to taste", "for garnish", "optional"
}
```

The `Quantity` and `Notes` fields are used for ingredient importance weighting (see Implementation section).

---

## API Design

### Search Recipes by Ingredients

```
POST /api/v1/recipes/search/by-ingredients
```

**Request:**

```json
{
    "ingredients": ["chicken breast", "rice", "broccoli", "soy sauce"],
    "maxMissingIngredients": 3,
    "includePantryStaples": true,
    "includeSubstitutions": false,
    "maxCookTimeMinutes": null,
    "dietaryProfileResourceId": null,
    "pageNumber": 1,
    "pageSize": 25,
    "sortBy": "matchPercentage"
}
```

**Response:**

```json
{
    "items": [
        {
            "recipe": {
                "recipeResourceId": "abc123...",
                "title": "Chicken Stir Fry",
                "prepTimeMinutes": 15,
                "cookTimeMinutes": 20,
                "totalTimeMinutes": 35,
                "originalImageUrl": "https://...",
                "rating": 4,
                "isFavorite": false,
                "isOwner": true,
                "tags": [...]
            },
            "matchedIngredients": ["chicken breast", "rice", "broccoli", "soy sauce"],
            "missingIngredients": ["sesame oil", "cornstarch"],
            "matchPercentage": 75.0,
            "weightedMatchPercentage": 82.5,
            "pantryStaplesUsed": ["garlic", "salt"],
            "substitutionsAvailable": [
                {
                    "recipeIngredient": "sesame oil",
                    "possibleSubstitute": "olive oil + toasted sesame seeds"
                }
            ]
        }
    ],
    "pageNumber": 1,
    "pageSize": 25,
    "totalCount": 42,
    "totalPages": 2
}
```

**Notes:**
- `matchPercentage` = simple count-based percentage (matched / total non-pantry ingredients)
- `weightedMatchPercentage` = importance-weighted score (primary ingredients count more)

### Get Ingredient Suggestions (Autocomplete)

```
GET /api/v1/ingredients/suggest?query=chick&limit=10
```

**Response:**

```json
{
    "suggestions": [
        { "name": "chicken breast", "recipeCount": 24 },
        { "name": "chicken thigh", "recipeCount": 18 },
        { "name": "chickpeas", "recipeCount": 12 },
        { "name": "chicken stock", "recipeCount": 31 }
    ]
}
```

Returns distinct ingredient items from `RecipeIngredient` matching the query using word-boundary tokenization, ordered by frequency.

### User Pantry Management

#### Get User Pantry Items

```
GET /api/v1/pantry
```

**Response:**

```json
{
    "items": [
        {
            "userPantryItemId": 1,
            "ingredientName": "chicken breast",
            "isStaple": false,
            "expirationDate": "2026-03-20"
        },
        {
            "ingredientName": "salt",
            "isStaple": true,
            "expirationDate": null
        }
    ]
}
```

#### Add Pantry Item

```
POST /api/v1/pantry
```

**Request:**

```json
{
    "ingredientName": "broccoli",
    "isStaple": false,
    "expirationDate": "2026-03-18"
}
```

**Response:** `201 Created` with the created `UserPantryItem`.

#### Update Pantry Item

```
PUT /api/v1/pantry/{id}
```

**Request:**

```json
{
    "ingredientName": "broccoli",
    "isStaple": false,
    "expirationDate": "2026-03-25"
}
```

**Response:** `200 OK` with updated item.

#### Delete Pantry Item

```
DELETE /api/v1/pantry/{id}
```

**Response:** `204 No Content`

#### Get Default Pantry Staples (Seed List)

```
GET /api/v1/pantry/defaults
```

**Response:**

```json
{
    "staples": ["salt", "black pepper", "water", "olive oil", "vegetable oil", "butter", "all-purpose flour", "granulated sugar", "garlic", "onion"]
}
```

Returns the global default list. Used to seed a new user's pantry on first access.

---

## UX Design

### Ingredient Search Page

```
┌──────────────────────────────────────────────────────────────────┐
│  What's in Your Fridge?                                          │
│                                                                  │
│  [My Pantry] [Search]                                            │
│                                                                  │
│  ┌────────────────────────────────────────┐                     │
│  │ Type an ingredient...             [+]  │                     │
│  └────────────────────────────────────────┘                     │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  chicken breast         24 recipes                      │    │
│  │  chicken thigh          18 recipes                      │    │
│  │  chickpeas              12 recipes                      │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  Your ingredients:                                               │
│  [x chicken breast] [x rice] [x broccoli] [x soy sauce]        │
│                                                                  │
│  ┌─ Filters ──────────────────────────────────────────────┐     │
│  │  Max missing: [3 ▼]   Cook time: [Any ▼]              │     │
│  │  [✓] Include my pantry staples                         │     │
│  │  [ ] Show substitution suggestions                     │     │
│  │  Dietary profile: [None ▼]                             │     │
│  │  Sort by: [Best Match ▼]                               │     │
│  └────────────────────────────────────────────────────────┘     │
│                                                                  │
│  [Search Recipes]                                                │
│                                                                  │
│  ─── 42 recipes found ───                                       │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Chicken Stir Fry                       83% match       │   │
│  │  ★★★★☆  35 min                          (weighted)      │   │
│  │                                                          │   │
│  │  ✓ chicken breast (primary)  ✓ rice (primary)           │   │
│  │  ✓ broccoli  ✓ soy sauce                                │   │
│  │  ✗ sesame oil  ✗ cornstarch                             │   │
│  │  ~ garlic (pantry)  ~ salt (pantry)                     │   │
│  │                                                          │   │
│  │  💡 Can substitute: sesame oil → olive oil + sesame     │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  [1] [2] [>]                                                    │
└──────────────────────────────────────────────────────────────────┘
```

### My Pantry Management

New section accessible via tab on the ingredient search page:

```
┌──────────────────────────────────────────────────────────────────┐
│  My Pantry                                                       │
│                                                                  │
│  [Search] [My Pantry]                                            │
│                                                                  │
│  Manage your ingredient inventory. Items marked as staples are   │
│  automatically included in ingredient searches.                  │
│                                                                  │
│  ┌────────────────────────────────────────┐                     │
│  │ Add ingredient...                 [+]  │                     │
│  └────────────────────────────────────────┘                     │
│                                                                  │
│  ┌─ Pantry Staples ─────────────────────────────────────────┐   │
│  │  [x] salt                                                 │   │
│  │  [x] black pepper                                         │   │
│  │  [x] olive oil                                            │   │
│  │  [x] garlic                                               │   │
│  │  [x] onion                                                │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌─ Current Ingredients ────────────────────────────────────┐   │
│  │  chicken breast          [🗑] Expires: 2026-03-20  [Edit]│   │
│  │  broccoli                [🗑] Expires: 2026-03-18  [Edit]│   │
│  │  soy sauce               [🗑] No expiration         [Edit]│   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  [Load my pantry into search]                                   │
└──────────────────────────────────────────────────────────────────┘
```

**Behavior:**
- When user opens ingredient search, pantry items are pre-loaded into the ingredient list
- User can add temporary ingredients (for this search only) without saving to pantry
- "Load my pantry into search" button copies all pantry items to the search ingredient list
- Items marked `IsStaple` are auto-included when "Include my pantry staples" filter is checked

### Mobile Layout

On narrow screens, tabs stack and filter section collapses:

```
┌─────────────────────────┐
│  What's in Your Fridge? │
│                         │
│  [Search] [My Pantry]   │
│                         │
│  [Type ingredient... +] │
│                         │
│  [x chicken] [x rice]  │
│  [x broccoli]          │
│                         │
│  [▸ Filters]            │
│                         │
│  [Search Recipes]       │
│                         │
│  ┌─────────────────────┐│
│  │ Chicken Stir Fry    ││
│  │ 83% match  35 min   ││
│  │ Missing: sesame oil,││
│  │ cornstarch           ││
│  └─────────────────────┘│
└─────────────────────────┘
```

---

## Implementation

### Database Migration

```sql
-- ============================================================================
-- Migration: Ingredient-First Search
-- ============================================================================

-- ============================================================================
-- 1. User Pantry Items Table
-- ============================================================================

CREATE TABLE "UserPantryItem" (
    "UserPantryItemId" SERIAL PRIMARY KEY,
    "SubjectId" UUID NOT NULL,
    "IngredientName" VARCHAR(250) NOT NULL,
    "IsStaple" BOOLEAN NOT NULL DEFAULT false,
    "ExpirationDate" TIMESTAMP NULL,
    "CreatedSubjectId" UUID NOT NULL,
    "CreatedTimestamp" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ModifiedSubjectId" UUID NULL,
    "ModifiedTimestamp" TIMESTAMP NULL
);

CREATE UNIQUE INDEX "IX_UserPantryItem_SubjectId_IngredientName" 
    ON "UserPantryItem"("SubjectId", "IngredientName");

CREATE INDEX "IX_UserPantryItem_SubjectId_IsStaple" 
    ON "UserPantryItem"("SubjectId", "IsStaple");

-- ============================================================================
-- 2. Ingredient Synonym Table (Expanded)
-- ============================================================================

CREATE TABLE "IngredientSynonym" (
    "IngredientSynonymId" SERIAL PRIMARY KEY,
    "CanonicalName" VARCHAR(250) NOT NULL,
    "Synonym" VARCHAR(250) NOT NULL,
    "Source" INT NOT NULL DEFAULT 0  -- 0=Manual, 1=Imported, 2=UserSuggested
);

CREATE UNIQUE INDEX "IX_IngredientSynonym_Synonym" ON "IngredientSynonym"("Synonym");
CREATE INDEX "IX_IngredientSynonym_CanonicalName" ON "IngredientSynonym"("CanonicalName");
CREATE INDEX "IX_IngredientSynonym_Source" ON "IngredientSynonym"("Source");

-- Seed 50+ synonym mappings
INSERT INTO "IngredientSynonym" ("CanonicalName", "Synonym", "Source") VALUES
    ('chicken breast', 'chicken breasts', 0),
    ('chicken breast', 'boneless skinless chicken breast', 0),
    ('chicken breast', 'chicken cutlet', 0),
    ('ground beef', 'hamburger meat', 0),
    ('ground beef', 'minced beef', 0),
    ('ground beef', 'beef mince', 0),
    ('bell pepper', 'sweet pepper', 0),
    ('bell pepper', 'capsicum', 0),
    ('heavy cream', 'heavy whipping cream', 0),
    ('heavy cream', 'whipping cream', 0),
    ('green onion', 'scallion', 0),
    ('green onion', 'spring onion', 0),
    ('cilantro', 'coriander leaves', 0),
    ('cilantro', 'chinese parsley', 0),
    ('eggplant', 'aubergine', 0),
    ('zucchini', 'courgette', 0),
    ('shrimp', 'prawns', 0),
    ('shrimp', 'prawn', 0),
    ('corn', 'maize', 0),
    ('snow peas', 'mangetout', 0),
    ('arugula', 'rocket', 0),
    ('lima beans', 'butter beans', 0),
    ('garbanzo beans', 'chickpeas', 0),
    ('chickpeas', 'garbanzo beans', 0),
    ('baking soda', 'bicarbonate of soda', 0),
    ('confectioners sugar', 'powdered sugar', 0),
    ('confectioners sugar', 'icing sugar', 0),
    ('molasses', 'black treacle', 0),
    ('cookie', 'biscuit', 0),
    ('eggplant', 'brinjal', 0),
    ('rutabaga', 'swede', 0),
    ('potato', 'potatoes', 0),
    ('tomato', 'tomatoes', 0),
    ('onion', 'onions', 0),
    ('carrot', 'carrots', 0),
    ('apple', 'apples', 0),
    ('banana', 'bananas', 0),
    ('egg', 'eggs', 0),
    ('garlic clove', 'clove of garlic', 0),
    ('garlic clove', 'garlic cloves', 0),
    ('soy sauce', 'soya sauce', 0),
    ('soy sauce', 'shoyu', 0),
    ('sesame oil', 'toasted sesame oil', 0),
    ('rice vinegar', 'rice wine vinegar', 0),
    ('white vinegar', 'distilled vinegar', 0),
    ('ground cinnamon', 'cinnamon powder', 0),
    ('red pepper flakes', 'crushed red pepper', 0),
    ('red pepper flakes', 'chili flakes', 0),
    ('cayenne pepper', 'cayenne', 0),
    ('black pepper', 'ground black pepper', 0),
    ('kosher salt', 'coarse salt', 0),
    ('sea salt', 'flaky sea salt', 0),
    ('peanut butter', 'smooth peanut butter', 0),
    ('peanut butter', 'crunchy peanut butter', 0),
    ('tahini', 'sesame paste', 0);

-- ============================================================================
-- 3. GIN Trigram Index on RecipeIngredient.Item (with fallback)
-- ============================================================================

DO $$
BEGIN
    -- Try to create pg_trgm extension
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
    
    -- Create trigram index if extension exists
    IF EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') THEN
        CREATE INDEX "IX_RecipeIngredient_Item_Trgm" 
            ON "RecipeIngredient" USING GIN ("Item" gin_trgm_ops);
        RAISE NOTICE 'pg_trgm extension enabled, trigram index created';
    ELSE
        -- Fall back to standard B-tree index for prefix matching
        CREATE INDEX "IX_RecipeIngredient_Item_Prefix" 
            ON "RecipeIngredient"("Item" varchar_pattern_ops);
        RAISE WARNING 'pg_trgm extension not available, using ILIKE prefix matching';
    END IF;
END $$;

-- ============================================================================
-- 4. PostgreSQL Function: Score Recipes by Ingredients
-- ============================================================================

CREATE OR REPLACE FUNCTION score_recipes_by_ingredients(
    p_subject_id UUID,
    p_user_ingredients TEXT[],
    p_pantry_staples TEXT[]
)
RETURNS TABLE (
    recipe_id INT,
    matched_ingredients TEXT[],
    missing_ingredients TEXT[],
    pantry_staples_used TEXT[],
    match_percentage NUMERIC,
    weighted_match_percentage NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    WITH 
    -- Expand user ingredients through synonyms
    expanded_user_ingredients AS (
        SELECT DISTINCT LOWER(TRIM(syn."CanonicalName")) AS ingredient
        FROM UNNEST(p_user_ingredients) AS user_ing
        LEFT JOIN "IngredientSynonym" syn ON LOWER(TRIM(syn."Synonym")) = LOWER(TRIM(user_ing))
        UNION
        SELECT DISTINCT LOWER(TRIM(user_ing)) AS ingredient
        FROM UNNEST(p_user_ingredients) AS user_ing
    ),
    -- Get all available ingredients (user + pantry staples)
    all_available AS (
        SELECT ingredient FROM expanded_user_ingredients
        UNION
        SELECT LOWER(TRIM(staple)) AS ingredient FROM UNNEST(p_pantry_staples) AS staple
    ),
    -- Tokenize recipe ingredients into words for word-boundary matching
    recipe_ingredients_tokenized AS (
        SELECT 
            ri."RecipeId",
            ri."RecipeIngredientId",
            LOWER(TRIM(ri."Item")) AS item,
            ri."Quantity",
            ri."Notes",
            -- Split item into words, removing punctuation
            regexp_split_to_array(
                regexp_replace(LOWER(TRIM(ri."Item")), '[^a-z0-9\s]', ' ', 'g'),
                '\s+'
            ) AS item_words
        FROM "RecipeIngredient" ri
        WHERE ri."Item" IS NOT NULL AND TRIM(ri."Item") <> ''
    ),
    -- Calculate ingredient importance weight
    recipe_ingredients_weighted AS (
        SELECT
            "RecipeId",
            "RecipeIngredientId",
            item,
            item_words,
            CASE
                -- Secondary ingredients: tiny quantities or garnish/optional notes
                WHEN "Notes" ILIKE '%to taste%' OR "Notes" ILIKE '%for garnish%' 
                    OR "Notes" ILIKE '%optional%' OR "Quantity" < 0.25 THEN 0.5
                -- Primary ingredients: first 4 ingredients or large quantities
                WHEN ROW_NUMBER() OVER (PARTITION BY "RecipeId" ORDER BY "RecipeIngredientId") <= 4
                    OR "Quantity" >= 2.0 THEN 2.0
                -- Default weight
                ELSE 1.0
            END AS weight
        FROM recipe_ingredients_tokenized
    ),
    -- Match recipe ingredients against available ingredients
    recipe_matches AS (
        SELECT
            rw."RecipeId",
            rw.item,
            rw.weight,
            CASE
                -- Exact match (case-insensitive)
                WHEN EXISTS (
                    SELECT 1 FROM all_available aa WHERE aa.ingredient = rw.item
                ) THEN 'matched'
                -- Word-boundary match: any word in recipe item matches a user ingredient
                WHEN EXISTS (
                    SELECT 1 FROM all_available aa, UNNEST(rw.item_words) AS word
                    WHERE aa.ingredient = word
                ) THEN 'matched'
                -- Pantry staple match
                WHEN EXISTS (
                    SELECT 1 FROM UNNEST(p_pantry_staples) AS staple
                    WHERE LOWER(TRIM(staple)) = rw.item
                ) THEN 'pantry'
                -- Not matched
                ELSE 'missing'
            END AS match_status
        FROM recipe_ingredients_weighted rw
    ),
    -- Aggregate results per recipe
    recipe_scores AS (
        SELECT
            rm."RecipeId",
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'matched') AS matched,
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'missing') AS missing,
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'pantry') AS pantry,
            COALESCE(ARRAY_LENGTH(ARRAY_AGG(rm.item) FILTER (WHERE rm.match_status = 'matched'), 1), 0)::NUMERIC AS matched_count,
            COALESCE(ARRAY_LENGTH(ARRAY_AGG(rm.item) FILTER (WHERE rm.match_status = 'missing'), 1), 0)::NUMERIC AS missing_count,
            COALESCE(SUM(rm.weight) FILTER (WHERE rm.match_status = 'matched'), 0)::NUMERIC AS weighted_matched,
            COALESCE(SUM(rm.weight) FILTER (WHERE rm.match_status = 'missing'), 0)::NUMERIC AS weighted_missing
        FROM recipe_matches rm
        GROUP BY rm."RecipeId"
    )
    -- Final output
    SELECT
        rs."RecipeId" AS recipe_id,
        COALESCE(rs.matched, ARRAY[]::TEXT[]) AS matched_ingredients,
        COALESCE(rs.missing, ARRAY[]::TEXT[]) AS missing_ingredients,
        COALESCE(rs.pantry, ARRAY[]::TEXT[]) AS pantry_staples_used,
        CASE 
            WHEN (rs.matched_count + rs.missing_count) = 0 THEN 0
            ELSE ROUND((rs.matched_count / (rs.matched_count + rs.missing_count)) * 100, 1)
        END AS match_percentage,
        CASE
            WHEN (rs.weighted_matched + rs.weighted_missing) = 0 THEN 0
            ELSE ROUND((rs.weighted_matched / (rs.weighted_matched + rs.weighted_missing)) * 100, 1)
        END AS weighted_match_percentage
    FROM recipe_scores rs
    -- Only return recipes owned by user or public
    INNER JOIN "Recipe" r ON r."RecipeId" = rs."RecipeId"
    WHERE r."CreatedSubjectId" = p_subject_id OR r."IsPublic" = true;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 5. Default Pantry Staples (for seeding new users)
-- ============================================================================

-- Store in a simple key-value config table (or hardcode in C# service)
-- For this implementation, we'll hardcode the defaults in the service layer

COMMENT ON FUNCTION score_recipes_by_ingredients IS 
'Scores recipes based on available ingredients. Uses word-boundary matching to avoid false positives (e.g., "rice" does not match "licorice"). Returns matched, missing, and pantry-used ingredients with both simple and weighted match percentages.';

```

### C# Service Layer

#### IngredientSearchService

```csharp
public class IngredientSearchService : IIngredientSearchService
{
    private readonly IRecipeRepository recipeRepository;
    private readonly IIngredientSynonymRepository synonymRepository;
    private readonly IUserPantryRepository userPantryRepository;
    private readonly ISubstitutionService substitutionService;
    private readonly IDietaryProfileService dietaryProfileService;
    private readonly ISubjectPrincipal subjectPrincipal;
    private readonly ILogger<IngredientSearchService> logger;
    private readonly IDbConnection dbConnection;

    private static readonly List<string> DefaultPantryStaples = new()
    {
        "salt", "black pepper", "water", "olive oil", "vegetable oil",
        "butter", "all-purpose flour", "granulated sugar", "garlic", "onion"
    };

    public async Task<PagedList<IngredientSearchResultDto>> SearchByIngredientsAsync(
        IngredientSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);

        // 1. Seed user pantry if this is their first access
        await EnsureUserPantrySeededAsync(currentSubjectId, cancellationToken)
            .ConfigureAwait(false);

        // 2. Get user's pantry staples if requested
        var pantryStaples = request.IncludePantryStaples
            ? await userPantryRepository.GetStaplesAsync(currentSubjectId, cancellationToken)
                .ConfigureAwait(false)
            : new List<string>();

        // 3. Call PostgreSQL scoring function
        var scored = await ScoreRecipesInDatabaseAsync(
            currentSubjectId,
            request.Ingredients,
            pantryStaples,
            cancellationToken
        ).ConfigureAwait(false);

        // 4. Apply filters
        var filtered = scored
            .Where(r => r.MissingIngredients.Count <= request.MaxMissingIngredients)
            .ToList();

        if (request.MaxCookTimeMinutes.HasValue)
        {
            filtered = filtered
                .Where(r => r.Recipe.TotalTimeMinutes <= request.MaxCookTimeMinutes.Value)
                .ToList();
        }

        // 5. Apply dietary profile filter
        if (request.DietaryProfileResourceId.HasValue)
        {
            filtered = await FilterByDietaryProfileAsync(
                filtered,
                request.DietaryProfileResourceId.Value,
                cancellationToken
            ).ConfigureAwait(false);
        }

        // 6. Find substitutions if requested
        if (request.IncludeSubstitutions)
        {
            var allAvailableIngredients = request.Ingredients
                .Union(pantryStaples, StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var result in filtered)
            {
                result.SubstitutionsAvailable = await FindSubstitutionsAsync(
                    result.MissingIngredients,
                    allAvailableIngredients,
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }

        // 7. Sort and paginate
        var sorted = request.SortBy switch
        {
            "matchPercentage" => filtered
                .OrderByDescending(r => r.WeightedMatchPercentage)
                .ThenByDescending(r => r.Recipe.Rating ?? 0),
            "cookTime" => filtered
                .OrderBy(r => r.Recipe.TotalTimeMinutes ?? int.MaxValue),
            "rating" => filtered
                .OrderByDescending(r => r.Recipe.Rating ?? 0),
            _ => filtered
                .OrderByDescending(r => r.WeightedMatchPercentage)
        };

        return PagedList<IngredientSearchResultDto>.Create(
            sorted,
            request.PageNumber,
            request.PageSize
        );
    }

    private async Task<List<IngredientSearchResultDto>> ScoreRecipesInDatabaseAsync(
        Guid subjectId,
        List<string> userIngredients,
        List<string> pantryStaples,
        CancellationToken cancellationToken)
    {
        // Call the PostgreSQL function
        var sql = @"
            SELECT 
                recipe_id,
                matched_ingredients,
                missing_ingredients,
                pantry_staples_used,
                match_percentage,
                weighted_match_percentage
            FROM score_recipes_by_ingredients(
                @SubjectId,
                @UserIngredients,
                @PantryStaples
            );
        ";

        using var command = dbConnection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new NpgsqlParameter("@SubjectId", subjectId));
        command.Parameters.Add(new NpgsqlParameter("@UserIngredients", userIngredients.ToArray()));
        command.Parameters.Add(new NpgsqlParameter("@PantryStaples", pantryStaples.ToArray()));

        var results = new List<IngredientSearchResultDto>();

        using var reader = await command.ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var recipeId = reader.GetInt32(0);
            var matched = reader.GetFieldValue<string[]>(1);
            var missing = reader.GetFieldValue<string[]>(2);
            var pantryUsed = reader.GetFieldValue<string[]>(3);
            var matchPercentage = reader.GetDecimal(4);
            var weightedMatchPercentage = reader.GetDecimal(5);

            // Fetch full recipe details
            var recipe = await recipeRepository.GetByIdAsync(recipeId, cancellationToken)
                .ConfigureAwait(false);

            if (recipe != null)
            {
                results.Add(new IngredientSearchResultDto
                {
                    Recipe = MapToRecipeSummary(recipe),
                    MatchedIngredients = matched.ToList(),
                    MissingIngredients = missing.ToList(),
                    PantryStaplesUsed = pantryUsed.ToList(),
                    MatchPercentage = (double)matchPercentage,
                    WeightedMatchPercentage = (double)weightedMatchPercentage,
                    SubstitutionsAvailable = new List<SubstitutionSuggestionDto>()
                });
            }
        }

        return results;
    }

    private async Task EnsureUserPantrySeededAsync(
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var existingCount = await userPantryRepository.CountAsync(subjectId, cancellationToken)
            .ConfigureAwait(false);

        if (existingCount == 0)
        {
            // Seed default staples
            var stapleItems = DefaultPantryStaples.Select(name => new UserPantryItem
            {
                SubjectId = subjectId,
                IngredientName = name,
                IsStaple = true,
                CreatedSubjectId = subjectId,
                CreatedTimestamp = DateTime.UtcNow
            }).ToList();

            await userPantryRepository.AddRangeAsync(stapleItems, cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation(
                "Seeded {Count} default pantry staples for user {SubjectId}",
                stapleItems.Count,
                subjectId
            );
        }
    }

    private async Task<List<IngredientSearchResultDto>> FilterByDietaryProfileAsync(
        List<IngredientSearchResultDto> results,
        Guid dietaryProfileResourceId,
        CancellationToken cancellationToken)
    {
        var profile = await dietaryProfileService.GetProfileAsync(
            dietaryProfileResourceId,
            cancellationToken
        ).ConfigureAwait(false);

        if (profile == null) return results;

        var avoidedNames = profile.AvoidedIngredients
            .Select(a => a.IngredientName.ToLowerInvariant())
            .ToHashSet();

        return results
            .Where(r => !r.Recipe.Ingredients.Any(i =>
                avoidedNames.Contains(i.Item.ToLowerInvariant())))
            .ToList();
    }

    private async Task<List<SubstitutionSuggestionDto>> FindSubstitutionsAsync(
        List<string> missingIngredients,
        HashSet<string> availableIngredients,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<SubstitutionSuggestionDto>();

        foreach (var missing in missingIngredients)
        {
            var substitutions = await substitutionService
                .GetSubstitutionsForAsync(missing, cancellationToken)
                .ConfigureAwait(false);

            var viable = substitutions
                .Where(s => availableIngredients.Contains(
                    s.SubstituteIngredient,
                    StringComparer.OrdinalIgnoreCase
                ))
                .ToList();

            if (viable.Any())
            {
                suggestions.Add(new SubstitutionSuggestionDto
                {
                    RecipeIngredient = missing,
                    PossibleSubstitute = viable.First().Description
                });
            }
        }

        return suggestions;
    }

    private RecipeSummaryDto MapToRecipeSummary(Recipe recipe)
    {
        // Map Recipe entity to RecipeSummaryDto
        // (implementation omitted for brevity)
        return new RecipeSummaryDto { /* ... */ };
    }
}
```

#### IngredientSuggestService

```csharp
public class IngredientSuggestService : IIngredientSuggestService
{
    private readonly IRecipeRepository recipeRepository;
    private readonly IDbConnection dbConnection;
    private readonly ILogger<IngredientSuggestService> logger;

    public async Task<List<IngredientSuggestionDto>> SuggestAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<IngredientSuggestionDto>();

        // Use trigram similarity if available, otherwise ILIKE prefix matching
        var hasPgTrgm = await CheckPgTrgmAvailableAsync(cancellationToken)
            .ConfigureAwait(false);

        var sql = hasPgTrgm
            ? BuildTrigramSuggestQuery()
            : BuildILikeSuggestQuery();

        using var command = dbConnection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new NpgsqlParameter("@Query", query.ToLowerInvariant()));
        command.Parameters.Add(new NpgsqlParameter("@Limit", limit));

        var suggestions = new List<IngredientSuggestionDto>();

        using var reader = await command.ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            suggestions.Add(new IngredientSuggestionDto
            {
                Name = reader.GetString(0),
                RecipeCount = reader.GetInt32(1)
            });
        }

        return suggestions;
    }

    private async Task<bool> CheckPgTrgmAvailableAsync(CancellationToken cancellationToken)
    {
        var sql = "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm');";
        using var command = dbConnection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(cancellationToken)
            .ConfigureAwait(false);
        return (bool)(result ?? false);
    }

    private string BuildTrigramSuggestQuery()
    {
        return @"
            SELECT 
                LOWER(TRIM(ri.""Item"")) AS ingredient_name,
                COUNT(DISTINCT ri.""RecipeId"") AS recipe_count
            FROM ""RecipeIngredient"" ri
            WHERE 
                ri.""Item"" IS NOT NULL 
                AND TRIM(ri.""Item"") <> ''
                AND LOWER(ri.""Item"") % @Query  -- Trigram similarity operator
            GROUP BY ingredient_name
            ORDER BY 
                similarity(LOWER(ri.""Item""), @Query) DESC,
                recipe_count DESC
            LIMIT @Limit;
        ";
    }

    private string BuildILikeSuggestQuery()
    {
        return @"
            SELECT 
                LOWER(TRIM(ri.""Item"")) AS ingredient_name,
                COUNT(DISTINCT ri.""RecipeId"") AS recipe_count
            FROM ""RecipeIngredient"" ri
            WHERE 
                ri.""Item"" IS NOT NULL 
                AND TRIM(ri.""Item"") <> ''
                AND LOWER(ri.""Item"") ILIKE @Query || '%'  -- Prefix matching
            GROUP BY ingredient_name
            ORDER BY recipe_count DESC
            LIMIT @Limit;
        ";
    }
}
```

#### UserPantryService

```csharp
public class UserPantryService : IUserPantryService
{
    private readonly IUserPantryRepository repository;
    private readonly ISubjectPrincipal subjectPrincipal;
    private readonly ILogger<UserPantryService> logger;

    public async Task<List<UserPantryItemDto>> GetUserPantryAsync(
        CancellationToken cancellationToken = default)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var items = await repository.GetBySubjectIdAsync(subjectId, cancellationToken)
            .ConfigureAwait(false);
        return items.Select(MapToDto).ToList();
    }

    public async Task<UserPantryItemDto> AddPantryItemAsync(
        CreatePantryItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);

        var item = new UserPantryItem
        {
            SubjectId = subjectId,
            IngredientName = dto.IngredientName.Trim(),
            IsStaple = dto.IsStaple,
            ExpirationDate = dto.ExpirationDate,
            CreatedSubjectId = subjectId,
            CreatedTimestamp = DateTime.UtcNow
        };

        await repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
        return MapToDto(item);
    }

    public async Task<UserPantryItemDto> UpdatePantryItemAsync(
        int id,
        UpdatePantryItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var item = await repository.GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (item == null || item.SubjectId != subjectId)
            throw new NotFoundException("Pantry item not found");

        item.IngredientName = dto.IngredientName.Trim();
        item.IsStaple = dto.IsStaple;
        item.ExpirationDate = dto.ExpirationDate;
        item.ModifiedSubjectId = subjectId;
        item.ModifiedTimestamp = DateTime.UtcNow;

        await repository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        return MapToDto(item);
    }

    public async Task DeletePantryItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var item = await repository.GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (item == null || item.SubjectId != subjectId)
            throw new NotFoundException("Pantry item not found");

        await repository.DeleteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<string>> GetDefaultStaplesAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new List<string>
        {
            "salt", "black pepper", "water", "olive oil", "vegetable oil",
            "butter", "all-purpose flour", "granulated sugar", "garlic", "onion"
        });
    }

    private UserPantryItemDto MapToDto(UserPantryItem item)
    {
        return new UserPantryItemDto
        {
            UserPantryItemId = item.UserPantryItemId,
            IngredientName = item.IngredientName,
            IsStaple = item.IsStaple,
            ExpirationDate = item.ExpirationDate
        };
    }
}
```

### DTOs

```csharp
// ============================================================================
// Request DTOs
// ============================================================================

public class IngredientSearchRequestDto
{
    public List<string> Ingredients { get; set; } = new();
    public int MaxMissingIngredients { get; set; } = 5;
    public bool IncludePantryStaples { get; set; } = true;
    public bool IncludeSubstitutions { get; set; } = false;
    public int? MaxCookTimeMinutes { get; set; }
    public Guid? DietaryProfileResourceId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "matchPercentage";
}

public class CreatePantryItemDto
{
    [Required]
    [MaxLength(250)]
    public string IngredientName { get; set; }
    
    public bool IsStaple { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class UpdatePantryItemDto
{
    [Required]
    [MaxLength(250)]
    public string IngredientName { get; set; }
    
    public bool IsStaple { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

// ============================================================================
// Response DTOs
// ============================================================================

public class IngredientSearchResultDto
{
    public RecipeSummaryDto Recipe { get; set; }
    public List<string> MatchedIngredients { get; set; } = new();
    public List<string> MissingIngredients { get; set; } = new();
    public List<string> PantryStaplesUsed { get; set; } = new();
    public double MatchPercentage { get; set; }
    public double WeightedMatchPercentage { get; set; }
    public List<SubstitutionSuggestionDto> SubstitutionsAvailable { get; set; } = new();
}

public class RecipeSummaryDto
{
    public Guid RecipeResourceId { get; set; }
    public string Title { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? TotalTimeMinutes { get; set; }
    public string OriginalImageUrl { get; set; }
    public int? Rating { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsOwner { get; set; }
    public List<RecipeTagDto> Tags { get; set; } = new();
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
}

public class SubstitutionSuggestionDto
{
    public string RecipeIngredient { get; set; }
    public string PossibleSubstitute { get; set; }
}

public class IngredientSuggestionDto
{
    public string Name { get; set; }
    public int RecipeCount { get; set; }
}

public class UserPantryItemDto
{
    public int UserPantryItemId { get; set; }
    public string IngredientName { get; set; }
    public bool IsStaple { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
```

### Controller Endpoints

```csharp
// ============================================================================
// RecipesController.cs (additions)
// ============================================================================

[HttpPost("search/by-ingredients")]
[ProducesResponseType(typeof(PagedList<IngredientSearchResultModel>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> SearchByIngredientsAsync(
    [FromBody] IngredientSearchRequestModel model,
    CancellationToken cancellationToken = default)
{
    if (model.Ingredients == null || model.Ingredients.Count == 0)
        return BadRequest("At least one ingredient is required");

    if (model.Ingredients.Count > 30)
        return BadRequest("Maximum 30 ingredients allowed per search");

    var dto = mapper.Map<IngredientSearchRequestDto>(model);
    var results = await ingredientSearchFacade.SearchByIngredientsAsync(dto, cancellationToken)
        .ConfigureAwait(false);

    return Ok(mapper.Map<PagedList<IngredientSearchResultModel>>(results));
}

// ============================================================================
// IngredientsController.cs (new controller)
// ============================================================================

[ApiVersion("1")]
[Produces("application/json")]
[ApiController]
[Route("api/v{version:apiVersion}/ingredients")]
[Authorize]
public class IngredientsController : ControllerBase
{
    private readonly IIngredientFacade facade;
    private readonly IMapper mapper;

    public IngredientsController(IIngredientFacade facade, IMapper mapper)
    {
        this.facade = facade;
        this.mapper = mapper;
    }

    [HttpGet("suggest")]
    [ProducesResponseType(typeof(IngredientSuggestionsModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestIngredientsAsync(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(new IngredientSuggestionsModel { Suggestions = new List<IngredientSuggestionModel>() });

        var suggestions = await facade.SuggestAsync(query, Math.Min(limit, 25), cancellationToken)
            .ConfigureAwait(false);

        return Ok(new IngredientSuggestionsModel
        {
            Suggestions = mapper.Map<List<IngredientSuggestionModel>>(suggestions)
        });
    }
}

// ============================================================================
// PantryController.cs (new controller)
// ============================================================================

[ApiVersion("1")]
[Produces("application/json")]
[ApiController]
[Route("api/v{version:apiVersion}/pantry")]
[Authorize]
public class PantryController : ControllerBase
{
    private readonly IUserPantryFacade facade;
    private readonly IMapper mapper;

    public PantryController(IUserPantryFacade facade, IMapper mapper)
    {
        this.facade = facade;
        this.mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserPantryModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPantryAsync(CancellationToken cancellationToken = default)
    {
        var items = await facade.GetUserPantryAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new UserPantryModel { Items = mapper.Map<List<UserPantryItemModel>>(items) });
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserPantryItemModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPantryItemAsync(
        [FromBody] CreatePantryItemModel model,
        CancellationToken cancellationToken = default)
    {
        var dto = mapper.Map<CreatePantryItemDto>(model);
        var created = await facade.AddPantryItemAsync(dto, cancellationToken).ConfigureAwait(false);
        var resultModel = mapper.Map<UserPantryItemModel>(created);
        return CreatedAtAction(nameof(GetUserPantryAsync), new { id = created.UserPantryItemId }, resultModel);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserPantryItemModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePantryItemAsync(
        int id,
        [FromBody] UpdatePantryItemModel model,
        CancellationToken cancellationToken = default)
    {
        var dto = mapper.Map<UpdatePantryItemDto>(model);
        var updated = await facade.UpdatePantryItemAsync(id, dto, cancellationToken).ConfigureAwait(false);
        return Ok(mapper.Map<UserPantryItemModel>(updated));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePantryItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await facade.DeletePantryItemAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("defaults")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefaultStaplesAsync(CancellationToken cancellationToken = default)
    {
        var staples = await facade.GetDefaultStaplesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(staples);
    }
}
```

### Angular Frontend

#### ingredient-search.service.ts

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '@env/environment';
import {
  IngredientSearchRequest,
  IngredientSearchResult,
  IngredientSuggestion,
  UserPantryItem,
  CreatePantryItem,
  UpdatePantryItem
} from './ingredient-search.models';
import { PagedResult } from '@app/shared/models/paged-result';

@Injectable({ providedIn: 'root' })
export class IngredientSearchService {
  private apiUrl = `${environment.apiUrl}/api/v1`;
  
  // Client-side cache for autocomplete suggestions
  // Map<query, { suggestions: IngredientSuggestion[], timestamp: number }>
  private suggestionCache = new Map<string, { suggestions: IngredientSuggestion[], timestamp: number }>();
  private readonly CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes

  constructor(private http: HttpClient) {}

  // =========================================================================
  // Ingredient Search
  // =========================================================================

  searchByIngredients(request: IngredientSearchRequest): Observable<PagedResult<IngredientSearchResult>> {
    return this.http.post<PagedResult<IngredientSearchResult>>(
      `${this.apiUrl}/recipes/search/by-ingredients`,
      request
    );
  }

  // =========================================================================
  // Autocomplete Suggestions (with caching)
  // =========================================================================

  suggestIngredients(query: string, limit: number = 10): Observable<IngredientSuggestion[]> {
    const normalizedQuery = query.toLowerCase().trim();

    // Check cache
    const cached = this.suggestionCache.get(normalizedQuery);
    if (cached && Date.now() - cached.timestamp < this.CACHE_TTL_MS) {
      return of(cached.suggestions);
    }

    // Cache miss or expired — fetch from API
    const params = new HttpParams()
      .set('query', normalizedQuery)
      .set('limit', limit.toString());

    return this.http.get<{ suggestions: IngredientSuggestion[] }>(
      `${this.apiUrl}/ingredients/suggest`,
      { params }
    ).pipe(
      map(response => response.suggestions),
      tap(suggestions => {
        // Update cache
        this.suggestionCache.set(normalizedQuery, {
          suggestions,
          timestamp: Date.now()
        });
        // Prune old entries (keep cache size reasonable)
        this.pruneCache();
      })
    );
  }

  private pruneCache(): void {
    const now = Date.now();
    const keysToDelete: string[] = [];

    this.suggestionCache.forEach((value, key) => {
      if (now - value.timestamp >= this.CACHE_TTL_MS) {
        keysToDelete.push(key);
      }
    });

    keysToDelete.forEach(key => this.suggestionCache.delete(key));
  }

  clearSuggestionCache(): void {
    this.suggestionCache.clear();
  }

  // =========================================================================
  // User Pantry Management
  // =========================================================================

  getUserPantry(): Observable<UserPantryItem[]> {
    return this.http.get<{ items: UserPantryItem[] }>(`${this.apiUrl}/pantry`)
      .pipe(map(response => response.items));
  }

  addPantryItem(item: CreatePantryItem): Observable<UserPantryItem> {
    return this.http.post<UserPantryItem>(`${this.apiUrl}/pantry`, item);
  }

  updatePantryItem(id: number, item: UpdatePantryItem): Observable<UserPantryItem> {
    return this.http.put<UserPantryItem>(`${this.apiUrl}/pantry/${id}`, item);
  }

  deletePantryItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/pantry/${id}`);
  }

  getDefaultStaples(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/pantry/defaults`);
  }
}
```

#### ingredient-search.models.ts

```typescript
export interface IngredientSearchRequest {
  ingredients: string[];
  maxMissingIngredients: number;
  includePantryStaples: boolean;
  includeSubstitutions: boolean;
  maxCookTimeMinutes?: number;
  dietaryProfileResourceId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: 'matchPercentage' | 'rating' | 'cookTime';
}

export interface IngredientSearchResult {
  recipe: RecipeSummary;
  matchedIngredients: string[];
  missingIngredients: string[];
  pantryStaplesUsed: string[];
  matchPercentage: number;
  weightedMatchPercentage: number;
  substitutionsAvailable?: SubstitutionSuggestion[];
}

export interface RecipeSummary {
  recipeResourceId: string;
  title: string;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  originalImageUrl?: string;
  rating?: number;
  isFavorite?: boolean;
  isOwner?: boolean;
  tags?: RecipeTag[];
}

export interface SubstitutionSuggestion {
  recipeIngredient: string;
  possibleSubstitute: string;
}

export interface IngredientSuggestion {
  name: string;
  recipeCount: number;
}

export interface UserPantryItem {
  userPantryItemId?: number;
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface CreatePantryItem {
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface UpdatePantryItem {
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface RecipeTag {
  name: string;
  color?: string;
}
```

#### ingredient-search.component.ts

```typescript
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { debounceTime, switchMap, of } from 'rxjs';
import { IngredientSearchService } from './ingredient-search.service';
import {
  IngredientSearchResult,
  IngredientSuggestion,
  UserPantryItem,
  CreatePantryItem
} from './ingredient-search.models';

@Component({
  selector: 'app-ingredient-search',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule, MatAutocompleteModule,
    MatChipsModule, MatIconModule, MatButtonModule,
    MatSelectModule, MatCheckboxModule, MatExpansionModule,
    MatPaginatorModule, MatCardModule, MatProgressSpinnerModule,
    MatTabsModule, MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './ingredient-search.component.html',
  styleUrls: ['./ingredient-search.component.scss']
})
export class IngredientSearchComponent implements OnInit {
  private service = inject(IngredientSearchService);

  // Search Tab
  ingredientControl = new FormControl('');
  selectedIngredients = signal<string[]>([]);
  suggestions = signal<IngredientSuggestion[]>([]);
  results = signal<IngredientSearchResult[]>([]);
  loading = signal(false);

  // Filters
  maxMissing = signal(3);
  includePantryStaples = signal(true);
  includeSubstitutions = signal(false);
  maxCookTime = signal<number | null>(null);
  sortBy = signal<'matchPercentage' | 'rating' | 'cookTime'>('matchPercentage');

  // Pagination
  totalCount = signal(0);
  pageSize = signal(25);
  pageNumber = signal(1);

  // My Pantry Tab
  pantryItems = signal<UserPantryItem[]>([]);
  pantryLoading = signal(false);
  newPantryItemName = new FormControl('');
  newPantryItemIsStaple = signal(false);
  newPantryItemExpiration = signal<Date | null>(null);

  ngOnInit(): void {
    // Autocomplete with debounce and client-side caching
    this.ingredientControl.valueChanges.pipe(
      debounceTime(300),
      switchMap(query => query && query.length >= 2
        ? this.service.suggestIngredients(query)
        : of([]))
    ).subscribe(suggestions => this.suggestions.set(suggestions));

    // Load user pantry
    this.loadPantry();
  }

  // =========================================================================
  // Search Tab
  // =========================================================================

  addIngredient(name: string): void {
    const trimmed = name.trim().toLowerCase();
    if (trimmed && !this.selectedIngredients().includes(trimmed)) {
      this.selectedIngredients.update(items => [...items, trimmed]);
    }
    this.ingredientControl.setValue('');
    this.suggestions.set([]);
  }

  removeIngredient(name: string): void {
    this.selectedIngredients.update(items => items.filter(i => i !== name));
  }

  search(): void {
    if (this.selectedIngredients().length === 0) return;

    this.loading.set(true);
    this.service.searchByIngredients({
      ingredients: this.selectedIngredients(),
      maxMissingIngredients: this.maxMissing(),
      includePantryStaples: this.includePantryStaples(),
      includeSubstitutions: this.includeSubstitutions(),
      maxCookTimeMinutes: this.maxCookTime() ?? undefined,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy()
    }).subscribe({
      next: result => {
        this.results.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.search();
  }

  getMatchColor(percentage: number): string {
    if (percentage >= 75) return 'green';
    if (percentage >= 50) return 'orange';
    return 'red';
  }

  loadPantryIntoSearch(): void {
    const pantryIngredientNames = this.pantryItems().map(item => item.ingredientName);
    this.selectedIngredients.set([...pantryIngredientNames]);
  }

  // =========================================================================
  // My Pantry Tab
  // =========================================================================

  loadPantry(): void {
    this.pantryLoading.set(true);
    this.service.getUserPantry().subscribe({
      next: items => {
        this.pantryItems.set(items);
        this.pantryLoading.set(false);
      },
      error: () => this.pantryLoading.set(false)
    });
  }

  addPantryItem(): void {
    const name = this.newPantryItemName.value?.trim();
    if (!name) return;

    const newItem: CreatePantryItem = {
      ingredientName: name,
      isStaple: this.newPantryItemIsStaple(),
      expirationDate: this.newPantryItemExpiration()?.toISOString()
    };

    this.service.addPantryItem(newItem).subscribe({
      next: created => {
        this.pantryItems.update(items => [...items, created]);
        this.newPantryItemName.setValue('');
        this.newPantryItemIsStaple.set(false);
        this.newPantryItemExpiration.set(null);
      }
    });
  }

  deletePantryItem(item: UserPantryItem): void {
    if (!item.userPantryItemId) return;

    this.service.deletePantryItem(item.userPantryItemId).subscribe({
      next: () => {
        this.pantryItems.update(items =>
          items.filter(i => i.userPantryItemId !== item.userPantryItemId)
        );
      }
    });
  }

  toggleStaple(item: UserPantryItem): void {
    if (!item.userPantryItemId) return;

    const updated = { ...item, isStaple: !item.isStaple };
    this.service.updatePantryItem(item.userPantryItemId, updated).subscribe({
      next: result => {
        this.pantryItems.update(items =>
          items.map(i => i.userPantryItemId === result.userPantryItemId ? result : i)
        );
      }
    });
  }

  get pantryStaples(): UserPantryItem[] {
    return this.pantryItems().filter(item => item.isStaple);
  }

  get pantryIngredients(): UserPantryItem[] {
    return this.pantryItems().filter(item => !item.isStaple);
  }
}
```

#### ingredient-search.component.html (abbreviated)

```html
<div class="ingredient-search-container">
  <h1>What's in Your Fridge?</h1>

  <mat-tab-group>
    <!-- Search Tab -->
    <mat-tab label="Search">
      <div class="search-content">
        <!-- Autocomplete input -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Type an ingredient...</mat-label>
          <input matInput [formControl]="ingredientControl" [matAutocomplete]="auto" />
          <mat-autocomplete #auto="matAutocomplete" (optionSelected)="addIngredient($event.option.value)">
            @for (suggestion of suggestions(); track suggestion.name) {
              <mat-option [value]="suggestion.name">
                {{ suggestion.name }} ({{ suggestion.recipeCount }} recipes)
              </mat-option>
            }
          </mat-autocomplete>
        </mat-form-field>

        <!-- Selected ingredients chips -->
        <mat-chip-listbox class="ingredient-chips">
          @for (ingredient of selectedIngredients(); track ingredient) {
            <mat-chip (removed)="removeIngredient(ingredient)">
              {{ ingredient }}
              <button matChipRemove><mat-icon>cancel</mat-icon></button>
            </mat-chip>
          }
        </mat-chip-listbox>

        <!-- Filters -->
        <mat-expansion-panel>
          <mat-expansion-panel-header>Filters</mat-expansion-panel-header>
          <!-- Filter controls here -->
        </mat-expansion-panel>

        <!-- Search button -->
        <button mat-raised-button color="primary" (click)="search()" [disabled]="selectedIngredients().length === 0">
          Search Recipes
        </button>

        <!-- Load pantry button -->
        <button mat-button (click)="loadPantryIntoSearch()">
          Load my pantry into search
        </button>

        <!-- Results -->
        @if (loading()) {
          <mat-spinner></mat-spinner>
        } @else if (results().length > 0) {
          <div class="results-list">
            @for (result of results(); track result.recipe.recipeResourceId) {
              <mat-card class="result-card">
                <mat-card-header>
                  <mat-card-title>{{ result.recipe.title }}</mat-card-title>
                  <mat-card-subtitle>
                    <span [style.color]="getMatchColor(result.weightedMatchPercentage)">
                      {{ result.weightedMatchPercentage }}% match
                    </span>
                    · {{ result.recipe.totalTimeMinutes }} min
                  </mat-card-subtitle>
                </mat-card-header>
                <mat-card-content>
                  <!-- Matched/missing ingredients display -->
                </mat-card-content>
              </mat-card>
            }
          </div>

          <mat-paginator
            [length]="totalCount()"
            [pageSize]="pageSize()"
            [pageIndex]="pageNumber() - 1"
            (page)="onPageChange($event)">
          </mat-paginator>
        }
      </div>
    </mat-tab>

    <!-- My Pantry Tab -->
    <mat-tab label="My Pantry">
      <div class="pantry-content">
        <!-- Add pantry item form -->
        <!-- Pantry staples list -->
        <!-- Current ingredients list -->
      </div>
    </mat-tab>
  </mat-tab-group>
</div>
```

---

## Testing

### Backend Unit Tests

#### IngredientSearchService Tests

- **ScoreRecipesInDatabaseAsync**: Calls PostgreSQL function correctly, maps results to DTOs
- **EnsureUserPantrySeededAsync**: Seeds default staples on first access, skips seeding for existing users
- **FilterByDietaryProfileAsync**: Excludes recipes with avoided ingredients, retains compatible recipes
- **FindSubstitutionsAsync**: Returns viable substitutions when user has substitute ingredients, returns empty list when no substitutions available

#### UserPantryService Tests

- **GetUserPantryAsync**: Returns only current user's pantry items, excludes other users' items
- **AddPantryItemAsync**: Creates new pantry item with correct SubjectId, trims ingredient name
- **UpdatePantryItemAsync**: Updates existing item, rejects update for item belonging to different user
- **DeletePantryItemAsync**: Deletes item, rejects delete for item belonging to different user

#### IngredientSuggestService Tests

- **SuggestAsync with pg_trgm**: Uses trigram similarity search, orders by similarity then recipe count
- **SuggestAsync without pg_trgm**: Falls back to ILIKE prefix matching, orders by recipe count
- **CheckPgTrgmAvailableAsync**: Returns true when extension exists, returns false when extension missing

### PostgreSQL Function Tests

Execute these tests directly in PostgreSQL:

```sql
-- Test word-boundary matching: "rice" should NOT match "licorice"
SELECT * FROM score_recipes_by_ingredients(
    '00000000-0000-0000-0000-000000000001'::UUID,
    ARRAY['rice'],
    ARRAY[]::TEXT[]
);
-- Verify that recipes with only "licorice" are not matched

-- Test synonym expansion: "scallion" should match "green onion"
SELECT * FROM score_recipes_by_ingredients(
    '00000000-0000-0000-0000-000000000001'::UUID,
    ARRAY['scallion'],
    ARRAY[]::TEXT[]
);
-- Verify recipes with "green onion" are matched

-- Test ingredient weighting: primary ingredients count more
-- (Requires test data with varied ingredient positions and quantities)

-- Test pantry staples: excluded from match percentage calculation
SELECT * FROM score_recipes_by_ingredients(
    '00000000-0000-0000-0000-000000000001'::UUID,
    ARRAY['chicken breast'],
    ARRAY['salt', 'pepper']
);
-- Verify pantry items appear in pantry_staples_used, not in missing or matched
```

### Frontend Unit Tests

#### IngredientSearchService Tests

- **suggestIngredients**: Returns cached suggestions on cache hit, fetches from API on cache miss, prunes expired entries after fetching
- **clearSuggestionCache**: Clears all cached suggestions
- **getUserPantry**: Returns user's pantry items
- **addPantryItem**: Creates new pantry item, returns created item with ID

#### IngredientSearchComponent Tests

- **ngOnInit**: Triggers autocomplete after 300ms debounce, loads user pantry on initialization
- **addIngredient**: Adds ingredient to selected list, deduplicates silently, clears input field after adding
- **removeIngredient**: Removes ingredient from selected list
- **search**: Sends correct request payload, updates results and pagination state, disabled when no ingredients selected
- **loadPantryIntoSearch**: Copies all pantry ingredient names to selected ingredients
- **addPantryItem**: Calls service to add item, updates pantry list on success, clears form after adding
- **deletePantryItem**: Removes item from pantry list
- **toggleStaple**: Updates IsStaple flag, persists change via service

### Integration Tests

- **POST /api/v1/recipes/search/by-ingredients**: Returns scored results with matched/missing/pantry arrays, respects maxMissingIngredients filter, respects maxCookTimeMinutes filter
- **GET /api/v1/ingredients/suggest**: Returns autocomplete suggestions ordered by frequency, falls back to ILIKE when pg_trgm unavailable
- **GET /api/v1/pantry**: Returns only current user's pantry items
- **POST /api/v1/pantry**: Creates new pantry item, enforces unique constraint on (SubjectId, IngredientName)
- **DELETE /api/v1/pantry/{id}**: Deletes item, returns 404 for item belonging to different user
- **GET /api/v1/pantry/defaults**: Returns default staples list (unauthenticated)

---

## Edge Cases

1. **No ingredients entered**
   - Search button disabled
   - Show helper text: "Add at least one ingredient to search."

2. **All recipes filtered out**
   - Show message: "No recipes match your ingredients with current filters. Try increasing max missing ingredients or removing filters."

3. **Duplicate ingredient input**
   - Silently deduplicate on frontend
   - Backend treats case-insensitively

4. **Synonym collision**
   - User enters both "scallion" and "green onion"
   - Both resolve to same canonical name in PostgreSQL function
   - Counted as single ingredient in matching logic

5. **Recipe with no parseable ingredients**
   - PostgreSQL function filters out recipes where all `RecipeIngredient.Item` values are NULL or empty
   - Such recipes excluded from results

6. **Very common ingredients (pantry staples)**
   - Automatically excluded from match percentage calculation
   - Appear in `pantry_staples_used` array instead of `matched_ingredients`

7. **Plural/singular variations**
   - Handled via synonym table mappings (e.g., "tomato" ↔ "tomatoes")
   - Word-boundary matching also catches some cases ("tomato" matches "tomatoes" as a word token)

8. **Empty recipe ingredient list**
   - Recipes with zero ingredients excluded by PostgreSQL function
   - Division by zero avoided in percentage calculation

9. **Substring false positives**
   - Fixed by word-boundary matching
   - "rice" tokenized as `['rice']`
   - "licorice" tokenized as `['licorice']`
   - No word overlap → no match

10. **Very large ingredient lists**
    - Controller enforces 30-ingredient limit per search request
    - Returns 400 Bad Request if exceeded

11. **pg_trgm extension unavailable**
    - Migration detects availability and logs warning if missing
    - Autocomplete falls back to ILIKE prefix matching
    - Slightly worse fuzzy matching but feature still functional

12. **First-time user pantry access**
    - Service automatically seeds default staples on first search
    - User sees pre-populated pantry staples immediately
    - Can customize from there

13. **Expired pantry items**
    - `ExpirationDate` stored but not enforced by backend
    - Frontend can highlight/filter expired items
    - Future enhancement: exclude expired items from searches automatically

14. **Concurrent pantry edits**
    - Unique constraint on `(SubjectId, IngredientName)` prevents duplicate pantry items
    - Returns 409 Conflict if duplicate attempted

15. **Cache staleness (autocomplete)**
    - 5-minute TTL ensures suggestions don't drift too far from current data
    - User can refresh page to clear client cache
    - Service provides `clearSuggestionCache()` method for manual clearing

---

## Future Enhancements

1. **Barcode scanning (mobile)**
   - Use device camera to scan grocery item barcodes
   - Auto-add ingredients to pantry inventory

2. **Smart pantry suggestions**
   - "You have 4 of 5 ingredients for Chicken Alfredo — just need parmesan"
   - Proactive notifications when user is close to completing a recipe

3. **Seasonal ingredient highlighting**
   - Tag ingredients as in-season
   - Boost recipes using seasonal produce in ranking

4. **Cost estimation for missing ingredients**
   - Integrate with grocery price APIs
   - Show estimated cost to complete a recipe

5. **Shopping list generation**
   - Select multiple recipes from search results
   - Generate aggregated shopping list for all missing ingredients

6. **User-submitted synonym suggestions**
   - "Is 'cilantro' the same as 'coriander'? Suggest synonym"
   - Admin review and approval workflow

7. **Ingredient expiration alerts**
   - Push notifications when pantry items near expiration
   - Suggest recipes that use expiring ingredients

8. **Bulk pantry import**
   - Upload CSV of grocery purchases
   - Auto-populate pantry inventory

9. **Recipe difficulty scoring**
   - Combine ingredient match with recipe complexity
   - Prioritize simple recipes when user is missing ingredients

10. **Collaborative pantries**
    - Share pantry with household members
    - Everyone contributes to shared ingredient inventory

11. **Nutrition-aware search**
    - "Find high-protein recipes with chicken and broccoli"
    - Filter by macros, calories, dietary goals

12. **Voice input for ingredients**
    - "Add chicken, rice, and broccoli to my pantry"
    - Hands-free pantry management while cooking

13. **Image-based ingredient recognition**
    - Take photo of fridge contents
    - AI detects visible ingredients and adds to search

14. **Ingredient usage analytics**
    - "You use chicken breast in 80% of your recipes"
    - Suggest diversification or pantry optimization

15. **Recipe completion prediction**
    - ML model learns user preferences
    - Predicts likelihood of user enjoying a recipe based on past ratings + ingredient matches

---

## Word-Boundary Matching Algorithm

The core improvement over the previous design is **word-boundary matching** to eliminate false positives.

### Problem

Using `String.Contains` or SQL `LIKE '%rice%'` causes:
- "rice" matches "licorice" ❌
- "corn" matches "cornstarch" ❌
- "pea" matches "peanut butter" ❌

### Solution

Tokenize ingredient names into words, then match whole words only.

### PostgreSQL Implementation

```sql
-- Tokenize RecipeIngredient.Item into words
regexp_split_to_array(
    regexp_replace(LOWER(TRIM(ri."Item")), '[^a-z0-9\s]', ' ', 'g'),
    '\s+'
) AS item_words
```

This:
1. Converts to lowercase
2. Removes punctuation (replaces with spaces)
3. Splits on whitespace into array of words

**Example:**
- Input: `"2 cups long-grain white rice"`
- Normalized: `"long grain white rice"`
- Tokenized: `['long', 'grain', 'white', 'rice']`

### Matching Logic

```sql
WHEN EXISTS (
    SELECT 1 FROM all_available aa, UNNEST(rw.item_words) AS word
    WHERE aa.ingredient = word
) THEN 'matched'
```

User ingredient "rice" matches if `'rice'` appears as a **complete word** in the recipe ingredient's word array.

### Matching Hierarchy

1. **Exact match** — Entire `Item` field equals user ingredient (case-insensitive)
   - User: `"chicken breast"` → Recipe: `"chicken breast"` ✅

2. **Synonym match** — User ingredient resolves to canonical name that matches
   - User: `"scallion"` → Canonical: `"green onion"` → Recipe: `"green onion"` ✅

3. **Word-boundary match** — Any word in recipe item matches user ingredient
   - User: `"rice"` → Recipe: `"jasmine rice"` (words: `['jasmine', 'rice']`) ✅
   - User: `"rice"` → Recipe: `"licorice"` (words: `['licorice']`) ❌

4. **No match** — Ingredient goes to `missing` array

This hierarchy ensures high precision while maintaining flexibility.

---

## Ingredient Importance Weighting Algorithm

Not all ingredients are equal. Proteins and starches matter more than garnishes.

### Weighting Rules

```csharp
CASE
    -- Secondary ingredients (0.5x weight):
    WHEN "Notes" ILIKE '%to taste%' OR "Notes" ILIKE '%for garnish%' 
        OR "Notes" ILIKE '%optional%' OR "Quantity" < 0.25 THEN 0.5

    -- Primary ingredients (2.0x weight):
    WHEN ROW_NUMBER() OVER (PARTITION BY "RecipeId" ORDER BY "RecipeIngredientId") <= 4
        OR "Quantity" >= 2.0 THEN 2.0

    -- Default weight:
    ELSE 1.0
END AS weight
```

### Scoring

- **Simple match percentage**: `matched_count / (matched_count + missing_count)`
- **Weighted match percentage**: `weighted_matched / (weighted_matched + weighted_missing)`

### Example

Recipe: "Chicken Stir Fry"
- `chicken breast` — position 1, qty 1.5 lbs → weight 2.0
- `rice` — position 2, qty 2 cups → weight 2.0
- `broccoli` — position 3, qty 1 cup → weight 2.0
- `soy sauce` — position 4, qty 3 tbsp → weight 2.0
- `sesame oil` — position 5, qty 1 tsp → weight 1.0
- `salt` — notes "to taste" → weight 0.5

User has: `chicken breast`, `rice`, `broccoli`, `soy sauce`

**Simple match**: 4 matched / 5 missing (excluding salt as pantry) = 80%  
**Weighted match**: (2+2+2+2) / (2+2+2+2+1) = 8/9 = 88.9%

The weighted score better reflects that the user has all the primary ingredients.

---

## Deployment Checklist

- [ ] Run migration to create `UserPantryItem`, `IngredientSynonym` tables
- [ ] Seed 50+ synonym mappings
- [ ] Create `score_recipes_by_ingredients` PostgreSQL function
- [ ] Add GIN trigram index (or B-tree fallback)
- [ ] Deploy backend with new services, controllers, DTOs
- [ ] Deploy Angular frontend with new components, services, models
- [ ] Add "Ingredient Search" nav link to main menu
- [ ] Test word-boundary matching with "rice" query (should NOT match "licorice")
- [ ] Test synonym expansion with "scallion" query (should match "green onion")
- [ ] Test user pantry seeding on first access
- [ ] Test autocomplete caching (verify cache hits in browser devtools)
- [ ] Test pg_trgm fallback (disable extension temporarily, verify ILIKE fallback works)
- [ ] Load test: 1000 recipes, 20 ingredients → ensure response time < 500ms
- [ ] Monitor Sentry for any errors in ingredient search flow

---

**End of Document**
