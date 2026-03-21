import { CanDeactivateFn } from '@angular/router';
import { RecipeFormComponent } from '../components/recipes/recipe-form/recipe-form.component';

export const unsavedChangesGuard: CanDeactivateFn<RecipeFormComponent> = (component) => {
  if (component.hasUnsavedChanges) {
    return confirm('You have unsaved changes. Are you sure you want to leave?');
  }
  return true;
};
