# Yummly Import — Design Doc

**Author:** Kovacs  
**Date:** 2026-03-20  
**Status:** Draft  
**Target:** Weekend launch (Mar 22-23)

## Context

Yummly (owned by Whirlpool) was shut down in December 2024. The entire team was laid off in April 2024, and the app/website went offline in December. Users had **no data export option** — their saved/bookmarked recipes are gone.

This is a competitive opportunity. Ex-Yummly users are looking for a new home. If we offer a way to recover their recipes, we win them as users.

## The Problem

Yummly never provided a data export. Users have:
1. **Wayback Machine cached pages** — Some recipe pages are archived
2. **Browser bookmarks** — URLs like `yummly.com/recipe/Recipe-Name-12345`
3. **Screenshots** — Photos of recipe pages (already handled by our multi-image import)
4. **Email forwards** — Yummly "email recipe" feature saved recipe text to email
5. **Google Takeout** — Yummly app activity may have generated data in Google Takeout

## Approach: Multi-Strategy Import

Since there's no single canonical export format, we build multiple recovery paths:

### Strategy 1: Wayback Machine URL Recovery (Primary)

**How it works:**
1. User provides a list of Yummly URLs (from browser bookmarks, history export)
2. We rewrite each URL to `web.archive.org/web/2024/{yummly_url}`
3. Fetch the archived page
4. Parse recipe data from HTML (Yummly used schema.org/Recipe JSON-LD)
5. Import into RecipeVault

**Yummly URL format:** `https://www.yummly.com/recipe/Recipe-Title-NUMERIC_ID`

**Data in Yummly pages (pre-shutdown):**
- JSON-LD `@type: Recipe` in `<script type="application/ld+json">`
- Title, description, image URL
- Ingredients (structured list)
- Instructions (step-by-step)
- Prep time, cook time, total time (ISO 8601 duration)
- Yield/servings
- Nutrition info
- Source URL (original recipe site)
- Rating, review count

**Implementation:**
- New endpoint: `POST /api/v1/import/yummly`
- Request: `{ "urls": ["https://www.yummly.com/recipe/..."] }`
- Backend fetches from Wayback Machine, parses JSON-LD
- Falls back to HTML parsing if no JSON-LD
- Returns preview of all parsed recipes for user confirmation
- Batch import on confirm

**Effort:** 4-6 hours backend, 2-3 hours frontend

### Strategy 2: Text/Email Paste Import (Already Supported)

Users who emailed themselves recipes can paste the text into our existing structured import or use Gemini text parsing (`POST /api/v1/import/html`).

**Effort:** 0 hours (already built)

### Strategy 3: Screenshot Import (Already Supported)

Users with screenshots can use multi-image import (`POST /api/v1/import/multi-image`).

**Effort:** 0 hours (already built)

## Technical Design

### Backend

**New files:**
- `RecipeVault.Integrations.WaybackMachine/` — New project, HTTP client for web.archive.org
  - `IWaybackMachineClient.cs` — interface
  - `WaybackMachineClient.cs` — fetches archived pages
- `RecipeVault.DomainService/YummlyImportService.cs` — Yummly-specific JSON-LD parsing
- `RecipeVault.WebApi/Controllers/ImportController.cs` — Add `POST import/yummly` endpoint

**JSON-LD Parser:**
```csharp
// Yummly pages embed schema.org Recipe as JSON-LD
// 1. Fetch HTML from Wayback Machine
// 2. Extract <script type="application/ld+json"> from HTML
// 3. Deserialize to schema.org Recipe format
// 4. Map to our internal RecipeImportModel
```

**Wayback Machine API:**
- CDX API: `http://web.archive.org/cdx/search/cdx?url={url}&output=json&limit=1&fl=timestamp`
  - Returns most recent snapshot timestamp
- Fetch snapshot: `https://web.archive.org/web/{timestamp}id_/{url}`
  - `id_` flag returns original page without Wayback toolbar injection
- Rate limit: Be respectful — 1 request/second max, queue large batches

**Schema.org Recipe → RecipeVault Mapping:**

| schema.org | RecipeVault | Notes |
|---|---|---|
| name | Title | |
| description | Description | Truncate to 1000 chars |
| recipeIngredient[] | Ingredients (rawText) | Parse with Gemini for quantity/unit/item |
| recipeInstructions[] | Instructions | May be HowToStep objects or plain strings |
| prepTime (ISO 8601) | PrepTimeMinutes | Parse PT30M → 30 |
| cookTime (ISO 8601) | CookTimeMinutes | Parse PT1H30M → 90 |
| recipeYield | Yield | Extract number from "4 servings" |
| image / image[0].url | OriginalImageUrl | |
| url | Source | Original recipe source URL |

**ISO 8601 Duration Parser:**
```
PT30M → 30 minutes
PT1H → 60 minutes
PT1H30M → 90 minutes
PT2H15M → 135 minutes
```

### Frontend

**New UI in import dialog:**
- "Import from Yummly" tab with Yummly-themed icon/text
- Textarea for pasting URLs (one per line)
- "Recover Recipes" button
- Progress indicator (X of Y processed)
- Results: recovered recipes with preview, failed URLs with reason
- Bulk "Import All" or individual select

### Error Handling

| Error | User sees |
|---|---|
| URL not in Wayback Machine | "Recipe not archived — try screenshot import" |
| JSON-LD missing from page | Fall back to Gemini HTML parsing |
| Rate limited by archive.org | "Processing paused, resuming in X seconds" |
| Invalid Yummly URL format | "Not a valid Yummly recipe URL" |
| Partial success | Summary: "Recovered 8 of 12 recipes. 4 not available." |

## Scope for Weekend Launch

### Saturday — Backend
- [ ] WaybackMachine client project + HTTP client
- [ ] JSON-LD extraction from HTML
- [ ] Schema.org Recipe parser + mapping
- [ ] ISO 8601 duration parser
- [ ] `POST /api/v1/import/yummly` endpoint
- [ ] Rate limiting (1 req/sec to archive.org)
- [ ] Unit tests for parser + duration converter

### Sunday — Frontend + Polish
- [ ] Import dialog "Yummly" tab
- [ ] URL list input + validation
- [ ] Progress UI
- [ ] Recovery results display
- [ ] Error states
- [ ] Integration testing with real archived URLs

### Won't Do (Future)
- Yummly API integration (API is dead)
- Google Takeout parsing (uncertain data format)
- Automatic recipe discovery (user provides URLs)
- Yummly recipe box crawling (box pages likely not archived)

## Marketing Angle

**Landing page copy:**
> "Lost your Yummly recipes? We can help. RecipeVault recovers your saved Yummly recipes from web archives. Just paste your bookmarks and we'll do the rest."

**SEO keywords:** yummly alternative, yummly recipes export, yummly shutdown, save yummly recipes, yummly replacement, yummly closed

**Distribution:** Reddit r/Cooking, r/recipes, r/mealprep — organic posts about Yummly shutdown + recovery tool

## Risks

1. **Wayback Machine coverage** — Not all Yummly pages are archived. Mitigation: tell users upfront which recipes can't be recovered.
2. **Rate limiting** — archive.org may throttle us. Mitigation: 1 req/sec, queue large batches.
3. **JSON-LD format variation** — Yummly may have changed their markup over time. Mitigation: test with multiple archived pages from different dates.
4. **Legal** — Scraping archived pages for recipe data is fine (recipes aren't copyrightable, we're helping users recover their own data).

## Effort Estimate

| Component | Hours |
|---|---|
| WaybackMachine client | 2 |
| JSON-LD parser + schema.org mapping | 2 |
| ISO 8601 duration parser | 0.5 |
| Yummly import endpoint + service | 2 |
| Unit tests | 2 |
| Frontend import tab | 3 |
| Integration testing | 1 |
| **Total** | **~12 hours** |

Achievable in a weekend with focused effort.
