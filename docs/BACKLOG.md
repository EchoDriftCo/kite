# RecipeVault Backlog

*Last updated: 2026-02-11*

---

## Completed (MVP)

- [x] Angular standalone component scaffold
- [x] Recipe list view (my recipes)
- [x] Recipe detail view
- [x] Image upload + AI parse flow (Gemini 1.5 Flash)
- [x] Image crop/rotate before parsing
- [x] Manual edit after AI parse
- [x] Recipe CRUD (create, read, update, delete)
- [x] Supabase Auth integration (login/signup, session persistence)
- [x] Subject-scoped data ownership (row-level filtering)
- [x] Public/private recipe visibility
- [x] Tag system (manual + AI dietary analysis)
- [x] Meal planning with weekly calendar
- [x] Grocery list generation (AI-consolidated)
- [x] Source image preservation (separate from hero image)
- [x] Recipe image upload endpoint (local storage, gitignored)
- [x] Cascade delete for recipe children
- [x] Print-friendly recipe view

---

## v1 Roadmap

### Recipe Ratings & Favorites
- [ ] Rating system — personal 1-5 star rating on recipes
- [ ] Favorites / bookmarks — quick-flag recipes (heart icon on list + detail)
- [ ] Sort recipe list by rating
- [ ] "My Favorites" filter on recipe list

### Recipe Scaling
- [ ] Scale button on recipe detail page ("Make for X servings")
- [ ] Frontend-only math — multiply ingredient quantities by scale factor
- [ ] Display scaled quantities alongside originals
- [ ] Fraction display for scaled values (e.g. 1.5 → 1 1/2)

### Search & Discovery
- [ ] Full-text search across title, description, ingredients
- [ ] Search/filter own recipes by tags, rating, cook time, etc.
- [ ] Sort options on recipe list (date added, alphabetical, cook time, rating)
- [ ] Public recipe browse — discover other users' public recipes
- [ ] Search public recipes by title, tags, dietary filters

### Share by Link
- [ ] Generate shareable link for a recipe (no login required to view)
- [ ] Public recipe detail page (read-only, no auth)
- [ ] Copy-to-my-recipes button for logged-in users viewing shared recipe

### Recipe Import from URL
- [ ] Paste recipe website URL → scrape structured data
- [ ] Parse JSON-LD schema.org/Recipe markup (most major recipe sites)
- [ ] Fallback to AI parsing of page content
- [ ] Preview parsed data before saving

### Recipe Notes
- [ ] Personal notes field on recipes ("Dad loved this", "use less salt")
- [ ] Separate from description — private, not shared
- [ ] Display on detail page in collapsible section

### Meal Planning Improvements
- [ ] Copy/repeat a previous meal plan as starting point
- [ ] Quick-add to meal plan from recipe detail page
- [ ] Grocery list: check off items as you shop
- [ ] Grocery list: group by category (produce, dairy, pantry, etc.)

### Image Storage (Production)
- [ ] Move from local filesystem to cloud blob storage (Azure Blob / S3)
- [ ] Configurable storage provider
- [ ] CDN-served image URLs

### Technical Polish
- [ ] Move Gemini API key to user secrets
- [ ] Rate limiting for parse endpoint
- [ ] Dockerfile + docker-compose for local dev
- [ ] Export recipe as PDF or text

---

## v2 Roadmap

### Recipe Relationships (Forking / Remixes)
- [ ] Fork recipe (creates copy with ParentRecipeId reference)
- [ ] View derivation tree (upstream → this → downstream)
- [ ] Diff view: see what changed from original
- [ ] "Community remixes" endpoint for public recipes

### Social Features (Circles)
- [ ] Family/Friends circles — invite-only groups
- [ ] Share recipe to circle (reference, not copy)
- [ ] Request recipe from circle member
- [ ] "Mom's Cookbook" = curated circle with shared recipes
- [ ] Privacy model: private → circles → public

### Intelligence Layer
- [ ] Nutrition integration (USDA FoodData Central API)
- [ ] Ingredient normalization → macros per serving
- [ ] Allergen warnings on meal plans
- [ ] Smart substitutions ("I don't have X" → suggest Y)
- [ ] Dietary mode: "Make this dairy-free" → auto-suggest all swaps
- [ ] "What can I make?" — filter recipes by ingredients on hand

### Advanced Import/Export
- [ ] Browser extension: scrape recipe from any website
- [ ] Import from Paprika, Whisk, AnyList export formats
- [ ] Batch import for recipe books (queue + async processing)

### Future Ideas
- [ ] Voice mode: "What's in grandma's spaghetti?"
- [ ] Cooking mode: step-by-step with timers, read aloud
- [ ] Auto-generate meal plans based on dietary goals
- [ ] OpenTelemetry for distributed tracing

---

*Prioritization happens in conversation. This document captures the vision.*
