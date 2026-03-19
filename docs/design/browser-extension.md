# Browser Extension — Complete Design

## Overview

A Chrome extension (Manifest V3) that extracts recipe data from any website and saves it to RecipeVault. The extension uses intelligent heuristic extraction as the primary method, with fast-path shortcuts for known recipe formats (JSON-LD, Microdata). Users can edit extracted data before saving. Failed saves queue locally and retry when connectivity returns. The extension works across Chrome, Firefox, and Edge.

**Key Design Principles:**
- **On-demand extraction only** — no passive background scanning
- **Edit before save** — users fix extraction errors in the popup
- **Intelligent heuristics** — scores DOM sections for ingredients/instructions instead of brittle selectors
- **Offline resilience** — queues failed saves locally with automatic retry
- **Duplicate detection** — warns before saving recipes from URLs already in the library
- **Error telemetry** — opt-in anonymous reporting to improve extraction accuracy

---

## Use Cases

1. **Save from recipe sites** — User is browsing AllRecipes, clicks the extension icon, recipe is extracted, user edits title/ingredients, saves to RecipeVault
2. **Save from blogs** — Food blogger recipes with JSON-LD schema are cleanly extracted and editable before save
3. **Save from unstructured pages** — Heuristic scoring extracts best-effort recipe data from pages without structured markup
4. **Fix extraction errors** — User removes incorrect ingredients or edits prep time before saving
5. **Offline browsing** — User saves recipes while offline; extension queues them and retries when online
6. **Duplicate detection** — User clicks extension on a recipe they've already saved; warning appears with option to view existing or save anyway
7. **Batch browsing** — User browses multiple recipe sites, saving favorites as they go with edited metadata

---

## Data Model

### Structured Import DTO (Backend)

```csharp
public class ImportStructuredRequestDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int? Yield { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public string Source { get; set; }           // Original URL
    public string OriginalImageUrl { get; set; }
    public List<string> RawIngredients { get; set; }
    public List<string> RawInstructions { get; set; }
    public List<string> Categories { get; set; } // Mapped to tags
}
```

### HTML Import DTO (Backend) — NEW

```csharp
public class ImportHtmlRequestDto
{
    public string Html { get; set; }             // Raw HTML content of the page
    public string Source { get; set; }           // Original URL (for metadata, not fetching)
}
```

This allows the extension to send the page's HTML for server-side parsing when client-side extraction fails. The server can access the exact content the user sees (including paywalled recipes) without needing to fetch the URL itself.

### API Token Entity

```csharp
public class ApiToken
{
    public int ApiTokenId { get; set; }
    public Guid ApiTokenResourceId { get; set; }
    public Guid SubjectId { get; set; }
    public string Name { get; set; }               // "Chrome Extension"
    public string TokenHash { get; set; }           // SHA-256 hash of the token
    public string TokenPrefix { get; set; }         // First 8 chars for display: "rv_k8f2m..."
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime? ExpiresDate { get; set; }
    public bool IsRevoked { get; set; }

    public void MarkUsed()
    {
        LastUsedDate = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid()
    {
        return !IsRevoked && (!ExpiresDate.HasValue || ExpiresDate > DateTime.UtcNow);
    }
}
```

### Extracted Recipe (Extension-Side TypeScript)

```typescript
interface ExtractedRecipe {
    title: string;
    description: string;
    yield: number;
    prepTimeMinutes: number | null;
    cookTimeMinutes: number | null;
    originalImageUrl: string | null;
    rawIngredients: string[];
    rawInstructions: string[];
    categories: string[];
    source: string;             // window.location.href
    method: 'json-ld' | 'microdata' | 'heuristic';
    confidence: 'high' | 'medium' | 'low';
}
```

### Offline Queue Item (Extension-Side TypeScript)

```typescript
interface QueuedSave {
    id: string;                 // UUID for the queued item
    recipe: ExtractedRecipe;
    timestamp: number;          // Date.now() when queued
    retryCount: number;
    lastError: string | null;
}
```

### Error Telemetry Event (Extension-Side TypeScript)

```typescript
interface TelemetryEvent {
    eventType: 'extraction_failed' | 'save_failed' | 'api_error';
    method: 'json-ld' | 'microdata' | 'heuristic' | 'none';
    url: string;                // Domain only, no path (for privacy)
    errorMessage: string;
    timestamp: number;
    userAgent: string;
}
```

---

## API Design

### Import Structured Recipe

```
POST /api/v1/import/structured
```

Request body:

```json
{
    "title": "Classic Margherita Pizza",
    "description": "A simple, authentic Italian pizza...",
    "yield": 4,
    "prepTimeMinutes": 30,
    "cookTimeMinutes": 15,
    "source": "https://example.com/margherita-pizza",
    "originalImageUrl": "https://example.com/images/pizza.jpg",
    "rawIngredients": [
        "2 cups all-purpose flour",
        "1 cup warm water",
        "1 packet active dry yeast",
        "1/2 cup crushed San Marzano tomatoes",
        "8 oz fresh mozzarella",
        "Fresh basil leaves",
        "2 tbsp olive oil",
        "1 tsp salt"
    ],
    "rawInstructions": [
        "Combine flour, water, yeast, and salt. Knead for 10 minutes.",
        "Let dough rise for 1 hour.",
        "Stretch dough into a 12-inch circle.",
        "Top with crushed tomatoes, mozzarella, and basil.",
        "Bake at 500°F for 12-15 minutes."
    ],
    "categories": ["Italian", "Dinner", "Pizza"]
}
```

Response: Full `RecipeModel` of the created recipe (same as existing import endpoints).

### Import from HTML — NEW

```
POST /api/v1/import/html
```

**Purpose:** When client-side extraction fails (no structured data, heuristics fail), send the page's raw HTML for server-side parsing. This works for paywalled content because we send the HTML the user sees, not the URL for the server to fetch.

Request body:

```json
{
    "html": "<!DOCTYPE html><html><body>...full page HTML...</body></html>",
    "source": "https://example.com/paywalled-recipe"
}
```

Response: Full `RecipeModel` of the created recipe.

**Implementation:** Server-side extraction uses the same heuristic scoring logic as the client, but runs on the backend. This ensures consistent extraction behavior and allows the server to handle complex cases that might timeout in the extension.

### Check for Duplicate Recipe — NEW

```
GET /api/v1/recipes/check-source?url={encodedUrl}
```

**Purpose:** Before saving, check if a recipe with the same source URL already exists in the user's library.

Query parameter:
- `url` (string, required) — URL-encoded source URL

Response when recipe exists:

```json
{
    "exists": true,
    "recipeResourceId": "a1b2c3d4-...",
    "title": "Classic Margherita Pizza",
    "createdDate": "2026-02-15T14:30:00Z"
}
```

Response when recipe does not exist:

```json
{
    "exists": false
}
```

**Implementation:** Query `Recipe` table WHERE `Source = @url AND SubjectId = @subjectId`. Uses an index on `(SubjectId, Source)` for fast lookups.

### Token Management

#### Create API Token

```
POST /api/v1/user/api-tokens
```

Request:

```json
{
    "name": "Chrome Extension",
    "expiresInDays": 365
}
```

Response:

```json
{
    "apiTokenResourceId": "a1b2c3d4-...",
    "name": "Chrome Extension",
    "token": "rv_k8f2m9x1p4n7j3b6_a0c5e8g1i4l7o0r3u6x9b2d5f8h1",
    "tokenPrefix": "rv_k8f2m",
    "expiresDate": "2027-03-13T00:00:00Z",
    "createdDate": "2026-03-13T00:00:00Z"
}
```

The full `token` value is returned only once at creation time. It is never stored in plaintext — only the SHA-256 hash is persisted.

#### List API Tokens

```
GET /api/v1/user/api-tokens
```

Response:

```json
{
    "tokens": [
        {
            "apiTokenResourceId": "a1b2c3d4-...",
            "name": "Chrome Extension",
            "tokenPrefix": "rv_k8f2m",
            "createdDate": "2026-03-13T00:00:00Z",
            "lastUsedDate": "2026-03-13T14:30:00Z",
            "expiresDate": "2027-03-13T00:00:00Z",
            "isRevoked": false
        }
    ]
}
```

#### Revoke API Token

```
DELETE /api/v1/user/api-tokens/{apiTokenResourceId}
```

Response: `204 No Content`

### Auth Token Validation

```
GET /api/v1/user/me
```

Returns basic user info or 401 if token is expired. Used by the extension to verify connectivity.

### Submit Telemetry — NEW

```
POST /api/v1/telemetry/extension
```

**Purpose:** Opt-in anonymous error reporting to help improve extraction accuracy.

Request body:

```json
{
    "events": [
        {
            "eventType": "extraction_failed",
            "method": "none",
            "url": "example.com",
            "errorMessage": "No recipe data found",
            "timestamp": 1710345600000,
            "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/122.0.0.0"
        }
    ]
}
```

Response: `204 No Content`

**Privacy:** Only sends domain (no path/query), anonymized error messages (no user data), aggregated in batches.

---

## UX Design

### Extension Popup (Default State — Not Authenticated)

```
┌──────────────────────────────────────┐
│  RecipeVault                         │
│                                      │
│  Sign in to save recipes.            │
│                                      │
│  ┌──────────────────────────────┐    │
│  │ API Token                    │    │
│  │ ____________________________│    │
│  └──────────────────────────────┘    │
│                                      │
│  Paste your API token from           │
│  RecipeVault Settings > API Tokens.  │
│                                      │
│  [Connect]                           │
│                                      │
└──────────────────────────────────────┘
```

### Extension Popup (Recipe Detected — Editable)

When the user clicks the extension icon on a page with a recipe, the extracted data is shown in editable fields:

```
┌──────────────────────────────────────┐
│  RecipeVault              [Settings] │
│                                      │
│  Recipe Extracted                    │
│                                      │
│  Title:                              │
│  ┌──────────────────────────────┐    │
│  │ Classic Margherita Pizza    │    │
│  └──────────────────────────────┘    │
│                                      │
│  Description:                        │
│  ┌──────────────────────────────┐    │
│  │ A simple, authentic...       │    │
│  └──────────────────────────────┘    │
│                                      │
│  Yield: [4]  Prep: [30] min          │
│  Cook: [15] min                      │
│                                      │
│  Ingredients (8):                    │
│  ┌──────────────────────────────┐    │
│  │ • 2 cups flour         [×]   │    │
│  │ • 1 cup water          [×]   │    │
│  │ • 1 packet yeast       [×]   │    │
│  │ ...                          │    │
│  └──────────────────────────────┘    │
│                                      │
│  Instructions (5):                   │
│  ┌──────────────────────────────┐    │
│  │ 1. Combine flour...    [×]   │    │
│  │ 2. Let dough rise...   [×]   │    │
│  │ ...                          │    │
│  └──────────────────────────────┘    │
│                                      │
│  Extracted via: JSON-LD (high conf.) │
│                                      │
│  [Save to RecipeVault]               │
│                                      │
└──────────────────────────────────────┘
```

**Edit Controls:**
- Title, description, yield, prep/cook time: all editable text fields
- Each ingredient has a remove button (×)
- Each instruction has a remove button (×)
- Save button disabled until title is non-empty

### Extension Popup (Duplicate Detected)

When the user tries to save a recipe from a URL already in their library:

```
┌──────────────────────────────────────┐
│  RecipeVault              [Settings] │
│                                      │
│  ⚠️ Already Saved                    │
│                                      │
│  You've already saved this recipe:   │
│                                      │
│  "Classic Margherita Pizza"          │
│  Saved on Feb 15, 2026               │
│                                      │
│  [View Existing]                     │
│  [Save Another Copy]                 │
│  [Cancel]                            │
│                                      │
└──────────────────────────────────────┘
```

**Behavior:**
- **View Existing** — Opens the existing recipe in RecipeVault web app (new tab)
- **Save Another Copy** — Proceeds with save (allows duplicates if user wants them)
- **Cancel** — Closes popup, discards current extraction

### Extension Popup (No Recipe Detected)

```
┌──────────────────────────────────────┐
│  RecipeVault              [Settings] │
│                                      │
│  No recipe found on this page.       │
│                                      │
│  [Try Server-Side Import]            │
│  Sends the page content to           │
│  RecipeVault for AI-powered          │
│  extraction.                         │
│                                      │
└──────────────────────────────────────┘
```

**Behavior:**
- "Try Server-Side Import" sends the page's HTML (not URL) to `POST /api/v1/import/html`
- Shows loading state while server processes
- On success, shows editable recipe preview
- On failure, shows error message

### Extension Popup (Save Success)

```
┌──────────────────────────────────────┐
│  RecipeVault              [Settings] │
│                                      │
│  ✓ Saved!                            │
│                                      │
│  Classic Margherita Pizza has been   │
│  added to your recipe library.       │
│                                      │
│  [View in RecipeVault]  [Done]       │
│                                      │
└──────────────────────────────────────┘
```

### Extension Popup (Offline — Queued)

When save fails due to network error:

```
┌──────────────────────────────────────┐
│  RecipeVault              [Settings] │
│                                      │
│  📡 Queued for Later                 │
│                                      │
│  Recipe saved locally. It will be    │
│  uploaded when you're back online.   │
│                                      │
│  3 recipes queued                    │
│                                      │
│  [View Queue]  [Done]                │
│                                      │
└──────────────────────────────────────┘
```

### Settings Page

```
┌──────────────────────────────────────┐
│  RecipeVault Settings                │
│                                      │
│  Server URL:                         │
│  ┌──────────────────────────────┐    │
│  │ https://api.recipevault.app  │    │
│  └──────────────────────────────┘    │
│                                      │
│  API Token:                          │
│  ┌──────────────────────────────┐    │
│  │ rv_k8f2m•••••••••••••••     │    │
│  └──────────────────────────────┘    │
│  Status: Connected ✓                 │
│                                      │
│  Offline Queue (3 items):            │
│  ┌──────────────────────────────┐    │
│  │ • Chocolate Chip Cookies     │    │
│  │   Queued 2 hours ago  [Retry]│    │
│  │ • Beef Stew                  │    │
│  │   Queued 1 hour ago   [Retry]│    │
│  │ • Thai Green Curry           │    │
│  │   Queued 5 min ago    [Retry]│    │
│  └──────────────────────────────┘    │
│                                      │
│  [Clear Queue]                       │
│                                      │
│  Telemetry:                          │
│  ☑ Send anonymous usage data         │
│      (helps improve recipe           │
│       extraction)                    │
│                                      │
│  [Disconnect]  [Save Settings]       │
│                                      │
└──────────────────────────────────────┘
```

### Badge Indicator

The extension icon shows a badge when a recipe is detected on the current tab:

- **Green dot** — Recipe detected via JSON-LD or Microdata (high confidence)
- **Yellow dot** — Recipe detected via heuristics (medium/low confidence)
- **Red badge with number** — Offline queue count (e.g., "3" when 3 recipes queued)
- **No badge** — No recipe detected

**Performance:** Badge updates on `chrome.tabs.onActivated` (when user switches tabs) and when popup opens. Does NOT listen to `tabs.onUpdated` globally. Uses debounce (300ms) to avoid excessive checks.

---

## Implementation

### Database Migration

```sql
-- API tokens
CREATE TABLE "ApiToken" (
    "ApiTokenId" INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "ApiTokenResourceId" UUID NOT NULL,
    "SubjectId" UUID NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "TokenHash" VARCHAR(64) NOT NULL,
    "TokenPrefix" VARCHAR(10) NOT NULL,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastUsedDate" TIMESTAMP WITH TIME ZONE NULL,
    "ExpiresDate" TIMESTAMP WITH TIME ZONE NULL,
    "IsRevoked" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX "IX_ApiToken_ResourceId" ON "ApiToken"("ApiTokenResourceId");
CREATE UNIQUE INDEX "IX_ApiToken_TokenHash" ON "ApiToken"("TokenHash");
CREATE INDEX "IX_ApiToken_SubjectId" ON "ApiToken"("SubjectId");

-- Index for duplicate detection
CREATE INDEX "IX_Recipe_SubjectId_Source" ON "Recipe"("SubjectId", "Source");

-- Extension telemetry (optional, for analytics)
CREATE TABLE "ExtensionTelemetry" (
    "TelemetryId" INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "EventType" VARCHAR(50) NOT NULL,
    "Method" VARCHAR(20),
    "Url" VARCHAR(500),
    "ErrorMessage" TEXT,
    "Timestamp" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UserAgent" VARCHAR(500)
);

CREATE INDEX "IX_ExtensionTelemetry_Timestamp" ON "ExtensionTelemetry"("Timestamp");
CREATE INDEX "IX_ExtensionTelemetry_EventType" ON "ExtensionTelemetry"("EventType");
```

### Project Structure

```
extension/
├── manifest.json
├── popup/
│   ├── popup.html
│   ├── popup.js
│   └── popup.css
├── settings/
│   ├── settings.html
│   ├── settings.js
│   └── settings.css
├── background/
│   └── service-worker.js   # Background service worker
├── lib/
│   ├── extractor.js         # Recipe extraction logic (injected on-demand)
│   ├── api-client.js        # RecipeVault API client
│   ├── queue-manager.js     # Offline queue management
│   └── telemetry.js         # Error telemetry
├── icons/
│   ├── icon-16.png
│   ├── icon-48.png
│   └── icon-128.png
└── _locales/
    └── en/
        └── messages.json
```

**Key Change:** No global content script. `extractor.js` is injected dynamically when the user clicks the extension icon.

### Manifest V3

```json
{
    "manifest_version": 3,
    "name": "RecipeVault",
    "version": "1.0.0",
    "description": "Save recipes from any website to your RecipeVault library.",
    "permissions": [
        "activeTab",
        "storage",
        "scripting"
    ],
    "host_permissions": [],
    "action": {
        "default_popup": "popup/popup.html",
        "default_icon": {
            "16": "icons/icon-16.png",
            "48": "icons/icon-48.png",
            "128": "icons/icon-128.png"
        }
    },
    "background": {
        "service_worker": "background/service-worker.js"
    },
    "options_page": "settings/settings.html",
    "icons": {
        "16": "icons/icon-16.png",
        "48": "icons/icon-48.png",
        "128": "icons/icon-128.png"
    }
}
```

**Key Changes:**
- **No `content_scripts`** — extraction happens on-demand via `chrome.scripting.executeScript`
- **`scripting` permission** — allows dynamic script injection
- **`activeTab` only** — no `<all_urls>` permission

### Background Service Worker

```javascript
// background/service-worker.js

import { checkForRecipe } from './lib/extractor.js';
import { getQueueCount } from './lib/queue-manager.js';

let debounceTimer = null;

// Update badge when user switches tabs (debounced)
chrome.tabs.onActivated.addListener(async (activeInfo) => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        updateBadgeForTab(activeInfo.tabId);
    }, 300);
});

// Update badge for a specific tab
async function updateBadgeForTab(tabId) {
    try {
        // Inject extractor and check for recipe
        const results = await chrome.scripting.executeScript({
            target: { tabId },
            func: checkForRecipe
        });

        const recipe = results?.[0]?.result;

        if (recipe) {
            const color = recipe.confidence === 'high' ? '#4CAF50' : '#FFC107';
            chrome.action.setBadgeBackgroundColor({ color, tabId });
            chrome.action.setBadgeText({ text: '●', tabId });
        } else {
            // Check if offline queue has items
            const queueCount = await getQueueCount();
            if (queueCount > 0) {
                chrome.action.setBadgeBackgroundColor({ color: '#F44336', tabId });
                chrome.action.setBadgeText({ text: String(queueCount), tabId });
            } else {
                chrome.action.setBadgeText({ text: '', tabId });
            }
        }
    } catch (error) {
        // Tab not accessible (chrome:// pages, etc.) or injection failed
        chrome.action.setBadgeText({ text: '', tabId });
    }
}

// Export for use by popup
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'updateBadge') {
        updateBadgeForTab(request.tabId).then(() => sendResponse({ success: true }));
        return true; // Keep channel open for async
    }
});
```

**Key Changes:**
- Uses `chrome.tabs.onActivated` (when user switches tabs) instead of `tabs.onUpdated` (fires constantly)
- Debounces badge updates (300ms) to avoid excessive checks
- Injects extractor on-demand, doesn't run on every page load

### Recipe Extractor (Injected On-Demand)

```javascript
// lib/extractor.js

// This function is injected into the page context when the popup opens
// It runs in the page's scope and has access to the DOM

export function checkForRecipe() {
    // Strategy 1: JSON-LD (fast path)
    const jsonLd = extractJsonLd();
    if (jsonLd) return { ...jsonLd, method: 'json-ld', confidence: 'high' };

    // Strategy 2: Microdata (fast path)
    const microdata = extractMicrodata();
    if (microdata) return { ...microdata, method: 'microdata', confidence: 'high' };

    // Strategy 3: Heuristic extraction (primary fallback)
    const heuristic = extractViaHeuristics();
    if (heuristic) return heuristic;

    return null;
}

function extractJsonLd() {
    const scripts = document.querySelectorAll('script[type="application/ld+json"]');
    for (const script of scripts) {
        try {
            const data = JSON.parse(script.textContent);
            const recipe = findRecipeInGraph(data);
            if (recipe) return parseSchemaRecipe(recipe);
        } catch (e) {
            // Invalid JSON, skip
        }
    }
    return null;
}

function findRecipeInGraph(data) {
    // Handle @graph arrays
    if (data['@graph']) {
        return data['@graph'].find(item =>
            item['@type'] === 'Recipe' ||
            (Array.isArray(item['@type']) && item['@type'].includes('Recipe'))
        );
    }
    // Handle arrays
    if (Array.isArray(data)) {
        for (const item of data) {
            const found = findRecipeInGraph(item);
            if (found) return found;
        }
        return null;
    }
    // Handle direct Recipe object
    if (data['@type'] === 'Recipe') return data;
    if (Array.isArray(data['@type']) && data['@type'].includes('Recipe')) return data;
    return null;
}

function parseSchemaRecipe(schema) {
    return {
        title: schema.name || '',
        description: schema.description || '',
        yield: parseYield(schema.recipeYield),
        prepTimeMinutes: parseIsoDuration(schema.prepTime),
        cookTimeMinutes: parseIsoDuration(schema.cookTime),
        originalImageUrl: parseImage(schema.image),
        rawIngredients: Array.isArray(schema.recipeIngredient)
            ? schema.recipeIngredient
            : [],
        rawInstructions: parseInstructions(schema.recipeInstructions),
        categories: [
            ...(schema.recipeCategory ? [].concat(schema.recipeCategory) : []),
            ...(schema.recipeCuisine ? [].concat(schema.recipeCuisine) : [])
        ],
        source: window.location.href
    };
}

function extractMicrodata() {
    const recipeEl = document.querySelector('[itemtype*="schema.org/Recipe"]');
    if (!recipeEl) return null;

    const getProp = (name) => {
        const el = recipeEl.querySelector(`[itemprop="${name}"]`);
        if (!el) return null;
        return el.content || el.textContent?.trim() || null;
    };

    const getProps = (name) => {
        return Array.from(recipeEl.querySelectorAll(`[itemprop="${name}"]`))
            .map(el => el.content || el.textContent?.trim())
            .filter(Boolean);
    };

    return {
        title: getProp('name') || '',
        description: getProp('description') || '',
        yield: parseYield(getProp('recipeYield')),
        prepTimeMinutes: parseIsoDuration(getProp('prepTime')),
        cookTimeMinutes: parseIsoDuration(getProp('cookTime')),
        originalImageUrl: recipeEl.querySelector('[itemprop="image"]')?.src ||
                          recipeEl.querySelector('[itemprop="image"]')?.content || null,
        rawIngredients: getProps('recipeIngredient'),
        rawInstructions: getProps('recipeInstructions'),
        categories: getProps('recipeCategory').concat(getProps('recipeCuisine')),
        source: window.location.href
    };
}

// HEURISTIC EXTRACTION — Primary fallback method
function extractViaHeuristics() {
    // Fast-path shortcuts for known recipe plugins (still useful but not primary)
    const knownSelectors = tryKnownSelectors();
    if (knownSelectors) return knownSelectors;

    // Primary heuristic: score DOM sections for ingredients and instructions
    const ingredients = findIngredientList();
    const instructions = findInstructionList();

    if (!ingredients.items.length && !instructions.items.length) {
        return null;
    }

    const title = findTitle();
    const image = findImage();
    const confidence = calculateConfidence(ingredients.score, instructions.score);

    return {
        title,
        description: findDescription(),
        yield: 4, // Default, heuristics rarely provide yield
        prepTimeMinutes: null,
        cookTimeMinutes: null,
        originalImageUrl: image,
        rawIngredients: ingredients.items,
        rawInstructions: instructions.items,
        categories: [],
        source: window.location.href,
        method: 'heuristic',
        confidence
    };
}

// Try known selectors as fast-path shortcuts
function tryKnownSelectors() {
    const knownPatterns = [
        {
            ingredients: '.wprm-recipe-ingredient',
            instructions: '.wprm-recipe-instruction-text',
            name: 'WPRM'
        },
        {
            ingredients: '.tasty-recipe-ingredients li',
            instructions: '.tasty-recipe-instructions li',
            name: 'Tasty'
        },
        {
            ingredients: '.mv-create-ingredients li',
            instructions: '.mv-create-instructions li',
            name: 'Mediavine'
        }
    ];

    for (const pattern of knownPatterns) {
        const ingredientEls = document.querySelectorAll(pattern.ingredients);
        const instructionEls = document.querySelectorAll(pattern.instructions);

        if (ingredientEls.length > 0 && instructionEls.length > 0) {
            return {
                title: findTitle(),
                description: findDescription(),
                yield: 4,
                prepTimeMinutes: null,
                cookTimeMinutes: null,
                originalImageUrl: findImage(),
                rawIngredients: Array.from(ingredientEls).map(el => el.textContent.trim()),
                rawInstructions: Array.from(instructionEls).map(el => el.textContent.trim()),
                categories: [],
                source: window.location.href,
                method: 'heuristic',
                confidence: 'high' // Known selectors are reliable
            };
        }
    }

    return null;
}

// Find the most likely ingredient list using heuristic scoring
function findIngredientList() {
    const lists = document.querySelectorAll('ul, ol');
    let bestMatch = { items: [], score: 0 };

    for (const list of lists) {
        const items = Array.from(list.querySelectorAll('li'))
            .map(li => li.textContent.trim())
            .filter(text => text.length > 0 && text.length < 200);

        if (items.length < 3) continue; // Too short to be ingredient list

        const score = scoreAsIngredients(items);
        if (score > bestMatch.score) {
            bestMatch = { items, score };
        }
    }

    return bestMatch;
}

// Score a list of items by how likely they are to be ingredients
function scoreAsIngredients(items) {
    const ingredientKeywords = [
        'cup', 'cups', 'tablespoon', 'tablespoons', 'tbsp', 'teaspoon', 'teaspoons', 'tsp',
        'ounce', 'ounces', 'oz', 'pound', 'pounds', 'lb', 'lbs', 'gram', 'grams', 'g',
        'kilogram', 'kg', 'ml', 'liter', 'pinch', 'dash', 'clove', 'cloves',
        'chopped', 'diced', 'minced', 'sliced', 'grated', 'crushed', 'fresh', 'dried',
        'flour', 'sugar', 'salt', 'pepper', 'oil', 'butter', 'eggs', 'milk', 'water',
        'onion', 'garlic', 'tomato', 'chicken', 'beef', 'pork', 'cheese', 'basil'
    ];

    const quantityPattern = /^\d+[\d\/\s]*/; // Starts with a number
    let score = 0;

    for (const item of items) {
        const lower = item.toLowerCase();

        // +10 points for quantity at start
        if (quantityPattern.test(item)) score += 10;

        // +5 points per ingredient keyword
        for (const keyword of ingredientKeywords) {
            if (lower.includes(keyword)) {
                score += 5;
                break; // Only count once per item
            }
        }

        // -5 points for instruction-like phrases
        if (lower.includes('preheat') || lower.includes('bake') || lower.includes('stir')) {
            score -= 5;
        }
    }

    return score;
}

// Find the most likely instruction list using heuristic scoring
function findInstructionList() {
    const lists = document.querySelectorAll('ul, ol');
    let bestMatch = { items: [], score: 0 };

    for (const list of lists) {
        const items = Array.from(list.querySelectorAll('li'))
            .map(li => li.textContent.trim())
            .filter(text => text.length > 20); // Instructions are longer than ingredients

        if (items.length < 2) continue; // Too short to be instruction list

        const score = scoreAsInstructions(items);
        if (score > bestMatch.score) {
            bestMatch = { items, score };
        }
    }

    return bestMatch;
}

// Score a list of items by how likely they are to be instructions
function scoreAsInstructions(items) {
    const instructionKeywords = [
        'preheat', 'heat', 'bake', 'cook', 'stir', 'mix', 'combine', 'whisk', 'fold',
        'pour', 'add', 'place', 'remove', 'serve', 'let', 'set', 'refrigerate', 'freeze',
        'until', 'about', 'minutes', 'degrees', 'oven', 'pan', 'bowl'
    ];

    let score = 0;

    for (const item of items) {
        const lower = item.toLowerCase();

        // +10 points for starting with a verb (imperative mood)
        const firstWord = lower.split(' ')[0];
        if (instructionKeywords.includes(firstWord)) {
            score += 10;
        }

        // +3 points per instruction keyword
        for (const keyword of instructionKeywords) {
            if (lower.includes(keyword)) {
                score += 3;
            }
        }

        // +5 points for being long (instructions are typically sentences)
        if (item.length > 50) score += 5;

        // -10 points if it looks like an ingredient (starts with quantity)
        if (/^\d+[\d\/\s]*/.test(item)) {
            score -= 10;
        }
    }

    return score;
}

// Calculate overall confidence based on ingredient and instruction scores
function calculateConfidence(ingredientScore, instructionScore) {
    const totalScore = ingredientScore + instructionScore;
    if (totalScore > 100) return 'high';
    if (totalScore > 50) return 'medium';
    return 'low';
}

function findTitle() {
    // Try h1, then og:title meta, then document title
    const h1 = document.querySelector('h1')?.textContent?.trim();
    if (h1) return h1;

    const ogTitle = document.querySelector('meta[property="og:title"]')?.content;
    if (ogTitle) return ogTitle;

    return document.title || 'Untitled Recipe';
}

function findDescription() {
    const ogDesc = document.querySelector('meta[property="og:description"]')?.content;
    if (ogDesc) return ogDesc;

    const metaDesc = document.querySelector('meta[name="description"]')?.content;
    if (metaDesc) return metaDesc;

    return '';
}

function findImage() {
    const ogImage = document.querySelector('meta[property="og:image"]')?.content;
    if (ogImage) return resolveUrl(ogImage);

    // Find the largest image in the main content area
    const contentImages = document.querySelectorAll('article img, main img, .recipe img');
    let largestImage = null;
    let maxArea = 0;

    for (const img of contentImages) {
        const area = img.width * img.height;
        if (area > maxArea) {
            maxArea = area;
            largestImage = img;
        }
    }

    return largestImage ? resolveUrl(largestImage.src) : null;
}

// Helper functions (same as before)

function parseIsoDuration(duration) {
    if (!duration) return null;
    const match = duration.match(/PT(?:(\d+)H)?(?:(\d+)M)?/);
    if (!match) return null;
    return (parseInt(match[1] || 0) * 60) + parseInt(match[2] || 0);
}

function parseYield(recipeYield) {
    if (!recipeYield) return 4;
    if (typeof recipeYield === 'number') return recipeYield;
    const str = Array.isArray(recipeYield) ? recipeYield[0] : recipeYield;
    const match = String(str).match(/\d+/);
    return match ? parseInt(match[0]) : 4;
}

function parseImage(image) {
    if (!image) return null;
    if (typeof image === 'string') return resolveUrl(image);
    if (Array.isArray(image)) return parseImage(image[0]);
    if (image.url) return resolveUrl(image.url);
    return null;
}

function resolveUrl(url) {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    try {
        return new URL(url, window.location.origin).href;
    } catch {
        return url;
    }
}

function parseInstructions(instructions) {
    if (!instructions) return [];
    if (typeof instructions === 'string') return [instructions];
    return instructions.flatMap(step => {
        if (typeof step === 'string') return [step];
        if (step['@type'] === 'HowToStep') return [step.text || ''];
        if (step['@type'] === 'HowToSection') {
            return (step.itemListElement || []).map(s => s.text || '');
        }
        return [];
    }).filter(Boolean);
}
```

**Key Changes:**
- **Heuristic scoring is primary** — not hardcoded selectors
- **Known selectors are fast-path shortcuts** — still useful but secondary
- **Scoring logic** — uses keyword matching and structural analysis to identify ingredients vs instructions
- **Confidence calculation** — based on total score, not just extraction method

### Popup Logic (with Edit-Before-Save)

```javascript
// popup/popup.js

import { apiCall, checkDuplicate } from './lib/api-client.js';
import { queueSave, getQueue, retryQueue } from './lib/queue-manager.js';
import { reportTelemetry } from './lib/telemetry.js';

const state = {
    token: null,
    serverUrl: null,
    recipe: null,
    saving: false,
    duplicateCheck: null
};

const init = async () => {
    const stored = await chrome.storage.local.get(['apiToken', 'serverUrl']);
    state.token = stored.apiToken;
    state.serverUrl = stored.serverUrl || 'https://api.recipevault.app';

    if (!state.token) {
        showLoginView();
        return;
    }

    // Verify token
    try {
        await apiCall(state.serverUrl, state.token, 'GET', '/user/me');
    } catch {
        showLoginView('Token expired. Please reconnect.');
        return;
    }

    // Inject extractor and get recipe from current tab
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    
    try {
        const results = await chrome.scripting.executeScript({
            target: { tabId: tab.id },
            files: ['lib/extractor.js'],
            world: 'MAIN'
        });

        const recipe = results?.[0]?.result;

        if (recipe) {
            state.recipe = recipe;
            
            // Check for duplicate before showing preview
            try {
                state.duplicateCheck = await checkDuplicate(
                    state.serverUrl,
                    state.token,
                    recipe.source
                );
                
                if (state.duplicateCheck.exists) {
                    showDuplicateWarning(state.duplicateCheck);
                } else {
                    showRecipeEditor(recipe);
                }
            } catch (err) {
                // Duplicate check failed — show editor anyway
                console.warn('Duplicate check failed:', err);
                showRecipeEditor(recipe);
            }
        } else {
            showNoRecipeView(tab.url);
            await reportTelemetry({
                eventType: 'extraction_failed',
                method: 'none',
                url: new URL(tab.url).hostname,
                errorMessage: 'No recipe found',
                timestamp: Date.now(),
                userAgent: navigator.userAgent
            });
        }
    } catch (error) {
        showNoRecipeView(tab.url);
        console.error('Extraction error:', error);
    }
};

function showRecipeEditor(recipe) {
    document.getElementById('loading').hidden = true;
    document.getElementById('editor').hidden = false;

    // Populate editable fields
    document.getElementById('edit-title').value = recipe.title;
    document.getElementById('edit-description').value = recipe.description;
    document.getElementById('edit-yield').value = recipe.yield;
    document.getElementById('edit-prep').value = recipe.prepTimeMinutes || '';
    document.getElementById('edit-cook').value = recipe.cookTimeMinutes || '';

    // Populate ingredient list (with remove buttons)
    const ingredientList = document.getElementById('ingredient-list');
    ingredientList.innerHTML = '';
    recipe.rawIngredients.forEach((ingredient, index) => {
        const li = document.createElement('li');
        li.innerHTML = `
            <span>${ingredient}</span>
            <button class="remove-btn" data-index="${index}">×</button>
        `;
        ingredientList.appendChild(li);
    });

    // Populate instruction list (with remove buttons)
    const instructionList = document.getElementById('instruction-list');
    instructionList.innerHTML = '';
    recipe.rawInstructions.forEach((instruction, index) => {
        const li = document.createElement('li');
        li.innerHTML = `
            <span>${index + 1}. ${instruction}</span>
            <button class="remove-btn" data-index="${index}">×</button>
        `;
        instructionList.appendChild(li);
    });

    // Show extraction method and confidence
    document.getElementById('method-info').textContent =
        `Extracted via: ${recipe.method} (${recipe.confidence} confidence)`;

    // Set up remove button handlers
    document.querySelectorAll('.remove-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.target.closest('li').remove();
        });
    });
}

function showDuplicateWarning(duplicateInfo) {
    document.getElementById('loading').hidden = true;
    document.getElementById('duplicate-warning').hidden = false;

    document.getElementById('duplicate-title').textContent = duplicateInfo.title;
    document.getElementById('duplicate-date').textContent =
        new Date(duplicateInfo.createdDate).toLocaleDateString();

    document.getElementById('view-existing-btn').addEventListener('click', () => {
        chrome.tabs.create({
            url: `${state.serverUrl.replace('/api', '')}/recipes/${duplicateInfo.recipeResourceId}`
        });
        window.close();
    });

    document.getElementById('save-anyway-btn').addEventListener('click', () => {
        showRecipeEditor(state.recipe);
    });

    document.getElementById('cancel-btn').addEventListener('click', () => {
        window.close();
    });
}

async function saveRecipe() {
    // Collect edited values from form
    const editedRecipe = {
        title: document.getElementById('edit-title').value.trim(),
        description: document.getElementById('edit-description').value.trim(),
        yield: parseInt(document.getElementById('edit-yield').value) || 4,
        prepTimeMinutes: parseInt(document.getElementById('edit-prep').value) || null,
        cookTimeMinutes: parseInt(document.getElementById('edit-cook').value) || null,
        source: state.recipe.source,
        originalImageUrl: state.recipe.originalImageUrl,
        rawIngredients: Array.from(document.querySelectorAll('#ingredient-list li span'))
            .map(span => span.textContent),
        rawInstructions: Array.from(document.querySelectorAll('#instruction-list li span'))
            .map(span => span.textContent.replace(/^\d+\.\s*/, '')), // Remove numbering
        categories: state.recipe.categories
    };

    if (!editedRecipe.title) {
        alert('Title is required');
        return;
    }

    state.saving = true;
    updateSaveButton('Saving...', true);

    try {
        const result = await apiCall(state.serverUrl, state.token, 'POST', '/import/structured', editedRecipe);
        showSuccessView(result.recipeResourceId);
    } catch (error) {
        if (error.status === 429) {
            showError('Rate limited. Please wait a moment and try again.');
        } else if (!navigator.onLine) {
            // Network error — queue for later
            await queueSave(editedRecipe);
            showQueuedView();
        } else {
            showError(`Failed to save: ${error.message}`);
            await reportTelemetry({
                eventType: 'save_failed',
                method: state.recipe.method,
                url: new URL(state.recipe.source).hostname,
                errorMessage: error.message,
                timestamp: Date.now(),
                userAgent: navigator.userAgent
            });
        }
    } finally {
        state.saving = false;
    }
}

async function serverSideImport() {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });

    updateSaveButton('Importing...', true);

    try {
        // Get the page's HTML content by injecting a script
        const results = await chrome.scripting.executeScript({
            target: { tabId: tab.id },
            func: () => document.documentElement.outerHTML
        });

        const html = results?.[0]?.result;
        if (!html) throw new Error('Failed to get page content');

        const result = await apiCall(state.serverUrl, state.token, 'POST', '/import/html', {
            html,
            source: tab.url
        });

        showSuccessView(result.recipeResourceId);
    } catch (error) {
        showError(`Import failed: ${error.message}`);
        await reportTelemetry({
            eventType: 'save_failed',
            method: 'html',
            url: new URL(tab.url).hostname,
            errorMessage: error.message,
            timestamp: Date.now(),
            userAgent: navigator.userAgent
        });
    }
}

function showQueuedView() {
    document.getElementById('editor').hidden = true;
    document.getElementById('queued').hidden = false;

    getQueue().then(queue => {
        document.getElementById('queue-count').textContent = `${queue.length} recipes queued`;
    });
}

function showSuccessView(recipeResourceId) {
    document.getElementById('editor').hidden = true;
    document.getElementById('success').hidden = false;

    document.getElementById('view-recipe-btn').addEventListener('click', () => {
        chrome.tabs.create({
            url: `${state.serverUrl.replace('/api', '')}/recipes/${recipeResourceId}`
        });
        window.close();
    });
}

// Auto-retry queued saves when online
window.addEventListener('online', async () => {
    const queue = await getQueue();
    if (queue.length > 0) {
        await retryQueue(state.serverUrl, state.token);
        // Update badge to reflect new queue count
        const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
        chrome.runtime.sendMessage({ action: 'updateBadge', tabId: tab.id });
    }
});

document.addEventListener('DOMContentLoaded', init);
document.getElementById('save-btn')?.addEventListener('click', saveRecipe);
document.getElementById('server-import-btn')?.addEventListener('click', serverSideImport);
```

**Key Changes:**
- **Edit before save** — all fields are editable, ingredients/instructions removable
- **Duplicate detection** — checks before showing editor, shows warning if duplicate exists
- **Offline queue** — failed saves go to local queue, retry when online
- **Error telemetry** — reports extraction/save failures anonymously

### Offline Queue Manager

```javascript
// lib/queue-manager.js

const QUEUE_KEY = 'offlineQueue';

export async function queueSave(recipe) {
    const queue = await getQueue();
    const item = {
        id: crypto.randomUUID(),
        recipe,
        timestamp: Date.now(),
        retryCount: 0,
        lastError: null
    };
    queue.push(item);
    await chrome.storage.local.set({ [QUEUE_KEY]: queue });
    return item;
}

export async function getQueue() {
    const result = await chrome.storage.local.get(QUEUE_KEY);
    return result[QUEUE_KEY] || [];
}

export async function getQueueCount() {
    const queue = await getQueue();
    return queue.length;
}

export async function removeFromQueue(id) {
    const queue = await getQueue();
    const filtered = queue.filter(item => item.id !== id);
    await chrome.storage.local.set({ [QUEUE_KEY]: filtered });
}

export async function retryQueue(serverUrl, token) {
    const queue = await getQueue();
    const results = [];

    for (const item of queue) {
        try {
            await apiCall(serverUrl, token, 'POST', '/import/structured', item.recipe);
            await removeFromQueue(item.id);
            results.push({ id: item.id, success: true });
        } catch (error) {
            item.retryCount++;
            item.lastError = error.message;
            results.push({ id: item.id, success: false, error: error.message });

            // If retry count exceeds 5, keep in queue but don't retry again this session
            if (item.retryCount <= 5) {
                await chrome.storage.local.set({ [QUEUE_KEY]: queue });
            }
        }
    }

    return results;
}

export async function clearQueue() {
    await chrome.storage.local.set({ [QUEUE_KEY]: [] });
}
```

### Error Telemetry

```javascript
// lib/telemetry.js

const TELEMETRY_KEY = 'telemetryEvents';
const MAX_QUEUE_SIZE = 50;

export async function reportTelemetry(event) {
    const settings = await chrome.storage.local.get('telemetryEnabled');
    if (!settings.telemetryEnabled) return; // User opted out

    const queue = await getTelemetryQueue();
    queue.push(event);

    // Flush if queue is full
    if (queue.length >= MAX_QUEUE_SIZE) {
        await flushTelemetry(queue);
    } else {
        await chrome.storage.local.set({ [TELEMETRY_KEY]: queue });
    }
}

async function getTelemetryQueue() {
    const result = await chrome.storage.local.get(TELEMETRY_KEY);
    return result[TELEMETRY_KEY] || [];
}

async function flushTelemetry(events) {
    const settings = await chrome.storage.local.get(['apiToken', 'serverUrl']);
    if (!settings.apiToken) return;

    try {
        await fetch(`${settings.serverUrl}/api/v1/telemetry/extension`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${settings.apiToken}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ events })
        });

        // Clear queue on success
        await chrome.storage.local.set({ [TELEMETRY_KEY]: [] });
    } catch (error) {
        console.warn('Failed to send telemetry:', error);
        // Keep events in queue for next flush
    }
}

// Flush telemetry periodically (every hour)
setInterval(async () => {
    const queue = await getTelemetryQueue();
    if (queue.length > 0) {
        await flushTelemetry(queue);
    }
}, 60 * 60 * 1000);
```

---

## Backend Implementation

### API Token Service

```csharp
public class ApiTokenService : IApiTokenService
{
    private readonly IApiTokenRepository apiTokenRepository;
    private readonly ISubjectPrincipal subjectPrincipal;
    private readonly IUnitOfWork unitOfWork;

    public async Task<ApiTokenCreatedDto> CreateTokenAsync(string name, int? expiresInDays)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var (token, hash) = GenerateToken();

        var apiToken = new ApiToken
        {
            ApiTokenResourceId = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
            SubjectId = subjectId,
            Name = name,
            TokenHash = hash,
            TokenPrefix = token[..10],
            CreatedDate = DateTime.UtcNow,
            ExpiresDate = expiresInDays.HasValue
                ? DateTime.UtcNow.AddDays(expiresInDays.Value)
                : null
        };

        await apiTokenRepository.AddAsync(apiToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return new ApiTokenCreatedDto
        {
            ApiTokenResourceId = apiToken.ApiTokenResourceId,
            Name = name,
            Token = token,  // Only time the full token is returned
            TokenPrefix = apiToken.TokenPrefix,
            ExpiresDate = apiToken.ExpiresDate,
            CreatedDate = apiToken.CreatedDate
        };
    }

    public async Task<List<ApiTokenDto>> ListTokensAsync()
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var tokens = await apiTokenRepository
            .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);

        return tokens.Select(t => new ApiTokenDto
        {
            ApiTokenResourceId = t.ApiTokenResourceId,
            Name = t.Name,
            TokenPrefix = t.TokenPrefix,
            CreatedDate = t.CreatedDate,
            LastUsedDate = t.LastUsedDate,
            ExpiresDate = t.ExpiresDate,
            IsRevoked = t.IsRevoked
        }).ToList();
    }

    public async Task RevokeTokenAsync(Guid apiTokenResourceId)
    {
        var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
        var token = await apiTokenRepository
            .GetAsync(apiTokenResourceId).ConfigureAwait(false);

        if (token == null || token.SubjectId != subjectId)
            throw new ValidationListException("Token not found");

        token.Revoke();
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }

    private static (string token, string hash) GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var randomPart = Convert.ToBase64String(randomBytes)
            .Replace("+", "").Replace("/", "").Replace("=", "")[..40];
        var token = $"rv_{randomPart}";
        var hash = ComputeSha256(token);
        return (token, hash);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

### API Token Authentication Handler

```csharp
public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiTokenRepository apiTokenRepository;
    private readonly IUnitOfWork unitOfWork;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer rv_"))
            return AuthenticateResult.NoResult(); // Fall through to JWT handler

        var token = headerValue["Bearer ".Length..];
        var hash = ComputeSha256(token);
        var apiToken = await apiTokenRepository.GetByHashAsync(hash)
            .ConfigureAwait(false);

        if (apiToken == null || !apiToken.IsValid())
            return AuthenticateResult.Fail("Invalid or expired API token");

        apiToken.MarkUsed();
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, apiToken.SubjectId.ToString()),
            new Claim("sub", apiToken.SubjectId.ToString()),
            new Claim("token_id", apiToken.ApiTokenResourceId.ToString()),
            new Claim("auth_method", "api_token")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

### Import Structured Endpoint

```csharp
// In ImportController.cs

[HttpPost("structured")]
[ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ImportStructuredAsync(
    [FromBody] ImportStructuredRequestModel model)
{
    using (LogContext.PushProperty("Source", model.Source))
    {
        var dto = importModelMapper.MapToDto(model);
        var recipe = await facade.ImportStructuredAsync(dto).ConfigureAwait(false);
        return Ok(recipeMapper.MapToModel(recipe));
    }
}

[HttpPost("html")]
[ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ImportHtmlAsync(
    [FromBody] ImportHtmlRequestModel model)
{
    using (LogContext.PushProperty("Source", model.Source))
    {
        var dto = importModelMapper.MapToDto(model);
        var recipe = await facade.ImportHtmlAsync(dto).ConfigureAwait(false);
        return Ok(recipeMapper.MapToModel(recipe));
    }
}
```

### Check Duplicate Endpoint

```csharp
// In RecipeController.cs

[HttpGet("check-source")]
[ProducesResponseType(typeof(CheckSourceResponseModel), StatusCodes.Status200OK)]
public async Task<IActionResult> CheckSourceAsync([FromQuery] string url)
{
    var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
    var existing = await recipeRepository
        .GetBySourceAsync(subjectId, url)
        .ConfigureAwait(false);

    if (existing == null)
    {
        return Ok(new CheckSourceResponseModel { Exists = false });
    }

    return Ok(new CheckSourceResponseModel
    {
        Exists = true,
        RecipeResourceId = existing.RecipeResourceId,
        Title = existing.Title,
        CreatedDate = existing.CreatedDate
    });
}
```

### Import HTML Service Method

```csharp
public async Task<RecipeDto> ImportHtmlAsync(ImportHtmlRequestDto dto)
{
    // Use the same heuristic extraction logic as the client, but server-side
    var extractor = new HeuristicRecipeExtractor();
    var extracted = extractor.ExtractFromHtml(dto.Html, dto.Source);

    if (extracted == null)
    {
        throw new ValidationListException("Could not extract recipe data from HTML");
    }

    // Use the same structured import flow
    return await ImportStructuredAsync(new ImportStructuredRequestDto
    {
        Title = extracted.Title,
        Description = extracted.Description,
        Yield = extracted.Yield,
        PrepTimeMinutes = extracted.PrepTimeMinutes,
        CookTimeMinutes = extracted.CookTimeMinutes,
        Source = dto.Source,
        OriginalImageUrl = extracted.OriginalImageUrl,
        RawIngredients = extracted.RawIngredients,
        RawInstructions = extracted.RawInstructions,
        Categories = extracted.Categories
    }).ConfigureAwait(false);
}
```

### Heuristic Extractor (Server-Side)

```csharp
public class HeuristicRecipeExtractor
{
    public ExtractedRecipe ExtractFromHtml(string html, string sourceUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Try JSON-LD first
        var jsonLd = ExtractJsonLd(doc);
        if (jsonLd != null) return jsonLd;

        // Try Microdata
        var microdata = ExtractMicrodata(doc);
        if (microdata != null) return microdata;

        // Fallback to heuristic scoring (same logic as client)
        var ingredients = FindIngredientList(doc);
        var instructions = FindInstructionList(doc);

        if (!ingredients.Any() && !instructions.Any())
            return null;

        return new ExtractedRecipe
        {
            Title = FindTitle(doc),
            Description = FindDescription(doc),
            Yield = 4,
            PrepTimeMinutes = null,
            CookTimeMinutes = null,
            OriginalImageUrl = FindImage(doc, sourceUrl),
            RawIngredients = ingredients,
            RawInstructions = instructions,
            Categories = new List<string>(),
            Source = sourceUrl
        };
    }

    private List<string> FindIngredientList(HtmlDocument doc)
    {
        // Same heuristic scoring logic as JavaScript version
        // Find all <ul> and <ol> elements, score them, return best match
        // (Implementation details match client-side logic)
    }

    private List<string> FindInstructionList(HtmlDocument doc)
    {
        // Same heuristic scoring logic as JavaScript version
        // (Implementation details match client-side logic)
    }

    // Additional helper methods...
}
```

### Telemetry Endpoint

```csharp
// In TelemetryController.cs

[HttpPost("extension")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> SubmitExtensionTelemetryAsync(
    [FromBody] ExtensionTelemetryRequestModel model)
{
    foreach (var evt in model.Events)
    {
        var telemetry = new ExtensionTelemetry
        {
            EventType = evt.EventType,
            Method = evt.Method,
            Url = evt.Url,
            ErrorMessage = evt.ErrorMessage,
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(evt.Timestamp).UtcDateTime,
            UserAgent = evt.UserAgent
        };

        await telemetryRepository.AddAsync(telemetry).ConfigureAwait(false);
    }

    await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    return NoContent();
}
```

---

## CORS Configuration

The backend must allow requests from the extension's `chrome-extension://` origin. Browser extensions use a unique origin per installation, so we use a wildcard pattern.

### .NET Middleware Configuration

```csharp
// In Program.cs or Startup.cs

builder.Services.AddCors(options =>
{
    options.AddPolicy("ExtensionPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                // Allow chrome-extension://, moz-extension://, and ms-browser-extension://
                return origin.StartsWith("chrome-extension://") ||
                       origin.StartsWith("moz-extension://") ||
                       origin.StartsWith("ms-browser-extension://");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for auth headers
    });
});

// Apply CORS policy
app.UseCors("ExtensionPolicy");
```

**Where to add this:**
- Add `AddCors` call BEFORE `builder.Build()`
- Add `UseCors` call AFTER `app.UseRouting()` and BEFORE `app.UseAuthorization()`

**Required CORS headers:**
- `Access-Control-Allow-Origin: chrome-extension://<unique-id>` (set dynamically based on request origin)
- `Access-Control-Allow-Credentials: true`
- `Access-Control-Allow-Headers: Authorization, Content-Type`
- `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`

**Preflight requests:**
- Browser sends `OPTIONS` request before actual request
- Middleware automatically handles preflight when using `AddCors` policy

---

## Browser Compatibility

### Chrome (Primary)

- Manifest V3 (required since Chrome 127)
- Service worker for background tasks
- `chrome.storage.local` for token and queue persistence
- `chrome.action` for badge and popup
- `chrome.scripting.executeScript` for dynamic injection

### Firefox

Port changes:
- Use `browser.*` namespace (or polyfill with `webextension-polyfill`)
- `manifest.json`: Add `"browser_specific_settings": { "gecko": { "id": "recipevault@recipevault.app" } }`
- Service workers supported in Firefox 121+
- CORS: Uses `moz-extension://` origin instead of `chrome-extension://`
- Submit to Firefox Add-ons (addons.mozilla.org)

### Edge

- Runs Chrome extensions natively (Chromium-based)
- Same manifest, submit to Microsoft Edge Add-ons
- CORS: May use `ms-browser-extension://` origin (verify with testing)
- No code changes needed

---

## Testing

### Backend Unit Tests

**ApiTokenService:**
- `CreateTokenAsync` generates token with `rv_` prefix
- Stores only SHA-256 hash (never plaintext)
- Returns full token only at creation time
- Sets correct expiry date
- Creates unique ResourceId

**ApiTokenAuthenticationHandler:**
- Valid `rv_` token resolves correct SubjectId and sets claims
- Expired token returns `AuthenticateResult.Fail`
- Revoked token returns `AuthenticateResult.Fail`
- Non-`rv_` Bearer tokens fall through to JWT handler (`AuthenticateResult.NoResult`)
- Updates LastUsedDate on successful auth

**ImportStructuredAsync:**
- Creates recipe from raw ingredients and instructions
- Parses ingredient strings into quantity/unit/item via existing parser
- Sets source URL
- Handles missing optional fields (no description, no image, no categories)
- Assigns tags from categories when provided

**ImportHtmlAsync:**
- Extracts recipe from raw HTML using heuristic scoring
- Falls back gracefully when no recipe data found
- Matches client-side extraction behavior

**CheckSourceAsync:**
- Returns `exists: true` with recipe details when URL matches existing recipe
- Returns `exists: false` when URL not found
- Only checks recipes for current user (scoped by SubjectId)

### Extension Unit Tests (Jest)

**extractJsonLd:**
- Extracts from standard Schema.org Recipe JSON-LD
- Handles nested `@graph` structures
- Handles arrays of `@type` (e.g., `["Recipe", "WebPage"]`)
- Skips non-Recipe `@type` entries
- Parses ISO 8601 durations correctly (PT1H30M = 90 minutes, PT45M = 45, PT2H = 120)
- Handles arrays in `recipeYield` and `image`
- Gracefully handles malformed JSON (try/catch, falls through)

**extractMicrodata:**
- Extracts from `itemtype="schema.org/Recipe"` elements
- Reads `content` attributes for duration properties
- Reads `src` for image elements

**extractViaHeuristics:**
- Tries known selectors as fast-path shortcuts
- Falls back to heuristic scoring when no shortcuts match
- Scores ingredient lists correctly (keyword matching, quantity detection)
- Scores instruction lists correctly (verb detection, length-based scoring)
- Calculates confidence based on total score
- Returns null when no recipe-like structure found

**queueSave:**
- Adds item to offline queue
- Assigns unique ID
- Persists to `chrome.storage.local`

**retryQueue:**
- Retries all queued items
- Removes successful saves from queue
- Increments retry count on failure
- Stops retrying after 5 attempts

**reportTelemetry:**
- Does not send when user opts out
- Batches events (max 50 before flush)
- Flushes to API endpoint
- Clears queue on success

### Integration Tests

**POST `/api/v1/import/structured`:**
- With valid Bearer token creates recipe with parsed ingredients
- Without auth returns 401

**POST `/api/v1/import/html`:**
- With valid HTML and source URL creates recipe
- Extraction matches client-side behavior
- Returns 400 when no recipe data found

**GET `/api/v1/recipes/check-source`:**
- Returns existing recipe when URL matches
- Returns `exists: false` when URL not found
- Scoped to current user only

**API Token Flow:**
- POST `/api/v1/user/api-tokens` creates token
- Subsequent request with that token authenticates and creates recipe
- DELETE `/api/v1/user/api-tokens/{id}` revokes token
- Subsequent request returns 401
- Token expiry enforced — request with expired token returns 401
- JWT auth still works alongside API token auth (both handlers registered)

**CORS:**
- Preflight `OPTIONS` request from `chrome-extension://` origin succeeds
- Actual `POST` request includes correct `Access-Control-Allow-Origin` header
- Same for `moz-extension://` (Firefox)

### Manual Testing Checklist

**Chrome:**
- Install extension, navigate to AllRecipes — verify JSON-LD extraction and badge
- Test on Serious Eats / NYT Cooking — verify JSON-LD with `@graph` structure
- Test on WordPress Recipe Maker (WPRM) sites — verify both fast-path and heuristic extraction
- Test on a blog with Microdata markup — verify Microdata extraction
- Test on a non-recipe page (e.g., news article) — verify "No recipe detected" state
- Edit title, remove an ingredient, change prep time — verify edits persist through save
- Test duplicate detection — save a recipe, revisit the same URL, verify warning appears
- Test offline queue — disconnect network, save recipe, verify queued state, reconnect, verify auto-retry
- Test server-side fallback — click "Try Server-Side Import" on a page with no client-side extraction
- Test token expiry — create token with 0 days expiry, verify extension shows login prompt

**Firefox:**
- Install extension with polyfill — verify same functionality as Chrome
- Verify CORS works with `moz-extension://` origin

**Edge:**
- Load extension — verify same functionality (no changes needed)

**Badge Performance:**
- Switch between tabs rapidly — verify badge updates without lag
- Verify badge does not update on pages you're not viewing (no global `tabs.onUpdated`)

**Telemetry:**
- Opt in to telemetry, trigger extraction failure — verify event queued
- Wait for flush (or trigger manually) — verify endpoint receives events

---

## Security

**Minimal permissions:**
- `activeTab` — Access current tab only when user clicks extension icon
- `storage` — Persist token and offline queue
- `scripting` — Inject extractor on-demand
- NO `<all_urls>` host permission — extension never runs passively

**Token storage:**
- API token stored in `chrome.storage.local`, encrypted at rest on user's device
- Isolated per extension (other extensions cannot access)

**Token hashing:**
- Server never stores plaintext tokens — only SHA-256 hashes
- Token shown once at creation time, never retrievable

**Token scoping:**
- Tokens tied to single user's SubjectId
- Compromised token only affects that user's account
- User can revoke tokens at any time

**No remote code:**
- All JavaScript bundled with extension
- No external script loading
- Content Security Policy prevents inline scripts and eval

**Input sanitization:**
- All extracted text sent as JSON strings to API
- Server handles its own validation and sanitization

**Rate limiting:**
- Extension requests go through existing upload-tier rate limiter
- Shows friendly error on 429

**CORS restrictions:**
- Only browser extension origins allowed (`chrome-extension://`, `moz-extension://`)
- No public web origins (prevents CSRF from malicious websites)

**Telemetry privacy:**
- Opt-in only (disabled by default)
- Sends domain only, no path/query (e.g., `example.com` not `example.com/secret-recipe`)
- No user-identifying data (no SubjectId, no email)
- Aggregated server-side for analytics

---

## Edge Cases

1. **Multiple recipes on one page** — JSON-LD may contain multiple Recipe objects (e.g., recipe roundup posts). The extension takes the first Recipe found. Future enhancement: let user pick.

2. **Paywalled content** — Content is extracted from the page the user sees, so paywalled recipes work as long as the user is logged in to that site. Server-side HTML import preserves this (sends HTML, not URL).

3. **Single Page Applications** — SPAs that load recipe data via JavaScript after initial page load. The extractor is injected when the user clicks the icon, catching most cases. For very slow-loading content, user can wait and re-click.

4. **Malformed JSON-LD** — Some sites have invalid JSON in their `<script type="application/ld+json">` tags. The parser uses try/catch and falls back to Microdata or heuristics.

5. **Very long recipes** — Recipes with 50+ ingredients or 30+ steps. No truncation — send everything to the API. The API already handles large recipes. Popup may scroll.

6. **Token expiration** — Popup checks token validity on open. If expired, shows login view with message. User generates new token from RecipeVault settings.

7. **Offline/server down** — Failed saves queue locally. User sees "Queued for later" message. Auto-retries when connectivity returns. Badge shows queue count.

8. **Duplicate recipe import** — User is warned before saving duplicate. Options: View Existing / Save Another Copy / Cancel. Allows duplicates if user wants them.

9. **Cross-origin images** — Some recipe sites serve images from CDNs with restrictive CORS. Extension sends image URL to API, which downloads server-side (existing behavior).

10. **Relative image URLs** — Resolved against `window.location.origin` before sending to API.

11. **Service worker termination** — Manifest V3 service workers can be terminated by browser at any time. Badge state is ephemeral (recalculated on tab switch). Offline queue and settings persist in `chrome.storage.local`.

12. **User edits introduce invalid data** — Save button validates title is non-empty. Server-side validation catches other issues (e.g., negative prep time). Shows friendly error.

13. **Heuristic extraction false positives** — Confidence scoring helps user understand extraction quality. Low-confidence extractions are editable before save.

14. **Server-side HTML import timeout** — Large HTML documents may timeout. Server should enforce max HTML size (e.g., 5 MB). Returns 400 with clear error message.

15. **Telemetry opt-out after events queued** — Flush respects current opt-in status. Events are discarded if user opts out before flush.

---

## Future Enhancements

**Phase 1 (MVP — Covered in this doc):**
- ✅ On-demand extraction with edit-before-save
- ✅ Heuristic scoring for ingredient/instruction detection
- ✅ Duplicate detection before save
- ✅ Offline queue with auto-retry
- ✅ Error telemetry (opt-in)
- ✅ Badge performance optimization

**Phase 2:**
- **Collection picker** — Choose which collection to save recipe to
- **Tags editor** — Add/remove tags before saving
- **Image preview** — Show recipe image in popup (not just URL)
- **Keyboard shortcuts** — Save recipe with `Ctrl+Shift+S` without opening popup

**Phase 3:**
- **Right-click context menu** — "Save recipe to RecipeVault" option
- **Recipe detection notification** — Automatically detect recipes and show subtle notification bar
- **Multiple recipe picker** — When a page has multiple recipes, show list and let user choose
- **Bulk import** — Save all recipes from a recipe index/search results page

**Phase 4:**
- **Safari extension** — Port using Safari Web Extensions (WebKit-based, similar to Chrome MV3)
- **Mobile browsers** — Investigate support for Firefox for Android

**Analytics & Improvements:**
- **Extraction success dashboard** — Show users which sites work best, which need improvement
- **User-submitted corrections** — Let users flag bad extractions, crowd-source selector improvements
- **A/B testing extraction methods** — Compare heuristic scoring vs ML-based extraction

---

**End of Document**
