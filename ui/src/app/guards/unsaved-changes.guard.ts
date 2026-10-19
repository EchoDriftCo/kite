import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { RecipeFormComponent } from '../components/recipes/recipe-form/recipe-form.component';
import { ConfirmService } from '../services/confirm.service';

export const unsavedChangesGuard: CanDeactivateFn<RecipeFormComponent> = (component) => {
  if (component.hasUnsavedChanges) {
    return inject(ConfirmService).ask({
      title: 'Leave without saving?',
      message: "Your changes to this recipe haven't been saved.",
      confirmText: 'Leave',
      cancelText: 'Keep editing',
    });
  }
  return true;
};
