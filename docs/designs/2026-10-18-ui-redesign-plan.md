# UI Redesign Plan (from the October 18 review)

**Source:** [reviews/2026-10-18-ui-review.md](reviews/2026-10-18-ui-review.md)
**Already done:** the four critical bugs (light-mode theme, cooking-mode icons, meal-plan dates, dark-mode contrast) are fixed in [EchoDriftCo/kite#9](https://github.com/EchoDriftCo/kite/pull/9). CI passes; it is not merged yet.
**This plan covers everything else in the review:** contrast, button hierarchy, the recipe page layout, mobile, and design-system drift.

## How each phase runs

Each phase follows the project's existing design → build → check workflow:

1. **Design spec.** A task doc in `docs/designs/tasks/` (new `5.x` series, same format as `2.3-recipe-detail.md`), with an ASCII layout for desktop and mobile, states, and token names. Phases 1 and 3 change layout, so they also get a clickable HTML mockup to approve before any code is written.
2. **Implementation.** One focused PR per phase, based on `main` after #9 is merged.
3. **Check.** Re-run `capture.mjs`, compare before/after screenshots, `ng build --configuration production`, and Karma specs for any logic touched.

## Order and dependencies

```
Phase 0  Design-system foundation   ──┬──▶ Phase 1  Recipe page redesign
(tokens, buttons, confirm dialog)     ├──▶ Phase 2  Action hierarchy on other screens
                                      └──▶ Phase 3  Mobile: navigation, recipe list, cooking mode
                                                          │
                                         Phase 4  Cleanup and regression guard ◀──┘
```

Phase 0 changes the tokens and shared pieces the other phases use, so it goes first. Phases 1–3 touch separate files, so they can run in parallel sessions. Phase 4 sweeps whatever is left.

---

## Phase 0 — Design-system foundation (spec 5.0)

**Goal:** one set of rules that the later phases apply, so each screen doesn't get a one-off fix.

| Change | Detail | Files |
|---|---|---|
| Update the foundation doc to what was built | Light by default with a dark theme; orange `#ff8c42` and sage `#7c9885`; full token list including the ones #9 added. Remove "dark only, amber/teal". | `docs/designs/01-foundation.md`, `02-components.md` |
| Text-safe colour tokens | `--color-primary-text` (light `#a34b15` = 5.9:1 on white; dark `#ffa366` = 5.3:1 on slate). #9 already added `--color-accent-text`. Rule: bright orange and sage are for **fills only**, never small text. | `ui/src/_tokens.scss` |
| Hint grey only for placeholders | `--color-text-hint` (2.5:1) is for placeholders and disabled text only. Settings descriptions, the user's email, dates, servings and fork counts move to `--color-text-secondary` (5.3:1). | Settings, recipe detail, meal plan detail, discover, circles SCSS |
| Button hierarchy | **Primary:** `mat-flat-button color="primary"`, at most one per view. **Secondary:** `mat-stroked-button`. **Tertiary:** `mat-button`. **Destructive:** menu item, then a confirm dialog; never a filled red button in a toolbar. Style these once in `theme.scss` and remove the per-component colours. Today there are 66 raised and 37 stroked buttons, and `color="warn"` raised buttons in 7 files. | `ui/src/theme.scss`, `02-components.md` |
| Shared confirm dialog | A `ConfirmDialogComponent` (title, body, confirm label, destructive flag) to replace the 7 `window.confirm()` calls: circles ×3, collections, dietary profile, meal plan, and recipe delete. | new `ui/src/app/shared/components/confirm-dialog/` |
| Type floor | Body text is never below 14px. Replace the 11 uses of `font-size: 0.75rem` with `--font-caption` (metadata only) or `--font-body-small`. Recipe instructions use `--font-body-large` (16px). | Component SCSS |
| Metadata vs chips | Time and servings show as plain icon + text (`45 min · 4 servings`). Chips are only for things you can tap: tags and filters. | Recipe list, discover and recipe detail meta rows |

**Done when:** `capture.mjs` shows no body text below 4.5:1 and no large text below 3:1 in either theme; the new rules are documented in `02-components.md`; no `window.confirm(` calls remain.

---

## Phase 1 — Recipe page redesign (spec 5.1 + mockup)

**Problem:** 13 actions and three supporting panels come before the ingredients, which start about 1,500px down.

**New order (desktop and mobile):**
```
[←]                          [print] [share] [♡] [⋮]      ← slim toolbar; ⋮ holds everything else
Hero image (16:9, only if there is one)
Title (h1)
★★★★☆ · 45 min · Serves 4 · Public                         ← one metadata line
[Italian] [Dinner]                                         ← tags
[START COOKING]  [Edit]                                    ← one primary + one secondary (owner)
                                                             ([Fork] as primary for non-owners)
┌ Ingredients ── [−] 4 servings [+]  ½x 1x 2x ┐  ┌ Instructions ───────┐
│ ○ 2 cup flour …                             │  │ ① Preheat …  (16px)  │
└─────────────────────────────────────────────┘  └──────────────────────┘
Your cooking history (if any)
More about this recipe: Nutrition · Equipment · Grocery     ← tabs or expansion panels, collapsed
Component recipes / used in
Original source image (collapsed)
```

| Change | Detail |
|---|---|
| One overflow menu | Remove the `desktop-actions` block, which duplicates the `moreMenu` items as stroked buttons (`recipe-detail.component.html:133-158`). The ⋮ menu holds Substitutions, Share to Circle, I Made This, Shop Ingredients, Mix with…, Make public/private, then a divider and Delete. |
| Delete | Menu item → `ConfirmDialogComponent` (destructive). |
| Scaling | Move the servings control into the Ingredients header, where it applies. Touch targets ≥44px on mobile. |
| Supporting panels | Nutrition, equipment and grocery move below the recipe into one collapsible "More about this recipe" section. (D3) |
| Print styles | Keep `printRecipe()` output to title, metadata, ingredients and instructions. |

**Files:** `recipe-detail.component.{html,scss,ts}`, minor input changes in `nutrition-panel`. Keep the existing `data-tour` attributes so the onboarding tour still works.
**Done when:** ingredients are visible without scrolling at 1366×860 and within one scroll on 390×844; there is one primary button; Delete is only reachable through the menu and asks for confirmation; the tour still finds its targets.

---

## Phase 2 — Action hierarchy on other screens (spec 5.2)

Apply the Phase 0 button rules to the remaining screens:

| Screen | Now | After |
|---|---|---|
| Meal plan detail | Grocery List (sage filled), Copy (near-black), Edit (orange raised), Delete (red filled) | **Grocery List** primary, Edit secondary, ⋮ menu with Copy and Delete (confirm). Add a legend for leftovers and a `title` tooltip on truncated entries. |
| Grocery list | Copy and Print as near-black filled buttons | Stroked secondaries. |
| Login | Sign In disabled until the form is valid; guest link last; 100px logo | Sign In always enabled, validating on submit; "Continue as guest" as a stroked button directly under Sign In for first-time visitors; logo 48px. |
| Settings, cooking history, ingredient search, recipe form, nutrition panel | `color="warn"` raised buttons | Destructive actions become menu items or stroked buttons in the warn colour with a confirm dialog. |
| Text links (Forgot password, Sign up, "See what's included", "Create your first collection", Get Checkout Links) | Orange or sage text at 2.3–3.1:1 | `--color-primary-text` / `--color-accent-text`. |

**Files:** `meal-plan-detail`, `grocery-list`, `login`, `settings`, `cooking-history`, `ingredient-search`, `recipe-form`, `features`, `collection-list`.

---

## Phase 3 — Mobile (spec 5.3 + mockup)

| Change | Detail | Files |
|---|---|---|
| Bottom navigation below 768px | Recipes · Discover · Meal Plans · Circles · More. "More" opens a bottom sheet with the other 6 destinations, grouped (Cooking: Log, Stats · Kitchen: Equipment, Dietary Profiles, Collections · Settings). Remove the hamburger. Desktop keeps the top nav. | `app.component.{html,scss}`, new `bottom-nav` component |
| Recipe list header | Row 1: title + search icon (expands to a field). Row 2: favourites + tag filters (no My/Public toggle, D1). A floating **New** button opens a menu (New recipe · Import · AI Generate). Down from 6 stacked rows to 2. | `recipe-list.component.{html,scss}` |
| Recipe cards on mobile | Compact horizontal card: 96px thumbnail, title, metadata line, rating. No-image cards get a short tinted thumbnail instead of a 200px block. Aim for 5+ recipes per screen. Apply the same card to Discover. | `recipe-list`, `discover` SCSS |
| Full-screen cooking mode | Hide the app toolbar on `/recipes/:id/cook`. Previous/Next become a sticky bottom bar with 56px targets. Keep the screen awake (Wake Lock API, where the browser supports it). | `cooking-mode`, `app.component` |
| Touch targets | Every interactive control ≥44px on mobile (scale buttons, card heart, meal-plan cells). | Various |

**Done when:** the 390×844 captures show the recipe list with 5+ recipes on the first screen, bottom nav on every signed-in screen except cooking mode, and no tap target under 44px. Measure with a small check added to `capture.mjs`.

---

## Phase 4 — Cleanup and regression guard (spec 5.4)

- **Hard-coded colours:** swap literals for tokens file by file, worst first: `dietary-warning` (24), `recipe-list` (16), `recipe-links` (14), `discover` (14), `circle-detail` (11). Remove the old teal/amber from the onboarding dialog, tour tooltip, beta-invite dialog and upgrade banner. Target: no hex literals in component SCSS outside `_tokens.scss`/`theme.scss`, and fewer than 20 `!important`.
- **Form fields:** one appearance (outline) in both themes.
- **Dark-mode leftovers** from #9's "not in scope" list: hard-coded greys in circles and ingredient search, and the warn red on "Delete Account" (2.8:1).
- **Regression guard:** move `capture.mjs` into `ui/e2e/visual/` with its fixtures (nutrition returns 404). Add `npm run audit:ui` and a Playwright test that fails on any contrast entry below AA or any broken icon. Run it in the existing `e2e.yml` workflow.

---

## Decisions (agreed 2026-10-18)

| # | Decision |
|---|---|
| D1 | **Recipes shows only your recipes.** Remove the My/Public toggle (Phase 3, recipe list header); Discover is the place for public recipes. |
| D2 | **Bottom nav on mobile** (Phase 3). |
| D3 | **Supporting panels go below the recipe, collapsed** (Phase 1). No right-hand column. |
| D4 | **HTML mockups first** for Phases 1 and 3, approved before any code is written. |

## PRs

One PR each for Phases 0, 1 and 2. Phase 3 may split into two (navigation, then recipe list and cooking mode). Phase 4 may split the regression guard from the colour sweep.
