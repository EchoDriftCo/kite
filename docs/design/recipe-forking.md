# Recipe Forking

## Overview

Allow users to create personal copies ("forks") of recipes — their own or others' public recipes. Forks maintain a reference to the original while allowing full customization.

## Use Cases

1. **Customize a shared recipe** — User finds a public recipe, wants to adjust ingredients/instructions for their preferences
2. **Save substitutions** — Substitution feature creates a fork with modified ingredients
3. **Iterate on your own recipes** — Create variations (e.g., "Mom's Pie - Gluten Free Version")
4. **Preserve original** — Make changes without losing the original recipe

---

## Data Model

### Recipe Entity Changes

```csharp
public class Recipe {
    // Existing fields...
    
    // Forking fields
    public int? ForkedFromRecipeId { get; private set; }
    public virtual Recipe ForkedFromRecipe { get; private set; }
    public virtual ICollection<Recipe> Forks { get; private set; }
    
    // Methods
    public Recipe Fork(string newTitle = null);
    public void ClearForkReference(); // If original is deleted
}
```

### Fork Behavior

| Scenario | Behavior |
|----------|----------|
| Fork own recipe | Creates copy, links to original |
| Fork public recipe | Creates copy in your library, links to original |
| Fork a fork | Links to the immediate parent (not root) |
| Original deleted | Fork keeps `ForkedFromRecipeId` but `ForkedFromRecipe` is null (orphaned fork) |
| Original made private | Fork still works, but "View Original" shows "no longer available" |

---

## API Design

### Fork a Recipe

```
POST /api/recipes/{recipeResourceId}/fork
{
    "title": "Mom's Pie - My Version"  // Optional, defaults to "{Original Title} (Copy)"
}
```

Response: Full `RecipeDto` of the new fork

### Get Fork Info

Included in standard recipe response:

```json
{
    "recipeResourceId": "...",
    "title": "Mom's Pie - My Version",
    "forkedFrom": {
        "recipeResourceId": "...",
        "title": "Mom's Pie",
        "ownerName": "Jane Doe",
        "isAvailable": true
    },
    "forkCount": 3  // Only on recipes you own
}
```

### List Forks of a Recipe

```
GET /api/recipes/{recipeResourceId}/forks
```

Returns paginated list of public forks (for discoverability).

---

## UX Design

### Fork Button

On recipe view, show fork button:

```
┌─────────────────────────────────────────────┐
│  Mom's Apple Pie                            │
│  by Jane Doe                                │
│                                             │
│  [♡ Save] [🍴 Fork] [📤 Share]              │
└─────────────────────────────────────────────┘
```

### Fork Indicator

On forked recipes, show lineage:

```
┌─────────────────────────────────────────────┐
│  Mom's Apple Pie - Gluten Free              │
│  ↳ Forked from "Mom's Apple Pie" by Jane    │
│                                             │
│  [Edit] [📤 Share]                          │
└─────────────────────────────────────────────┘
```

If original unavailable:

```
│  ↳ Forked from a recipe that's no longer available
```

### Fork Count (Optional)

On popular recipes, show fork count:

```
│  🍴 47 forks
```

---

## Implementation

### Database Migration

```sql
ALTER TABLE "Recipe" ADD COLUMN "ForkedFromRecipeId" INTEGER NULL;

ALTER TABLE "Recipe" 
ADD CONSTRAINT "FK_Recipe_ForkedFromRecipe" 
FOREIGN KEY ("ForkedFromRecipeId") 
REFERENCES "Recipe"("RecipeId") 
ON DELETE SET NULL;

CREATE INDEX "IX_Recipe_ForkedFromRecipeId" ON "Recipe"("ForkedFromRecipeId");
```

### Recipe.Fork() Method

```csharp
public Recipe Fork(string newTitle = null)
{
    var fork = new Recipe(
        title: newTitle ?? $"{Title} (Copy)",
        yield: Yield,
        prepTimeMinutes: PrepTimeMinutes,
        cookTimeMinutes: CookTimeMinutes,
        description: Description,
        source: null,  // Clear source - this is now user's recipe
        originalImageUrl: OriginalImageUrl,
        isPublic: false  // Forks start private
    );
    
    // Copy ingredients
    fork.SetIngredients(Ingredients.Select(i => new RecipeIngredient(
        i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText
    )).ToList());
    
    // Copy instructions
    fork.SetInstructions(Instructions.Select(i => new RecipeInstruction(
        i.StepNumber, i.Instruction, i.RawText
    )).ToList());
    
    // Copy tags (reference same tags, don't copy)
    foreach (var rt in RecipeTags.Where(rt => !rt.IsOverridden))
    {
        fork.AddTag(new RecipeTag(
            fork.RecipeId, 
            rt.TagId, 
            Guid.Empty,  // Will be set by service
            isAiAssigned: false, 
            confidence: null
        ));
    }
    
    // Link to original
    fork.ForkedFromRecipeId = RecipeId;
    
    // Copy image URL if exists
    if (!string.IsNullOrEmpty(SourceImageUrl))
    {
        fork.SetSourceImageUrl(SourceImageUrl);
    }
    
    return fork;
}
```

### RecipeService.ForkRecipeAsync()

```csharp
public async Task<Recipe> ForkRecipeAsync(Guid recipeResourceId, string newTitle = null)
{
    // Get original (respects visibility - own or public)
    var original = await GetRecipeAsync(recipeResourceId);
    
    var fork = original.Fork(newTitle);
    
    // Fix up tag assignments with current user
    var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
    foreach (var rt in fork.RecipeTags.ToList())
    {
        fork.RemoveTag(rt);
        fork.AddTag(new RecipeTag(
            fork.RecipeId,
            rt.TagId,
            currentSubjectId,
            isAiAssigned: false,
            confidence: null
        ));
    }
    
    await recipeRepository.AddAsync(fork);
    
    logger.LogInformation("Forked recipe {OriginalId} to {ForkId}", 
        original.RecipeResourceId, fork.RecipeResourceId);
    
    return fork;
}
```

---

## Files to Modify

### Backend
- `Recipe.cs` — Add ForkedFromRecipeId, Fork() method
- `RecipeDto.cs` — Add ForkedFrom info
- `RecipeService.cs` — Add ForkRecipeAsync
- `RecipeFacade.cs` — Add ForkRecipeAsync
- `RecipesController.cs` — Add POST fork endpoint
- `RecipeMapper.cs` — Map fork info
- Migration for new column

### Frontend
- Recipe view — Fork button
- Recipe card — Fork indicator
- Fork dialog (optional) — Customize title before forking

---

## Edge Cases

1. **Forking a fork** — Links to immediate parent, not root. Could add `RootRecipeId` later for full lineage.

2. **Circular fork detection** — Not possible since you can't fork your own fork back to itself (creates new recipe each time).

3. **Original recipe deleted** — `ForkedFromRecipeId` preserved, `ForkedFromRecipe` navigation returns null. UI shows "original no longer available".

4. **Original made private** — Fork still exists. `GetRecipeAsync` on original will throw for non-owner. UI handles gracefully.

5. **Bulk operations** — If user deletes their account, their recipes are deleted, forks become orphaned (fine).

---

## Future Enhancements

- **Diff view** — Show what changed from original
- **Merge updates** — Pull changes from original into fork
- **Fork tree** — Visualize fork lineage
- **Attribution** — "Based on X forks" on popular recipes
