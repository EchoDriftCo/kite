# Section 1: Authorization and data isolation

Audited 2026-10-18 against `main` at 43bff9e (after PR #8), with read-only queries against the live Supabase project `RecipeVault` (`umwycxfebintkenehqlj`).

## Scope

- **Authentication:** `Startup` (the `ApiTokenOrJwt` policy scheme, JWT validation, the `sub` check), `ApiTokenAuthenticationHandler`, `GuestAccess`, `SupabaseSubjectFactory`
- **Filters:** `AdminOnly`, `RegisteredUserOnly`, `GuestQuota`, `RequiresTier`
- **All 24 controllers** in `src/RecipeVault.WebApi/Controllers`, and the facade and domain service behind each endpoint that takes an id or token
- **Repositories and search objects** where they decide what a query returns (`RecipeSearch`, `TagSearch`, `CollectionRepository`, `TagRepository`), plus the `score_recipes_by_ingredients` SQL behind ingredient search

Method: for each endpoint that takes an id, find where ownership or visibility is checked. Suspected gaps are proved in `DataIsolationEndpointTests`, where Alice owns the data and Bob tries to reach it by id. The same file pins the checks that already hold.

## Confirmed safe

Each row is covered by a test in `DataIsolationEndpointTests` (regression tests that pass before and after this PR) or in the existing suites.

| Check | Evidence |
|-------|----------|
| Recipes: read, update, delete, visibility, rating, favorite, tags, share token, fork, forks list, substitutions, cooking data, links, export, equipment | `RecipeService.GetOwnRecipeAsync` (owner only) or `GetRecipeAsync` (owner, or anyone if public). `RecipeLinkService` and `ExportService` have their own owner checks. Bob gets 404 (or a 500 with no data, see "Found in passing") on 10 representative endpoints. |
| Recipe list and search | `RecipeFacade.SearchRecipesAsync` forces `CreatedSubjectId` to the caller. `discover` forces `IsPublic = true`. |
| Collections: read, update, delete, add, remove, reorder | `CollectionService` checks `SubjectId` (or public, for reads). The list forces `SubjectId` unless the request asks for public collections. |
| Meal plans: read, update, delete, grocery list, list | `MealPlanService.GetMealPlanAsync` checks the owner. The list forces `CreatedSubjectId`. |
| Cooking logs | Every read and write checks the owner. A new log needs a recipe the caller can see. |
| Circles | `CircleService.GetCircleAsync` requires an active membership. Updating, inviting and removing members need the owner or admin role, and deleting needs the owner. |
| API tokens | `RevokeTokenAsync` checks `SubjectId`. The list is the caller's own tokens. Guests can't manage tokens (`GuestAccessEndpointTests`). |
| Dietary profiles, pantry, equipment, onboarding, account | Each is keyed on the caller's subject id. No endpoint takes another user's id. |
| Admin endpoints | `[AdminOnly]` compares the caller's `sub` with `AdminSubjectIds` from configuration: one id, in `appsettings.json`. It applies to creating, listing and deactivating beta codes, setting the account tier, and listing the waitlist. Unit tests are in `AdminOnlyAttributeTests`. The JWT handler rejects a token whose `sub` isn't a user id, so a missing `sub` can't fall back to Cortside's shared empty subject. |
| Share tokens | 10 base62 characters (about 59 bits) from `RandomNumberGenerator`. A link opens without signing in, never returns the token, and stops working when revoked (`ShareLink_*` tests). |
| Guest restrictions | Guests can't publish a recipe or collection (also when `isPublic` is set on create or update), set visibility, make share links, send circle invites, redeem beta codes or manage API tokens. AI and save endpoints have per-IP daily quotas. Covered by `GuestAccessEndpointTests`. |
| Ingredient search | `score_recipes_by_ingredients` only scores rows where `CreatedSubjectId = p_subject_id OR IsPublic`. |
| Production data | Read-only counts: no cross-user tag links, no meal-plan entry pointing at another user's private recipe, no public collection holding a private recipe, no nutrition rows and no API tokens. Nothing was exploited, and there's nothing to clean up. |

## Findings

| # | Severity | Finding | Status |
|---|----------|---------|--------|
| A1 | High | Nutrition endpoints check neither ownership nor visibility | Fixed |
| A2 | Medium | Meal plans accept any recipe id and show its title and ingredients | Fixed |
| A3 | Medium | The dietary check reads any recipe's ingredients | Fixed |
| A4 | Medium | Public collections list private recipes, to anonymous callers too | Fixed |
| A5 | Medium | Tags are shared across users by name | Fixed |
| A6 | Medium | An API token can mint more API tokens | Fixed for token management |
| A7 | Low | Circle invites are bearer links, not tied to the invited email | Reported |
| A8 | Low | Circle members keep seeing a shared recipe after it's made private | Reported |
| A9 | Info | `RequiresTier` lets everyone through and writes to the database on every call | Owner decision |
| A10 | Low | Beta invite codes use `System.Random` | Reported |
| A11 | Low | Admin endpoints accept API tokens | Reported |

### A1 · High · Nutrition endpoints check neither ownership nor visibility (fixed)

`NutritionFacade` looked recipes up straight from the `DbContext` by resource id, with no owner or visibility check. Any signed-in user, guests included, could:

- **Overwrite** any recipe's ingredient nutrition with `PUT /recipes/{id}/ingredients/{i}/nutrition`.
- **Wipe and recompute** it with `POST /recipes/{id}/nutrition/analyze`, which deletes the stored values, including the owner's manual overrides, and spends USDA quota.
- **Read** the nutrition of private recipes with `GET /recipes/{id}/nutrition`.

Every public recipe's id is listed by `/recipes/discover`, so the writes need no guessing.

- **Proof:** `Nutrition_CannotOverrideIngredientNutritionOnAnotherUsersPublicRecipe`, `…PrivateRecipe` and `Nutrition_CannotReadAnotherUsersPrivateRecipeNutrition`. **All three fail without the fix** (Bob gets 200).
- **Fix:** analyze and update go through `IRecipeService.GetOwnRecipeAsync`, so they are owner only. The read goes through `GetRecipeAsync`, so it's open to the owner, or anyone if the recipe is public. A recipe the caller can't reach now returns 404, not 500. `GetOwnRecipeAsync` already existed privately in `RecipeService`; it's now on the interface.

### A2 · Medium · Meal plans accept any recipe id and show its title and ingredients (fixed)

`MealPlanService.SetEntriesFromDtoAsync` accepted any recipe id. The meal plan response then shows that recipe's title, and the grocery list shows every ingredient, scaled, plus the title. So knowing a private recipe's id, from A4 for example, was enough to read it.

- **Proof:** `MealPlan_CannotAddAnotherUsersPrivateRecipe` **fails without the fix** (201, and the body contains the title). `MealPlan_CanStillAddAnotherUsersPublicRecipe` passes before and after.
- **Fix:** entries need a recipe the caller owns, or a public one. Otherwise 404.

### A3 · Medium · The dietary check reads any recipe's ingredients (fixed)

`DietaryProfileFacade.CheckRecipeAsync` loaded the recipe without a visibility check. Each conflict it returns quotes the ingredient text, so a profile that avoids common ingredients reveals a private recipe's ingredient list.

- **Proof:** `DietaryCheck_CannotCheckAnotherUsersPrivateRecipe` **fails without the fix** (200).
- **Fix:** the recipe must be the caller's own or public. Otherwise 404.

### A4 · Medium · Public collections list private recipes, to anonymous callers too (fixed)

`CollectionMapper` returned every recipe in a collection: id, title, description, image and rating. A public collection can hold:

- its owner's private recipes, because adding only checks that the owner can see the recipe; and
- another user's recipe that was public when it was added and has since been made private.

`GET /collections/public` is anonymous, so anyone could collect those ids and titles and pass the ids to A1–A3. `GET /collections/featured` also skipped the `IsPublic` check.

- **Proof:** `PublicCollection_DoesNotListPrivateRecipes` **fails without the fix**: the anonymous list contains the private recipe's title and id.
- **Fix:** the mapper takes the viewer's subject id (null when anonymous) and lists only recipes that are public or the viewer's own. `RecipeCount` counts the same set. The owner still sees all of their own recipes. The featured list now also requires `IsPublic`.
- **Behavior change:** a viewer no longer sees a recipe that would 404 when they opened it.

### A5 · Medium · Tags are shared across users by name (fixed)

`TagRepository.GetByNameAndCategoryAsync` matched any tag, including another user's private one. So:

- `POST /tags` with a name another user already had returned **their** tag.
- Assigning a tag by name attached their tag to your recipe.
- Assigning by `tagResourceId` accepted any private tag.
- `GET /tags/{id}` returned any private tag.

The tag's owner can still rename or delete it (`GetOwnTagAsync`). Renaming changes the label on everyone else's recipes, and deleting it strips the tag from them. So the first user to claim a common name ("Weeknight", "Dinner") controlled that label on everyone's recipes. AI dietary tagging used the same lookup, so a user's private "Vegan" tag in the Dietary category could be attached to other users' recipes.

- **Proof:** the four `Tags_*` tests. **All fail without the fix.**
- **Fix:** the lookup takes the caller's subject id and matches only global tags or the caller's own, preferring global. Dietary tagging passes `null`, so it matches global tags only. Reading or assigning a tag by id needs a global tag or the caller's own; otherwise 404.
- **Production check:** none of the 15 private tags is linked to another user's recipe, so no data needs repairing.

### A6 · Medium · An API token can mint more API tokens (fixed for token management)

API tokens have no scopes. A token request passes `[Authorize]` and `[RegisteredUserOnly]`, because tokens carry no `is_anonymous` claim. So a leaked token could create a new token that survives revoking the leaked one, and could list or revoke the user's other tokens.

- **Proof:** `ApiToken_CannotBeUsedToManageApiTokens` **fails without the fix** (the token mints a new one).
- **Fix:** a new `[SessionOnly]` filter returns 403 when the request was authenticated with an API token. `ApiTokensController` uses it. Tokens still work everywhere else. The UI never calls the token endpoints with a token; it uses the Supabase session.
- **Remaining:** see A11. Longer term, give tokens scopes, for example import-only for the browser extension.
- **Production:** no API tokens exist, so there's nothing to revoke.

### A7 · Low · Circle invites are bearer links, not tied to the invited email (reported)

`AcceptInviteAsync` checks only the token. It doesn't check that the person accepting is the invitee. Anyone the email is forwarded to, or who sees the link, joins once. Because `Accept()` is single use, the real invitee is then locked out.

`GET /circles/invite/{token}` is anonymous and returns the invitee's email address. The token is a random GUID, so this needs the link.

- **Fix (for Section 7, alongside the UI):** compare the JWT `email` claim with `InviteeEmail`, case-insensitively, on accept, and drop `InviteeEmail` from the anonymous response. Decide what the UI does when they don't match. Production has 7 invites.

### A8 · Low · Circle members keep seeing a shared recipe after it's made private (reported)

`GetCircleRecipesAsync` returns the full recipe, ingredients included, for everything shared to the circle. A member can share someone else's public recipe. If that recipe's owner later makes it private, the circle keeps serving it in full, even though the owner may not be in the circle.

- **Fix:** list a shared recipe only if it's public, or its owner is an active member of the circle. That keeps "share my private recipe with my family" working.

### A9 · Info · `RequiresTier` lets everyone through and writes to the database on every call (owner decision)

`MeetsTierRequirement` returns `true`, so the four "Premium" controllers are open to every signed-in user, guests included: nutrition, dietary profiles, recipe generation and recipe mixing. That's deliberate for the beta (see the TODO). Guests are still held by the per-IP `GuestQuota`.

The filter also calls `GetOrCreateAccountAsync` and `SaveChangesAsync` on every request, before deciding nothing.

- **When premium launches:** compare the tiers, decide where `Beta` ranks against `Premium`, and add tests. Until then, skip the account lookup.

### A10 · Low · Beta invite codes use `System.Random` (reported)

`BetaInviteCodeService` makes codes with a shared static `System.Random`. That isn't a cryptographic generator, and it isn't thread-safe. The code space (32¹² ≈ 1.2 × 10¹⁸) makes guessing impractical, but it's a one-line change to use `RandomNumberGenerator.GetInt32`. `POST /beta-invite-codes/validate` is anonymous and has no rate limit (see "Found in passing").

### A11 · Low · Admin endpoints accept API tokens (reported)

`[AdminOnly]` checks only the subject id. So an API token belonging to the admin is an admin credential: it can create beta codes, set the tier and read the waitlist.

- **Fix:** add `[SessionOnly]` to the five admin actions. This PR doesn't, because the test fixture has no admin configured, so the change wouldn't be proved. It's a small follow-up.
- **Also:** the admin id sits in the committed `appsettings.json` (settings item S5).

## Found in passing (for other sections)

- **Section 5 (error handling):** `CookingLogController`, `EquipmentController` and `RecipeLinksController` catch every exception and return 500 with `ex.Message`. So "not found" becomes a 500 that echoes the internal message, for example "Recipe with id … not found". Access is still denied, and no data leaks; `DataIsolationEndpointTests` accepts any non-2xx response with no data in the body.
- **Sections 2 and 4 (rate limiting):** `RateLimitPolicies.Api` (60 a minute) is registered but no endpoint uses it. Only the `Upload` policy is applied. Anonymous endpoints such as `/beta-invite-codes/validate`, `/recipes/shared/{token}` and `/waitlist` have no rate limit.
- **Sections 3 and 5 (ingredient search):** `POST /recipes/search/by-ingredients` calls `score_recipes_by_ingredients()`. That function exists only in `docs/migrations/ingredient-search-scoring-function.sql`, not in any EF migration. Section 0 found no functions in production's `public` schema, so this endpoint most likely fails in production.

## This PR

- `src/RecipeVault.WebApi.IntegrationTests/Endpoints/DataIsolationEndpointTests.cs`: 27 tests.
  - **11 fail without the fix:** A1 (3), A2, A3, A4, A5 (4) and A6. That was checked by running them against `main`'s source.
  - **16 are regression tests** that pass before and after: own-recipe and public-recipe paths, 10 recipe endpoints, meal plans, collections, cooking logs, API tokens, circles and share links.
- **Fixes:**
  - `NutritionFacade`
  - `MealPlanService`
  - `DietaryProfileFacade`
  - `CollectionMapper`, `CollectionFacade` and `CollectionRepository`
  - `TagRepository`, `TagService` and `RecipeService` (tags)
  - `IRecipeService.GetOwnRecipeAsync`
  - `SessionOnlyAttribute` and `ApiTokensController`
- **Unit tests:** two existing tests now build a global tag, and four mocks take the new subject-id argument.
- **No migrations and no settings changes.**

## Settings only you can change

| # | Where | Change | Why |
|---|-------|--------|-----|
| S5 | Fly secrets / `appsettings.json` | Confirm `AdminSubjectIds` holds only your current Supabase user id (`9fa74dcb-…`). Consider moving it from the committed `appsettings.json` to a Fly secret (`AdminSubjectIds__0`), so changing admins doesn't need a commit. | Admin enforcement (A11) |
| S6 | Product decision | Decide when tier gating starts and whether `Beta` counts as `Premium`. Until then, every user, guests included, gets the premium features. | A9 |
| S7 | Product decision | Decide whether circle invites should be tied to the invited email (A7). It changes what happens when someone opens an invite while signed in with a different address. | A7 |
