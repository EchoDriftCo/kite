# RecipeVault Backlog

*Ideas and future work, captured 2026-02-04*

---

## Phase 1 Remaining

### Frontend (MVP)
- [ ] Angular 17 scaffold with standalone components
- [ ] Recipe list view (my recipes)
- [ ] Recipe detail view
- [ ] Image upload + parse flow
- [ ] Manual edit after AI parse
- [ ] Supabase Auth integration (login/signup)

### Backend Polish
- [ ] Supabase JWT auth middleware configuration
- [ ] Row-level security (filter by CreatedBySubjectId)
- [ ] Image storage to Supabase bucket (currently just URL references?)
- [ ] README.md expansion (setup instructions, architecture overview)

---

## Phase 2: Recipe Relationships (The Remix Feature)

### Domain Model Additions
```csharp
// Recipe.cs additions
public int? ParentRecipeId { get; private set; }
public virtual Recipe ParentRecipe { get; private set; }
public virtual IReadOnlyList<Recipe> Derivations => derivations;

// New method
public Recipe Fork(string newTitle) { ... }
```

### Features
- [ ] Fork recipe (creates copy with ParentRecipeId reference)
- [ ] View derivation tree (upstream → this → downstream)
- [ ] Diff view: see what changed from original
- [ ] "Community remixes" endpoint (if made public)

### Open Questions
- Can you fork someone else's recipe? (requires sharing model)
- How deep can the fork tree go?
- Should forks auto-update when parent changes? (probably not)

---

## Phase 2: Tags & Categories

### Domain Model Additions
```csharp
// New entities
Tag (TagId, Name, Type: System|User, Color?)
RecipeTag (RecipeId, TagId)

// Types
- System tags: auto-generated (vegan, gluten-free, dairy-free, etc.)
- User tags: custom categories (weeknight, company favorites, kid-approved)
```

### AI Auto-Tagging
- Extend Gemini prompt to suggest dietary tags
- Surface ingredient substitutions (dairy → oat milk)
- Allergen detection from ingredient names

### Categories vs Tags
- Tags = flat, many-per-recipe
- Categories = hierarchical? (Desserts > Pies > Fruit Pies)
- Or just use tags with naming convention? (dessert, dessert:pie)

---

## Phase 3: Social Features

### Family/Friends Circles
```csharp
Circle (CircleId, Name, OwnerId)
CircleMember (CircleId, UserId, Role: owner|admin|member)
CircleRecipe (CircleId, RecipeId, SharedByUserId, SharedAt)
```

### Sharing Flows
- Share recipe to circle (creates CircleRecipe reference, not copy)
- Request recipe from circle member
- "Mom's Cookbook" = curated circle with mom's recipes

### Privacy Model
- My recipes = private by default
- Circles = invite-only groups
- Public = opt-in per recipe (for global search)

---

## Phase 4: Intelligence Layer

### Nutrition
- Integrate USDA FoodData Central API
- Map normalized ingredients → nutrition data
- Aggregate per-serving macros

### Meal Planning
- Weekly calendar view
- Auto-generate shopping list from planned recipes
- Scale recipes for planned servings

### Smart Substitutions
- "I don't have X" → suggest Y
- Dietary mode: "Make this dairy-free" → auto-suggest all swaps

---

## Technical Debt / Improvements

- [ ] Move Gemini API key to user secrets (not appsettings.local.json)
- [ ] Add rate limiting for parse endpoint (Gemini free tier: 15 req/min)
- [ ] Consider batch parsing for recipe books (queue + async processing)
- [ ] Add OpenTelemetry for distributed tracing
- [ ] Dockerfile + docker-compose for local dev

---

## Random Ideas

- **Browser extension:** Scrape recipe from any website, auto-import
- **Voice mode:** "Hey Nora, what's in grandma's spaghetti?" (me, hi 👋)
- **Cooking mode:** Step-by-step with timers, read aloud
- **Recipe scaling:** "Make this for 12 people" → auto-adjust quantities
- **Import from other apps:** Paprika, Whisk, AnyList export formats

---

*This backlog is for brainstorming. Actual prioritization happens in conversation.*
