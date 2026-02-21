# Tag Detail System Design (Updated)

## Overview

**Status:** Backend refactor complete. Frontend updates pending.

**Approach Change:** Instead of per-user aliases (UserTagAlias), we now use per-recipe details (RecipeTag.Detail). This better supports the use case where Source tags need different values per recipe (e.g., Chef="Bobby Flay" on one recipe, Chef="Gordon Ramsay" on another).

## Goals

1. Users can personalize tags ("Mom's" instead of "Family Recipe")
2. Public/shared recipes show normalized global tags
3. Searchable by both alias (for owner) and global name (for everyone)
4. Certain source types (Chef, Restaurant) can normalize to known entities
5. Minimal friction — aliasing is optional and inline

---

## Completed Backend Changes

### Migration Applied
- ✅ Added `Detail` (string 100), `NormalizedEntityId` (string 100), `NormalizedEntityType` (int?) to `RecipeTag`
- ✅ Dropped `UserTagAlias` table (never fully implemented)
- ✅ Migration: `20260221192539_AddRecipeTagDetail.cs`

### Updated Entities
- ✅ `RecipeTag` entity already had Detail/NormalizedEntity fields
- ✅ `Tag` entity has SourceType and IsSystemTag
- ✅ RecipeService handles Detail and Gemini normalization

### DTOs and Mappers
- ✅ RecipeTagDto includes detail, normalizedEntityId, normalizedEntityType
- ✅ TagMapper uses Detail as displayName (fallback to globalName)
- ✅ RecipeModelMapper updated to include new fields

### Tests
- ✅ Removed UserTagAlias references from test files
- ✅ All backend tests build successfully

## Data Model (Current)

```
Tag
├── TagId (PK, identity)
├── TagResourceId (GUID)
├── Name
├── Category (enum: Dietary, Source, Cuisine, MealType, Custom)
├── IsGlobal (bool)
└── [audit fields]

RecipeTag
├── RecipeTagId (PK, identity)
├── RecipeId (FK)
├── TagId (FK)
├── AssignedBySubjectId
├── IsAiAssigned
├── Confidence
├── IsOverridden
└── AssignedDate
```

### New Schema

```
Tag (modified)
├── TagId (PK, identity)
├── TagResourceId (GUID)
├── Name
├── Category (enum: Dietary, Source, Cuisine, MealType, Custom)
├── SourceType (nullable enum: Family, Chef, Restaurant, Cookbook, Website, Original)
│   └── Only populated when Category = Source
├── IsGlobal (bool) → DEPRECATED, migrate to IsSystemTag
├── IsSystemTag (bool) — curated global tags, non-deletable
└── [audit fields]

UserTagAlias (new)
├── UserTagAliasId (PK, identity)
├── UserId (FK to Subject)
├── TagId (FK to Tag)
├── Alias (string, 100 chars)
├── NormalizedEntityId (nullable string) — for known entities
├── NormalizedEntityType (nullable enum: Chef, Restaurant, Cookbook)
├── ShowAliasPublicly (bool, default false)
├── CreatedDate
└── LastModifiedDate

UNIQUE(UserId, TagId) — one alias per tag per user

RecipeTag (unchanged)
└── Still links RecipeId → TagId
    Alias resolved at read time via UserTagAlias
```

### Entity Normalization (Future Enhancement)

```
NormalizedEntity (future, optional)
├── EntityId (PK)
├── EntityType (Chef, Restaurant, Cookbook)
├── CanonicalName ("Bobby Flay")
├── ExternalId (nullable — Google Places ID, OpenLibrary ID, etc.)
└── Metadata (JSON — image, bio, etc.)
```

For MVP, `NormalizedEntityId` is a simple string (e.g., "bobby-flay") that Gemini generates. Full entity table can come later.

---

## Migration Plan

### Phase 1: Schema Changes

1. Add `SourceType` column to `Tag` table
2. Add `IsSystemTag` column to `Tag` table
3. Create `UserTagAlias` table
4. Backfill `IsSystemTag = true` for existing global tags
5. Backfill `SourceType` for existing Source category tags

```sql
-- Migration: AddUserTagAliases

ALTER TABLE "Tag" ADD COLUMN "SourceType" INTEGER NULL;
ALTER TABLE "Tag" ADD COLUMN "IsSystemTag" BOOLEAN NOT NULL DEFAULT false;

CREATE TABLE "UserTagAlias" (
    "UserTagAliasId" SERIAL PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "TagId" INTEGER NOT NULL REFERENCES "Tag"("TagId") ON DELETE CASCADE,
    "Alias" VARCHAR(100) NOT NULL,
    "NormalizedEntityId" VARCHAR(100) NULL,
    "NormalizedEntityType" INTEGER NULL,
    "ShowAliasPublicly" BOOLEAN NOT NULL DEFAULT false,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastModifiedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE("UserId", "TagId")
);

CREATE INDEX "IX_UserTagAlias_UserId" ON "UserTagAlias"("UserId");
CREATE INDEX "IX_UserTagAlias_TagId" ON "UserTagAlias"("TagId");

-- Backfill system tags
UPDATE "Tag" SET "IsSystemTag" = true WHERE "IsGlobal" = true;
```

### Phase 2: Seed System Tags

Curated global tags to seed:

**Source (with SourceType):**
- Family Recipe (SourceType: Family)
- Chef (SourceType: Chef)
- Restaurant (SourceType: Restaurant)
- Cookbook (SourceType: Cookbook)
- Website (SourceType: Website)
- Original Creation (SourceType: Original)

**Dietary (existing):**
- Vegetarian, Vegan, Gluten-Free, Dairy-Free, Nut-Free, Keto, Paleo, etc.

**Cuisine:**
- Italian, Mexican, Chinese, Japanese, Indian, Thai, French, American, Mediterranean, etc.

**MealType:**
- Breakfast, Lunch, Dinner, Snack, Dessert, Appetizer, Side Dish, etc.

### Phase 3: API Changes

See API section below.

### Phase 4: UI Changes

See UX section below.

---

## API Design

### Get Tags (updated)

```
GET /api/tags?category={category}
```

Response includes user's aliases:

```json
{
  "items": [
    {
      "tagResourceId": "...",
      "name": "Family Recipe",
      "category": "Source",
      "sourceType": "Family",
      "isSystemTag": true,
      "alias": "Mom's",           // null if no alias
      "showAliasPublicly": false
    }
  ]
}
```

### Set/Update Alias

```
PUT /api/tags/{tagResourceId}/alias
{
  "alias": "Bobby Flay",
  "showAliasPublicly": true
}
```

Response:
```json
{
  "tagResourceId": "...",
  "name": "Chef",
  "alias": "Bobby Flay",
  "normalizedEntityId": "bobby-flay",  // Gemini-generated if recognized
  "normalizedEntityType": "Chef",
  "showAliasPublicly": true
}
```

### Remove Alias

```
DELETE /api/tags/{tagResourceId}/alias
```

### Assign Tag to Recipe (updated)

```
POST /api/recipes/{recipeResourceId}/tags
{
  "tagResourceId": "...",        // Required: global tag
  "alias": "Grandma's"           // Optional: set alias inline
}
```

Or create alias inline if doesn't exist.

### Get Recipe (updated response)

```json
{
  "recipeResourceId": "...",
  "title": "Apple Pie",
  "tags": [
    {
      "tagResourceId": "...",
      "globalName": "Family Recipe",
      "displayName": "Mom's",      // Alias if owner, global if viewer
      "category": "Source",
      "sourceType": "Family",
      "isOwnerAlias": true         // Helps UI decide styling
    }
  ]
}
```

Display logic:
- **Viewing own recipe**: Show alias (displayName = alias ?? globalName)
- **Viewing others' public recipe**: Show global (displayName = globalName)
- **Unless** `showAliasPublicly = true`: Show both or alias

---

## UX Design (Option A)

### Adding a Tag to a Recipe

```
┌─────────────────────────────────────────────┐
│  Tags                              [+ Add]  │
├─────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐         │
│  │ 🍽 Dinner    │  │ 🌿 Vegetarian│         │
│  └──────────────┘  └──────────────┘         │
└─────────────────────────────────────────────┘
```

**Step 1: Click [+ Add]**

```
┌─────────────────────────────────────────────┐
│  🔍 Search tags...                          │
├─────────────────────────────────────────────┤
│  SOURCE                                     │
│    Family Recipe                            │
│    Chef                                     │
│    Restaurant                               │
│    Cookbook                                 │
│  ─────────────────                          │
│  CUISINE                                    │
│    Italian                                  │
│    Mexican                                  │
│    ...                                      │
└─────────────────────────────────────────────┘
```

**Step 2: Select "Family Recipe"**

Tag added immediately. Inline alias prompt appears:

```
┌─────────────────────────────────────────────┐
│  Tags                              [+ Add]  │
├─────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐         │
│  │ 🍽 Dinner    │  │ 🌿 Vegetarian│         │
│  └──────────────┘  └──────────────┘         │
│                                             │
│  ┌────────────────────────────────────────┐ │
│  │ 📁 Family Recipe  [+ Add name]         │ │
│  └────────────────────────────────────────┘ │
│           ↑ newly added, highlighted        │
└─────────────────────────────────────────────┘
```

**Step 3a: Click away** → No alias, shows "Family Recipe"

**Step 3b: Click [+ Add name]** → Inline edit mode:

```
│  ┌────────────────────────────────────────┐ │
│  │ 📁 Family Recipe › [Mom's____] ✓ ✗    │ │
│  └────────────────────────────────────────┘ │
```

**Result:**

```
│  ┌──────────────────────┐                   │
│  │ 📁 Mom's             │  (hover: Family Recipe)
│  └──────────────────────┘                   │
```

### Managing Aliases (Settings or Tag Manager)

```
┌─────────────────────────────────────────────┐
│  My Tag Aliases                             │
├─────────────────────────────────────────────┤
│  Family Recipe  →  "Mom's"         [Edit]   │
│  Chef           →  "Bobby Flay"    [Edit]   │
│                    ☑ Show publicly          │
│  Restaurant     →  (none)          [+ Add]  │
└─────────────────────────────────────────────┘
```

---

## Search Behavior

| User | Query | Matches |
|------|-------|---------|
| Owner | "Mom's" | Own recipes with that alias |
| Owner | "Family Recipe" | Own recipes with that global tag |
| Anyone | "Family Recipe" | All public recipes with tag |
| Anyone | "Bobby Flay" | Public recipes where alias is public OR normalized entity matches |

### Implementation

```sql
-- Owner searching their own recipes
WHERE (
  t."Name" ILIKE @query 
  OR uta."Alias" ILIKE @query
)
AND r."CreatedSubjectId" = @currentUserId

-- Public search
WHERE (
  t."Name" ILIKE @query 
  OR (uta."Alias" ILIKE @query AND uta."ShowAliasPublicly" = true)
  OR uta."NormalizedEntityId" = @normalizedQuery
)
AND r."IsPublic" = true
```

---

## Gemini Integration

When user sets an alias for Chef/Restaurant/Cookbook:

1. Call Gemini to check if it's a known entity
2. If recognized, set `NormalizedEntityId` and `NormalizedEntityType`
3. Store canonical form for consistent matching

```csharp
public async Task<NormalizationResult> NormalizeAliasAsync(string alias, SourceType sourceType)
{
    var prompt = sourceType switch
    {
        SourceType.Chef => $"Is '{alias}' a known chef? If yes, return their canonical name in slug form.",
        SourceType.Restaurant => $"Is '{alias}' a known restaurant chain? If yes, return canonical name.",
        SourceType.Cookbook => $"Is '{alias}' a known cookbook? If yes, return title and author.",
        _ => null
    };
    
    if (prompt == null) return null;
    
    // Call Gemini, parse response
    // Return { EntityId: "bobby-flay", EntityType: Chef } or null
}
```

---

## Implementation Phases

### Phase 1: Foundation
Add database migration (new columns + UserTagAlias table), create UserTagAlias entity and repository, update Tag entity with SourceType/IsSystemTag, seed system tags, implement basic CRUD for aliases.

### Phase 2: API  
Update tag DTOs with alias fields, add alias endpoints (PUT/DELETE), update recipe tag assignment to support inline alias creation, update recipe GET to return display names resolved by viewer context.

### Phase 3: UI
Build tag picker with grouped system tags, implement inline alias prompt on tag add, update tag pill display with alias and hover for global name, create tag management page for viewing/editing aliases.

### Phase 4: Search & Polish (✅ Complete)
Recipe search and tag search now include alias matching. Owner can search by their own aliases; public searches match aliases with ShowAliasPublicly=true or NormalizedEntityId. Gemini normalization integrated for Chef/Restaurant/Cookbook source types—when user sets alias on these tags, system calls Gemini to check if it's a known entity and sets NormalizedEntityId/NormalizedEntityType if recognized. Tests added for entity normalization.

---

## Design Decisions

1. **Custom tags**: Users can still create fully custom tags (Custom category). UI should include non-invasive helper text showing global tag usage to nudge users toward aliasing globals when appropriate.

2. **Alias deletion**: When a user deletes an alias, recipes retain the global tag — only the personalized display name is removed.

3. **Cross-user alias suggestions**: Future enhancement, not MVP. Could show "10 other users call this 'Bobby Flay'" to help normalization organically.

4. **Gemini normalization**: Only call on save (not keystroke). Cache normalized results on the alias record to avoid redundant API calls.

---

## Migration Strategy

### Backfilling Existing Tags

Existing user-created tags need migration to the new schema. Strategy:

1. **Identify mappable tags**: Query existing non-global tags and attempt to match to system tags via:
   - Exact name match (case-insensitive)
   - Gemini-assisted fuzzy matching for near-matches ("Mom's Recipes" → "Family Recipe")

2. **Migration script logic**:
   ```
   FOR each user-created Tag:
     IF exact match to system tag name:
       → Create UserTagAlias(userId, systemTagId, alias=originalName)
       → Update RecipeTags to point to systemTagId
       → Delete original Tag
     ELIF Gemini suggests confident match:
       → Same as above
     ELSE:
       → Keep as Custom category tag (no change)
   ```

3. **Run in batches**: Process tags in batches to avoid Gemini rate limits.

4. **Audit log**: Track all migrations for rollback capability.

### Example Migration Cases

| Original Tag | Created By | Migration Result |
|-------------|-----------|------------------|
| "Mom's" | User A | → Alias of "Family Recipe" |
| "Bobby Flay" | User B | → Alias of "Chef" + NormalizedEntityId |
| "Weeknight Easy" | User C | → Keep as Custom tag |
| "Vegetarian" | User D | → Point to system "Vegetarian" (dietary) |

---

## Files to Modify

### Backend
- `RecipeVault.Domain/Entities/Tag.cs` — add SourceType, IsSystemTag
- `RecipeVault.Domain/Entities/UserTagAlias.cs` — new entity
- `RecipeVault.Domain/Enums/SourceType.cs` — new enum
- `RecipeVault.Data/RecipeVaultDbContext.cs` — add DbSet, configure
- `RecipeVault.Data/Repositories/TagRepository.cs` — alias queries
- `RecipeVault.DomainService/TagService.cs` — alias CRUD
- `RecipeVault.Dto/` — update DTOs
- `RecipeVault.WebApi/Controllers/TagsController.cs` — new endpoints

### Frontend
- Tag picker component
- Tag pill component (with alias display)
- Recipe edit form (tag section)
- Settings/tag management page

### Database
- Migration for schema changes
- Seed script for system tags
