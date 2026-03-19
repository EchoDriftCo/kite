# Recipe Linking Integration Guide

## Frontend Components Created

### 1. Service: `recipe-link.service.ts`
Location: `src/app/services/recipe-link.service.ts`

Provides API integration for recipe linking:
- `createRecipeLink()` - Link a recipe to another
- `updateRecipeLink()` - Update link details
- `deleteRecipeLink()` - Remove a link
- `getLinkedRecipes()` - Get component recipes
- `getUsedInRecipes()` - Get reverse lookup (recipes using this one)
- `searchLinkableRecipes()` - Search user's recipes for linking

### 2. Models: `recipe-link.model.ts`
Location: `src/app/models/recipe-link.model.ts`

TypeScript interfaces:
- `RecipeLink`
- `LinkedRecipe`
- `UsedInRecipe`
- `CreateRecipeLinkRequest`
- `UpdateRecipeLinkRequest`

### 3. Components

#### Recipe Link Dialog
Location: `src/app/shared/components/recipe-link-dialog/`

A dialog component for adding/editing recipe links with:
- Recipe search with autocomplete
- Display text customization
- Ingredient position (index)
- Portion multiplier
- Include in total time toggle

#### Recipe Links Display
Location: `src/app/shared/components/recipe-links/`

A display component showing:
- Linked recipes (components) with thumbnails
- "Used in" section (reverse lookup)
- Edit/delete actions (when user owns recipe)
- Add link button

## Integration into Recipe Detail View

To integrate into your recipe detail component:

### 1. Import the components:

```typescript
import { RecipeLinksComponent } from '../../shared/components/recipe-links/recipe-links.component';
import { RecipeLinkService } from '../../services/recipe-link.service';
import { LinkedRecipe, UsedInRecipe } from '../../models/recipe-link.model';
```

### 2. Add to component imports array:

```typescript
@Component({
  imports: [
    // ... other imports
    RecipeLinksComponent
  ]
})
```

### 3. Add component properties:

```typescript
linkedRecipes: LinkedRecipe[] = [];
usedInRecipes: UsedInRecipe[] = [];
```

### 4. Load links when recipe loads:

```typescript
loadRecipe(id: string) {
  // ... existing recipe loading code

  // Load linked recipes
  this.recipeLinkService.getLinkedRecipes(id).subscribe({
    next: (links) => {
      this.linkedRecipes = links;
    },
    error: (err) => {
      console.error('Error loading linked recipes:', err);
    }
  });

  // Load "used in" recipes
  this.recipeLinkService.getUsedInRecipes(id).subscribe({
    next: (usage) => {
      this.usedInRecipes = usage;
    },
    error: (err) => {
      console.error('Error loading used-in recipes:', err);
    }
  });
}
```

### 5. Add to template:

```html
<!-- After recipe instructions or wherever appropriate -->
<app-recipe-links
  [recipeId]="recipe.recipeResourceId"
  [linkedRecipes]="linkedRecipes"
  [usedInRecipes]="usedInRecipes"
  [canEdit]="recipe.isOwner"
  (refreshRequested)="loadRecipe(recipe.recipeResourceId)"
></app-recipe-links>
```

### 6. Handle link icon on ingredients (optional):

In the ingredients list, add a link icon for linked ingredients:

```html
<div *ngFor="let ingredient of recipe.ingredients; let i = index" class="ingredient-item">
  <mat-icon
    *ngIf="isIngredientLinked(i)"
    class="link-icon"
    matTooltip="This ingredient links to a component recipe"
  >
    link
  </mat-icon>
  {{ ingredient.rawText }}
</div>
```

```typescript
isIngredientLinked(index: number): boolean {
  return this.linkedRecipes.some(link => link.ingredientIndex === index);
}
```

## CSS Variables for Dark Mode

The components use CSS variables for theming:

```scss
// Light mode (default)
--primary: #1976d2;
--surface: #fff;
--surface-variant: #f5f5f5;
--border: #e0e0e0;
--text-secondary: #757575;
--error: #d32f2f;
--error-container: #ffebee;

// Dark mode (override in dark theme)
--primary: #90caf9;
--surface: #121212;
--surface-variant: #1e1e1e;
--border: #333;
--text-secondary: #aaa;
--error: #cf6679;
--error-container: #5f2120;
```

## API Endpoints

The service expects these endpoints (already implemented in backend):

- `POST /api/v1/recipes/{id}/links`
- `GET /api/v1/recipes/{id}/links`
- `DELETE /api/v1/recipes/{id}/links/{linkId}`
- `PUT /api/v1/recipes/{id}/links/{linkId}`
- `GET /api/v1/recipes/{id}/used-in`
- `GET /api/v1/recipes/linkable?query={search}`

## Notes

- All components are **standalone** (Angular 17+ style)
- Material Design components used for consistency
- Responsive design with CSS variables for theming
- Error handling included
- Loading states implemented
- Accessibility features (tooltips, labels, ARIA)

## Future Enhancements

Consider adding:
1. Bulk linking from ingredient list
2. Visual graph of recipe dependencies
3. Nutritional data aggregation from linked recipes
4. Time calculation with component recipe times
5. Ingredient quantity adjustment based on `portionUsed`
