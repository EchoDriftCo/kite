# Recipe App Design Document

**Project Codename:** RecipeVault  
**Version:** 0.1  
**Last Updated:** 2026-02-11

---

## Strategic Vision

**North Star:** "The Recipe App That Understands Food"

This is not a recipe digitization tool. It is a dietary intelligence platform that automatically infers allergens, nutritional implications, and substitutions from recipe text.

---

## Phase 1: Multimodal AI Scanner (MVP)

### Scope

Build the core recipe ingestion pipeline that transforms images of recipes into structured, normalized data.

### Features In Scope

1. **Image Upload** - Accept photo of recipe (card, book page, screenshot)
2. **AI Parsing** - Use multimodal LLM to extract and structure:
   - Title
   - Yield (normalized to servings)
   - Prep Time (normalized to minutes)
   - Cook Time (normalized to minutes)
   - Ingredients (parsed into: Quantity | Unit | Item | Preparation Notes)
   - Instructions (broken into discrete steps)
   - Source (if visible)
3. **Recipe CRUD** - Basic create, read, update, delete operations
4. **Manual Edit** - User can correct AI parsing errors

### Features OUT of Scope (Future Phases)

- User tagging and recipe categorization (Phase 2)
- Dietary auto-tagging and allergen inference (Phase 3)
- Recipe forking from public recipes (Phase 4)
- Photo cleanup/de-warping (Future)
- Video/TikTok import (Future)

---

## Technology Stack

### Backend

| Component | Technology | Notes |
|-----------|------------|-------|
| Framework | .NET 8 | LTS release |
| API Style | REST | Following Cortside patterns |
| ORM | Entity Framework Core | Code-first migrations |
| Database | PostgreSQL | Supabase free tier (500MB) |
| File Storage | Supabase Storage | 1GB free tier for recipe images |
| AI Provider | Google Gemini 1.5 Flash | 15 req/min, 1M tokens/month free |

### Frontend

| Component | Technology | Notes |
|-----------|------------|-------|
| Framework | Angular 19 | Standalone components |
| UI Library | Angular Material | Dark theme, Material Design 3 |
| State | Signals | Angular's native reactivity |

### Architecture Reference

**Follow patterns from:** `cortside/coeus/shoppingcart-api`

Key conventions to match:
- Solution structure: `{Product}.{Layer}`
- Layers: Configuration, BootStrap, Domain, DomainService, Data, Dto, Exceptions, Facade, Health, WebApi, WebApi.IntegrationTests
- Domain entities with AuditableEntity base
- Dual-key pattern: integer PK (internal) + Guid ResourceId (API-facing)
- Repository pattern with IRepository<T>
- DomainService layer for business logic between Domain and Facade
- Dto project with Input/, Output/, Search/ separation
- Facade layer for orchestration and unit of work
- BootStrap project with Installer pattern for DI registration
- Domain events via cortside.domainevent (RecipeCreatedEvent, no async handlers in Phase 1)
- Authentication via Supabase Auth JWT bearer tokens
- Three-layer mapping: API Model ↔ DTO ↔ Entity

### Request Flow

```
HTTP Request
    ↓
Controller [Authorize] ← validates Supabase JWT
    ↓ WebApi Mapper
API Request Model (CreateRecipeModel)
    ↓
Dto (CreateRecipeDto)
    ↓
Facade (IRecipeFacade) ← UnitOfWork / SaveChangesAsync
    ↓
DomainService (IRecipeService) ← business logic
    ↓
Repository (IRecipeRepository)
    ↓
Entity (Recipe) ← domain rules, AuditableEntity
    ↓ Facade Mapper
Dto (RecipeDto)
    ↓ WebApi Mapper
API Response Model (RecipeModel)
    ↓
HTTP Response
```

---

## Domain Model

### Core Entities

```
Recipe (Aggregate Root)
├── RecipeId (int, DB primary key - internal only)
├── RecipeResourceId (Guid, API-facing identifier)
├── Title (string, required)
├── Description (string, optional)
├── Yield (int, servings)
├── PrepTimeMinutes (int, nullable)
├── CookTimeMinutes (int, nullable)
├── TotalTimeMinutes (computed)
├── Source (string, optional - URL or "Grandma's cookbook")
├── OriginalImageUrl (string, optional - preserved nostalgia image)
├── Ingredients (List<RecipeIngredient>)
├── Instructions (List<RecipeInstruction>)
├── CreatedDate (AuditableEntity)
├── LastModifiedDate (AuditableEntity)
└── CreatedBySubjectId (AuditableEntity)

RecipeIngredient
├── RecipeIngredientId (int, DB primary key)
├── RecipeId (int, FK)
├── SortOrder (int)
├── Quantity (decimal, nullable - "pinch" has no quantity)
├── Unit (string, nullable - normalized: "cup", "tbsp", "g", etc.)
├── Item (string, required - "all-purpose flour")
├── Preparation (string, optional - "sifted", "room temperature")
└── RawText (string - original parsed text for reference)

RecipeInstruction
├── RecipeInstructionId (int, DB primary key)
├── RecipeId (int, FK)
├── StepNumber (int)
├── Instruction (string)
└── RawText (string - original parsed text)
```

### Sharing & Visibility

```
Recipe (extended)
├── IsPublic (bool, default false)
└── Visibility toggled via PUT /api/v1/recipes/{id}/visibility
```

- Public recipes are readable by any authenticated user
- Only the owner can update, delete, or toggle visibility
- Search supports `IncludePublic` flag to show public recipes alongside owned ones
- Response includes `IsOwner` flag for UI to conditionally render edit/delete controls

### Meal Planning

```
MealPlan (Aggregate Root)
├── MealPlanId (int, DB primary key)
├── MealPlanResourceId (Guid, API-facing)
├── Name (string)
├── StartDate (DateTime)
├── EndDate (DateTime)
├── Entries (List<MealPlanEntry>)
└── CreatedBySubjectId (AuditableEntity)

MealPlanEntry
├── MealPlanEntryId (int, DB primary key)
├── MealPlanId (int, FK)
├── Date (DateTime)
├── MealSlot (enum: Breakfast, Lunch, Dinner, Snack)
├── RecipeId (int, FK → Recipe)
├── Recipe (navigation property)
├── Servings (int?, override for scaling)
└── IsLeftover (bool)
```

- Grocery list generation aggregates ingredients across non-leftover entries
- Scales quantities by `entry.Servings / recipe.Yield`
- AI-powered consolidation merges similar ingredients (e.g., "cooking oil" + "oil") via Gemini

### User Tags (Phase 2)

```
Tag
├── TagId (int, DB primary key)
├── TagResourceId (Guid, API-facing)
├── Name (string, required - "Quick Weeknight", "Holiday", "Kid-Friendly")
├── Color (string, optional - hex color for UI display)
└── CreatedBySubjectId (AuditableEntity)

RecipeTag (join entity)
├── RecipeTagId (int, DB primary key)
├── RecipeId (int, FK → Recipe)
├── TagId (int, FK → Tag)
└── CreatedDate
```

- Tags are user-scoped — each user manages their own tag vocabulary
- Recipes can have multiple tags, tags can apply to multiple recipes
- Search supports filtering by tag(s)
- Predefined "starter" tags are not seeded — users create their own organically

### Dietary Intelligence (Phase 3)

```
DietaryFlag (enum)
├── Vegan, Vegetarian, GlutenFree, DairyFree, NutFree, Shellfish
├── LowCarb, HighProtein, Keto, Paleo
└── (extensible - new values added without migration)

RecipeDietaryFlag
├── RecipeDietaryFlagId (int, DB primary key)
├── RecipeId (int, FK → Recipe)
├── Flag (DietaryFlag enum, stored as string)
├── Confidence (decimal, 0-1 - AI confidence score)
├── IsOverridden (bool - user manually corrected)
└── CreatedDate
```

- AI auto-analyzes ingredients on recipe create/update
- Flags inferred from ingredient list (e.g., no dairy items → DairyFree)
- Users can override AI flags (mark/unmark) — overrides are preserved across re-analysis
- Allergen warnings surfaced on recipe detail and meal plan entries
- Substitution suggestions: AI recommends ingredient swaps for dietary goals

### Recipe Forking (Phase 4)

```
Recipe (extended)
├── ForkedFromRecipeId (int?, FK → Recipe, nullable)
├── ForkedFromRecipe (navigation property)
└── ForkCount (int, computed or denormalized)
```

- Fork creates a full copy of a public recipe into the user's collection
- Fork preserves attribution: "Forked from {original title} by {original author}"
- Forked recipe is fully editable — changes don't affect the original
- Original recipe tracks fork count for social proof
- Forking a private recipe is not allowed — only public recipes can be forked

### Data Ownership

Recipes are user-scoped via `CreatedBySubjectId`, populated automatically by `AuditableEntity` from the Supabase Auth JWT subject claim.

- Private recipes: only the owner can read, update, or delete
- Public recipes: any authenticated user can read; only the owner can update/delete
- Meal plans are strictly user-scoped — no sharing

---

## API Endpoints

### Recipes

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/recipes/parse` | Upload image, returns parsed recipe (not saved) |
| POST | `/api/v1/recipes` | Create recipe (from parsed or manual entry) |
| GET | `/api/v1/recipes` | List recipes (paginated, supports `includePublic` filter) |
| GET | `/api/v1/recipes/{id}` | Get single recipe (owner or public) |
| PUT | `/api/v1/recipes/{id}` | Update recipe (owner only) |
| DELETE | `/api/v1/recipes/{id}` | Delete recipe (owner only) |
| PUT | `/api/v1/recipes/{id}/visibility` | Toggle public/private (owner only) |
| POST | `/api/v1/recipes/{id}/fork` | Fork a public recipe into user's collection (Phase 4) |

### Meal Plans

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/meal-plans` | Create meal plan |
| GET | `/api/v1/meal-plans` | List meal plans (paginated, user-scoped) |
| GET | `/api/v1/meal-plans/{id}` | Get single meal plan |
| PUT | `/api/v1/meal-plans/{id}` | Update meal plan (name, dates, entries) |
| DELETE | `/api/v1/meal-plans/{id}` | Delete meal plan |
| GET | `/api/v1/meal-plans/{id}/grocery-list` | Generate AI-consolidated grocery list |

### Tags (Phase 2)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/tags` | Create tag |
| GET | `/api/v1/tags` | List user's tags |
| PUT | `/api/v1/tags/{id}` | Update tag (name, color) |
| DELETE | `/api/v1/tags/{id}` | Delete tag (removes from all recipes) |
| PUT | `/api/v1/recipes/{id}/tags` | Set tags for a recipe (replace all) |

### Dietary Analysis (Phase 3)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/recipes/{id}/dietary` | Get dietary flags for a recipe |
| PUT | `/api/v1/recipes/{id}/dietary/{flag}` | Override a dietary flag (user correction) |
| POST | `/api/v1/recipes/{id}/dietary/analyze` | Re-trigger AI analysis |
| GET | `/api/v1/recipes/{id}/substitutions` | Get AI substitution suggestions for dietary goals |

### Parse Endpoint Detail

**POST /api/v1/recipes/parse**

Request:
```json
{
  "image": "base64-encoded-image-data",
  "mimeType": "image/jpeg"
}
```

Response:
```json
{
  "confidence": 0.92,
  "parsed": {
    "title": "Grandma's Apple Pie",
    "yield": 8,
    "prepTimeMinutes": 30,
    "cookTimeMinutes": 45,
    "ingredients": [
      {
        "quantity": 2.5,
        "unit": "cup",
        "item": "all-purpose flour",
        "preparation": "sifted",
        "rawText": "2 1/2 cups all-purpose flour, sifted"
      }
    ],
    "instructions": [
      {
        "stepNumber": 1,
        "instruction": "Preheat oven to 375°F.",
        "rawText": "Preheat oven to 375°F."
      }
    ]
  },
  "warnings": [
    "Could not determine prep time - please verify"
  ]
}
```

---

## AI Integration

### Gemini Prompt Strategy

The parse endpoint will use a structured prompt with JSON schema output:

```
You are a recipe parser. Extract structured data from this recipe image.

Return ONLY valid JSON matching this schema:
{
  "title": "string",
  "yield": number or null,
  "prepTimeMinutes": number or null,
  "cookTimeMinutes": number or null,
  "ingredients": [
    {
      "quantity": number or null,
      "unit": "string or null (normalize to: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)",
      "item": "string (the ingredient name)",
      "preparation": "string or null (e.g., chopped, melted, room temperature)",
      "rawText": "string (exact text from image)"
    }
  ],
  "instructions": [
    {
      "stepNumber": number,
      "instruction": "string (cleaned up instruction)",
      "rawText": "string (exact text from image)"
    }
  ]
}

Rules:
- Normalize units (tablespoon → tbsp, teaspoon → tsp)
- Convert fractions to decimals (1/2 → 0.5)
- Separate preparation notes from ingredient names
- Number instructions sequentially even if source doesn't
- If information is unclear, use null rather than guessing
```

### Unit Normalization

Unit normalization is handled by the AI parser. The Gemini prompt instructs the model to output all ingredient units in a fixed canonical set: `cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash`.

Manual entry and corrections validate against this same set. No third-party library is needed for Phase 1 — the canonical set is small and fixed. If unit conversion (e.g. for recipe scaling) becomes necessary in a later phase, revisit then.

### Error Handling

- **No recipe detected:** Return 422 with message "Could not identify a recipe in this image"
- **Partial parse:** Return 200 with warnings array listing uncertain fields
- **API failure:** Return 503 with retry guidance

**Important:** Never write fallback/safety code that silently handles errors. Missing configuration or incomplete data is an error.

---

## Project Structure

```
src/
├── RecipeVault.Configuration/
│   └── SupabaseConfiguration.cs
├── RecipeVault.BootStrap/
│   └── Installer/
│       ├── DomainServiceInstaller.cs
│       ├── RepositoryInstaller.cs
│       └── FacadeInstaller.cs
├── RecipeVault.Domain/
│   ├── Entities/
│   │   ├── Recipe.cs
│   │   ├── RecipeIngredient.cs
│   │   ├── RecipeInstruction.cs
│   │   ├── MealPlan.cs
│   │   └── MealPlanEntry.cs
│   ├── Enums/
│   │   └── MealSlot.cs
│   └── Events/
│       └── RecipeCreatedEvent.cs
├── RecipeVault.DomainService/
│   ├── IRecipeService.cs
│   ├── RecipeService.cs
│   ├── IMealPlanService.cs
│   └── MealPlanService.cs
├── RecipeVault.Exceptions/
│   ├── RecipeNotFoundException.cs
│   └── MealPlanNotFoundException.cs
├── RecipeVault.Data/
│   ├── RecipeVaultDbContext.cs
│   ├── IRecipeVaultDbContext.cs
│   ├── Repositories/
│   │   ├── IRecipeRepository.cs
│   │   ├── RecipeRepository.cs
│   │   ├── IMealPlanRepository.cs
│   │   └── MealPlanRepository.cs
│   ├── Searches/
│   │   ├── IRecipeSearch.cs
│   │   ├── RecipeSearch.cs
│   │   ├── IMealPlanSearch.cs
│   │   └── MealPlanSearch.cs
│   └── Migrations/
├── RecipeVault.Dto/
│   ├── Input/
│   │   ├── UpdateRecipeDto.cs
│   │   ├── UpdateMealPlanDto.cs
│   │   └── ParseRecipeRequestDto.cs
│   ├── Output/
│   │   ├── RecipeDto.cs
│   │   ├── RecipeIngredientDto.cs
│   │   ├── RecipeInstructionDto.cs
│   │   ├── ParseRecipeResponseDto.cs
│   │   ├── MealPlanDto.cs
│   │   └── GroceryListDto.cs
│   └── Search/
│       ├── RecipeSearchDto.cs
│       └── MealPlanSearchDto.cs
├── RecipeVault.Facade/
│   ├── IRecipeFacade.cs
│   ├── RecipeFacade.cs
│   ├── IMealPlanFacade.cs
│   ├── MealPlanFacade.cs
│   └── Mappers/
│       ├── RecipeMapper.cs
│       └── MealPlanMapper.cs
├── RecipeVault.Integrations.Gemini/
│   ├── IGeminiClient.cs
│   ├── GeminiClient.cs
│   └── Models/
│       └── GeminiApiModels.cs
├── RecipeVault.Health/
│   └── GeminiHealthCheck.cs
├── RecipeVault.WebApi/
│   ├── Controllers/
│   │   ├── RecipesController.cs
│   │   └── MealPlansController.cs
│   ├── Models/
│   │   ├── Requests/
│   │   │   ├── UpdateRecipeModel.cs
│   │   │   ├── UpdateMealPlanModel.cs
│   │   │   ├── RecipeSearchModel.cs
│   │   │   ├── MealPlanSearchModel.cs
│   │   │   ├── SetVisibilityModel.cs
│   │   │   └── ParseRecipeRequestModel.cs
│   │   └── Responses/
│   │       ├── RecipeModel.cs
│   │       ├── MealPlanModel.cs
│   │       ├── GroceryListModel.cs
│   │       └── ParseRecipeResponseModel.cs
│   ├── Mappers/
│   │   ├── RecipeModelMapper.cs
│   │   └── MealPlanModelMapper.cs
│   ├── Program.cs
│   ├── Startup.cs
│   └── appsettings.json
├── RecipeVault.TestUtilities/
│   └── Builders/
│       ├── RecipeBuilder.cs
│       └── MealPlanBuilder.cs
├── RecipeVault.DomainService.Tests/
├── RecipeVault.Facade.Tests/
└── RecipeVault.WebApi.IntegrationTests/
```

---

## Configuration

Required settings (appsettings.json):

```json
{
  "Service": {
    "Name": "recipevault-api"
  },
  "Database": {
    "ConnectionString": "Host=;Database=;Username=;Password="
  },
  "Supabase": {
    "Url": "",
    "ServiceKey": "",
    "StorageBucket": "recipe-images",
    "Auth": {
      "JwtSecret": "",
      "Issuer": "",
      "Audience": "supabase"
    }
  },
  "Gemini": {
    "ApiKey": "",
    "Model": "gemini-1.5-flash"
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "Enrich": ["FromLogContext"],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

**Missing configuration must throw on startup, not at runtime.**

---

## Development Phases

### Phase 1: Foundation (Complete)
- [x] Solution scaffolding (Cortside template)
- [x] Domain entities (Recipe, RecipeIngredient, RecipeInstruction)
- [x] Database context + migrations (PostgreSQL via Supabase)
- [x] Supabase storage integration (recipe images)
- [x] Gemini integration service (image parsing)
- [x] Parse endpoint (multimodal AI recipe extraction)
- [x] Recipe CRUD endpoints with ownership enforcement
- [x] Angular UI (upload, list, detail, edit)
- [x] Public/private recipe sharing with visibility toggle
- [x] Meal planning with weekly calendar view
- [x] AI-powered grocery list generation with ingredient consolidation

### Phase 2: User Tagging & Organization
- [ ] Tag entity + migration (user-scoped tags with optional color)
- [ ] RecipeTag join entity + migration
- [ ] Tag CRUD endpoints
- [ ] Recipe-tag assignment endpoint
- [ ] Search filtering by tag(s)
- [ ] Tag management UI (create, edit, delete, color picker)
- [ ] Tag chips on recipe cards and detail view
- [ ] Tag filter in recipe list sidebar/toolbar
- [ ] Bulk tag operations (tag multiple recipes at once)

### Phase 3: Dietary Intelligence
- [ ] DietaryFlag enum + RecipeDietaryFlag entity + migration
- [ ] AI analysis service (infer dietary flags from ingredient list via Gemini)
- [ ] Auto-analyze on recipe create/update
- [ ] Dietary flags display on recipe detail (badges/chips)
- [ ] User override capability (correct AI mistakes, persisted across re-analysis)
- [ ] Allergen warnings on meal plan entries
- [ ] Dietary filter in recipe search (e.g., show only Vegan, GlutenFree)
- [ ] AI substitution suggestions (swap ingredients for dietary goals)
- [ ] Dietary summary on meal plan (weekly nutritional balance view)

### Phase 4: Recipe Forking
- [ ] ForkedFromRecipeId field + migration
- [ ] Fork endpoint (deep-copy public recipe into user's collection)
- [ ] Fork attribution display ("Forked from X by Y")
- [ ] Fork count on public recipe detail (social proof)
- [ ] Fork button on public recipe detail view
- [ ] "Forked recipes" filter/tab in recipe list
- [ ] Diff view (optional: show changes from original)

### Future Ideas (Unscheduled)
- Image cleanup/de-warping (straighten, adjust brightness/contrast)
- Video/TikTok import (share extension, audio transcription, caption extraction)
- Nutritional data integration (USDA/OpenFoodFacts API for calorie/macro estimation)
- Social features (follow users, activity feed, recipe ratings)
- Collections/cookbooks (group recipes beyond tags)
- Recipe scaling calculator (adjust servings with unit conversion)
- Cooking mode (step-by-step guided view with timers)

---

## Constraints & Principles

1. **Errors are errors.** No silent fallbacks. Missing config = startup failure. Bad data = 4xx/5xx response. Exception: AI-enhanced features (grocery consolidation, dietary analysis) gracefully degrade — return raw data with a warning log if AI fails.

2. **Follow Cortside patterns.** Reference `cortside/coeus/shoppingcart-api` for architectural decisions.

3. **Ownership first.** All user data is scoped by `CreatedBySubjectId`. Public visibility is opt-in. Users can never modify another user's data.

4. **Free tier friendly.** Design for Supabase/Gemini free limits. Batch AI calls where possible. Cache results when practical.

5. **AI is fallible.** Always preserve raw text. Always allow manual correction. AI-inferred data (dietary flags, ingredient parsing) must be overridable by the user.

6. **Phase discipline.** Complete the current phase before starting the next. Each phase should be independently shippable.

---

## References

- Cortside templates: `cortside/coeus/shoppingcart-api`
- Gemini API: https://ai.google.dev/docs
- Supabase .NET: https://github.com/supabase-community/supabase-csharp
