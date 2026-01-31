# Recipe App Design Document

**Project Codename:** RecipeVault  
**Version:** 0.1  
**Last Updated:** 2025-01-31

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

- Photo cleanup/de-warping (Phase 1b)
- Video/TikTok import (Phase 1c)
- Dietary auto-tagging (Phase 2)
- Substitution suggestions (Phase 2)
- Sharing/collaboration (Phase 3)
- Meal planning (Phase 3)

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
| Framework | Angular 17+ | Standalone components |
| UI Library | TBD | |
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

### Value Objects (Future - Phase 2)

```
NormalizedIngredient - maps "flour" to canonical ingredient for dietary analysis
DietaryTag - Vegan, GlutenFree, DairyFree, etc.
```

### Data Ownership

Recipes are user-scoped via `CreatedBySubjectId`, populated automatically by `AuditableEntity` from the Supabase Auth JWT subject claim.

- All recipe queries are filtered to the authenticated user's subject ID
- Supabase Row Level Security (RLS) policies enforce ownership at the database level
- Users cannot read, update, or delete recipes belonging to other users

---

## API Endpoints

### Recipes

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/recipes/parse` | Upload image, returns parsed recipe (not saved) |
| POST | `/api/v1/recipes` | Create recipe (from parsed or manual entry) |
| GET | `/api/v1/recipes` | List recipes (paginated) |
| GET | `/api/v1/recipes/{id}` | Get single recipe |
| PUT | `/api/v1/recipes/{id}` | Update recipe |
| DELETE | `/api/v1/recipes/{id}` | Delete recipe |

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
│   │   └── RecipeInstruction.cs
│   └── Events/
│       └── RecipeCreatedEvent.cs
├── RecipeVault.DomainService/
│   ├── IRecipeService.cs
│   └── RecipeService.cs
├── RecipeVault.Exceptions/
│   └── RecipeNotFoundException.cs
├── RecipeVault.Data/
│   ├── RecipeVaultDbContext.cs
│   ├── Repositories/
│   │   ├── IRecipeRepository.cs
│   │   └── RecipeRepository.cs
│   └── Migrations/
├── RecipeVault.Dto/
│   ├── Input/
│   │   ├── CreateRecipeDto.cs
│   │   ├── UpdateRecipeDto.cs
│   │   └── ParseRecipeRequestDto.cs
│   ├── Output/
│   │   ├── RecipeDto.cs
│   │   ├── RecipeIngredientDto.cs
│   │   ├── RecipeInstructionDto.cs
│   │   └── ParseRecipeResponseDto.cs
│   └── Search/
│       └── RecipeSearchDto.cs
├── RecipeVault.Facade/
│   ├── IRecipeFacade.cs
│   ├── RecipeFacade.cs
│   └── Mappers/
│       └── RecipeMapper.cs
├── RecipeVault.Health/
│   └── GeminiHealthCheck.cs
├── RecipeVault.WebApi/
│   ├── Controllers/
│   │   └── RecipesController.cs
│   ├── Models/
│   │   ├── Requests/
│   │   │   ├── CreateRecipeModel.cs
│   │   │   ├── UpdateRecipeModel.cs
│   │   │   └── ParseRecipeRequestModel.cs
│   │   └── Responses/
│   │       ├── RecipeModel.cs
│   │       └── ParseRecipeResponseModel.cs
│   ├── Mappers/
│   │   └── RecipeModelMapper.cs
│   ├── Program.cs
│   ├── Startup.cs
│   └── appsettings.json
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

### Phase 1a: Multimodal Scanner (Current)
- [ ] Solution scaffolding (cortside template)
- [ ] Domain entities
- [ ] Database context + migrations
- [ ] Supabase storage integration
- [ ] Gemini integration service
- [ ] Parse endpoint
- [ ] Recipe CRUD endpoints
- [ ] Basic Angular UI (upload + display)

### Phase 1b: Photo Cleanup (Future)
- Image straightening/de-warping
- Brightness/contrast adjustment
- Preserve original alongside cleaned version

### Phase 1c: Video Import (Future)
- Share extension architecture
- Audio transcription
- Caption extraction

---

## Constraints & Principles

1. **Errors are errors.** No silent fallbacks. Missing config = startup failure. Bad data = 4xx/5xx response.

2. **Follow Cortside patterns.** Reference `cortside/coeus/shoppingcart-api` for architectural decisions.

3. **Stay in scope.** Phase 1 is ONLY the multimodal scanner. Resist feature creep.

4. **Free tier friendly.** Design for Supabase/Gemini free limits. Warn before hitting them.

5. **AI is fallible.** Always preserve raw text. Always allow manual correction.

---

## References

- Cortside templates: `cortside/coeus/shoppingcart-api`
- Gemini API: https://ai.google.dev/docs
- Supabase .NET: https://github.com/supabase-community/supabase-csharp
