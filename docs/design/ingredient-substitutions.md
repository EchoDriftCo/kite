# Ingredient Substitutions

## Overview

AI-powered ingredient substitution suggestions. Users can select specific ingredients to substitute and/or dietary constraints to apply. Gemini analyzes the full recipe context and suggests alternatives with adjusted quantities.

**Dependency:** Requires [Recipe Forking](./recipe-forking.md) to save substitutions.

---

## User Flow

### 1. Enable Substitution Mode

User toggles substitution mode on recipe view:

```
┌─────────────────────────────────────────────┐
│  Grandma's Bread                            │
│                                             │
│  [🔄 Find Substitutions]  ← toggle on       │
└─────────────────────────────────────────────┘
```

### 2. Select What to Substitute

**Requirement:** Must select at least 1 ingredient OR 1 dietary constraint.

```
┌─────────────────────────────────────────────────┐
│  What do you want to substitute?                │
│                                                 │
│  Ingredients:                                   │
│  ☑ 3 cups bread flour                          │
│  ☐ 1 cup whole milk                            │
│  ☐ 2 tbsp butter                               │
│  ☐ 1 packet active dry yeast                   │
│  ☐ 1 tsp salt                                  │
│                                                 │
│  Dietary constraints:                          │
│  [+ Add constraint]                            │
│  ┌──────────────────┐                          │
│  │ 🌿 Gluten-Free  ✕│                          │
│  └──────────────────┘                          │
│                                                 │
│  ℹ️ Select ingredients you want to replace,    │
│     or dietary tags to evaluate the full       │
│     recipe for compliance.                     │
│                                                 │
│  [Find Substitutions]                          │
└─────────────────────────────────────────────────┘
```

### 3. View Suggestions

Gemini returns 2-3 options per ingredient:

```
┌─────────────────────────────────────────────────┐
│  Substitution Suggestions                       │
├─────────────────────────────────────────────────┤
│                                                 │
│  🌾 3 cups bread flour                          │
│                                                 │
│  ○ Option 1: Gluten-Free Bread Flour Blend     │
│    • 2¾ cups gluten-free all-purpose flour     │
│    • ¼ cup tapioca starch                      │
│    • 1 tsp xanthan gum                         │
│    📝 Provides structure without gluten.       │
│       Texture will be slightly denser.         │
│                                                 │
│  ○ Option 2: Almond Flour Blend                │
│    • 2 cups almond flour                       │
│    • 1 cup tapioca flour                       │
│    • 1 tsp xanthan gum                         │
│    📝 Lower carb option. Very different        │
│       texture - more cake-like.                │
│                                                 │
│  ○ None of these                               │
│                                                 │
├─────────────────────────────────────────────────┤
│  [Cancel]                    [Apply & Save]    │
└─────────────────────────────────────────────────┘
```

### 4. Apply and Save

User selects one option per ingredient (or "none"). On "Apply & Save":

1. Creates a fork of the recipe
2. Applies selected substitutions to the fork
3. Adds note about substitutions made
4. Opens the forked recipe in edit mode

---

## Prompt Modes

### Mode A: Specific Ingredients Selected

User selected specific ingredients to substitute.

**Gemini Prompt:**
```
You are a culinary expert helping with ingredient substitutions.

Recipe: {title}
Type: {meal type tags if available}

Full ingredient list (for context):
{all ingredients with quantities}

Instructions summary:
{condensed instructions for technique context}

Please suggest substitutions for these specific ingredients:
- 3 cups bread flour
- 1 cup whole milk

Dietary constraints to adhere to:
- Gluten-Free
- Dairy-Free

For each ingredient, provide 2-3 substitution options. Each option should include:
1. Replacement ingredient(s) with exact quantities
2. Brief note on taste/texture impact
3. Any technique adjustments needed

Format as JSON:
{
  "substitutions": [
    {
      "original": "3 cups bread flour",
      "options": [
        {
          "name": "Gluten-Free Blend",
          "ingredients": [
            {"quantity": "2¾", "unit": "cups", "item": "gluten-free all-purpose flour"},
            {"quantity": "¼", "unit": "cup", "item": "tapioca starch"},
            {"quantity": "1", "unit": "tsp", "item": "xanthan gum"}
          ],
          "notes": "Provides structure without gluten. Texture slightly denser.",
          "techniqueAdjustments": "Mix dry ingredients separately first."
        }
      ]
    }
  ]
}
```

### Mode B: Dietary Constraints Only (No Specific Ingredients)

User selected dietary constraints but no specific ingredients. Gemini evaluates the entire recipe.

**Gemini Prompt:**
```
You are a culinary expert helping adapt recipes for dietary needs.

Recipe: {title}

Full ingredient list:
{all ingredients with quantities}

Instructions:
{full instructions}

Evaluate this recipe for the following dietary constraints:
- Gluten-Free

Identify ALL ingredients that need substitution to meet these constraints.
For each problematic ingredient, provide 2-3 substitution options.

Format as JSON:
{
  "analysis": "This recipe contains gluten in the bread flour and soy sauce.",
  "substitutions": [
    {
      "original": "3 cups bread flour",
      "reason": "Contains gluten",
      "options": [...]
    },
    {
      "original": "2 tbsp soy sauce", 
      "reason": "Contains wheat",
      "options": [...]
    }
  ]
}
```

---

## Data Model

### Substitution Request (Transient)

```typescript
interface SubstitutionRequest {
  recipeResourceId: string;
  selectedIngredients: number[];  // Indices of selected ingredients
  dietaryConstraints: string[];   // Tag names: "Gluten-Free", "Dairy-Free"
}
```

### Substitution Response (Cached)

```typescript
interface SubstitutionResponse {
  requestHash: string;  // Hash of request for cache lookup
  timestamp: Date;
  substitutions: IngredientSubstitution[];
}

interface IngredientSubstitution {
  originalIndex: number;
  originalText: string;
  reason?: string;  // Only in Mode B
  options: SubstitutionOption[];
}

interface SubstitutionOption {
  name: string;
  ingredients: ParsedIngredient[];
  notes: string;
  techniqueAdjustments?: string;
}
```

### No Persistence Until Save

- Substitution responses are cached in-memory (or Redis/local storage)
- Cache key: hash of (recipeId + selectedIngredients + dietaryConstraints)
- TTL: 24 hours (or until recipe is modified)
- On "Apply & Save": creates fork, no substitution data persisted separately

---

## API Design

### Get Substitution Suggestions

```
POST /api/recipes/{recipeResourceId}/substitutions
{
  "ingredientIndices": [0, 2],     // Indices of ingredients to substitute
  "dietaryConstraints": ["Gluten-Free"]
}
```

**Validation:**
- Must provide at least one of: `ingredientIndices` or `dietaryConstraints`
- `ingredientIndices` must be valid indices

**Response:**
```json
{
  "substitutions": [
    {
      "originalIndex": 0,
      "originalText": "3 cups bread flour",
      "options": [
        {
          "name": "Gluten-Free Blend",
          "ingredients": [
            {"quantity": 2.75, "unit": "cups", "item": "gluten-free all-purpose flour"},
            {"quantity": 0.25, "unit": "cup", "item": "tapioca starch"}
          ],
          "notes": "Texture slightly denser."
        }
      ]
    }
  ],
  "cached": false
}
```

### Apply Substitutions (Creates Fork)

```
POST /api/recipes/{recipeResourceId}/substitutions/apply
{
  "selections": [
    {"ingredientIndex": 0, "optionIndex": 0},
    {"ingredientIndex": 2, "optionIndex": 1}
  ],
  "forkTitle": "Grandma's Bread - Gluten Free"  // Optional
}
```

**Response:** Full `RecipeDto` of the new forked recipe with substitutions applied.

---

## Implementation

### GeminiClient Extension

```csharp
public interface IGeminiClient
{
    // Existing methods...
    
    Task<SubstitutionAnalysis> AnalyzeSubstitutionsAsync(
        Recipe recipe,
        List<int> ingredientIndices,
        List<string> dietaryConstraints);
}
```

### SubstitutionService

```csharp
public class SubstitutionService : ISubstitutionService
{
    private readonly IGeminiClient geminiClient;
    private readonly IRecipeService recipeService;
    private readonly ICacheService cacheService;
    
    public async Task<SubstitutionResponse> GetSubstitutionsAsync(
        Guid recipeResourceId,
        List<int> ingredientIndices,
        List<string> dietaryConstraints)
    {
        var recipe = await recipeService.GetRecipeAsync(recipeResourceId);
        
        // Check cache
        var cacheKey = BuildCacheKey(recipeResourceId, ingredientIndices, dietaryConstraints);
        var cached = await cacheService.GetAsync<SubstitutionResponse>(cacheKey);
        if (cached != null) return cached with { Cached = true };
        
        // Call Gemini
        var analysis = await geminiClient.AnalyzeSubstitutionsAsync(
            recipe, ingredientIndices, dietaryConstraints);
        
        var response = MapToResponse(analysis);
        
        // Cache result
        await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
        
        return response;
    }
    
    public async Task<Recipe> ApplySubstitutionsAsync(
        Guid recipeResourceId,
        List<SubstitutionSelection> selections,
        string forkTitle = null)
    {
        var recipe = await recipeService.GetRecipeAsync(recipeResourceId);
        var substitutions = await GetSubstitutionsAsync(...);
        
        // Fork the recipe
        var fork = await recipeService.ForkRecipeAsync(recipeResourceId, forkTitle);
        
        // Apply substitutions to forked ingredients
        var newIngredients = ApplySelectionsToIngredients(
            fork.Ingredients.ToList(), 
            substitutions, 
            selections);
        
        fork.SetIngredients(newIngredients);
        
        // Add note about substitutions
        var note = GenerateSubstitutionNote(selections);
        fork.Update(
            fork.Title,
            fork.Yield,
            fork.PrepTimeMinutes,
            fork.CookTimeMinutes,
            $"{fork.Description}\n\n---\n{note}",
            fork.Source,
            fork.OriginalImageUrl
        );
        
        return fork;
    }
}
```

---

## Files to Create/Modify

### Backend
- `SubstitutionService.cs` — Core logic
- `ISubstitutionService.cs` — Interface
- `SubstitutionController.cs` — API endpoints
- `GeminiClient.cs` — Add AnalyzeSubstitutionsAsync
- DTOs for request/response
- Caching integration

### Frontend
- Substitution mode toggle component
- Ingredient selector with checkboxes
- Dietary constraint picker
- Substitution results view
- Apply confirmation dialog

---

## Implementation Order

1. **Implement Recipe Forking first** (see recipe-forking.md)
2. Add Gemini substitution analysis method
3. Create SubstitutionService with caching
4. Build API endpoints
5. Build UI components
6. Integration testing

---

## Future Enhancements

- **"I have these ingredients"** — Inverse mode: substitute TO what user has
- **Substitution history** — Remember what worked for user
- **Community substitutions** — Learn from popular fork patterns
- **Nutritional impact** — Show calorie/macro changes from substitution
- **Smart defaults** — Pre-select based on user's dietary profile
