import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { RecipeLinkService } from '../../../services/recipe-link.service';
import { Recipe } from '../../../models/recipe.model';
import { LinkedRecipe } from '../../../models/recipe-link.model';

export interface RecipeLinkDialogData {
  parentRecipeId: string;
  existingLink?: LinkedRecipe;
}

@Component({
  selector: 'app-recipe-link-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './recipe-link-dialog.component.html',
  styleUrl: './recipe-link-dialog.component.scss'
})
export class RecipeLinkDialogComponent implements OnInit {
  form: FormGroup;
  loading = false;
  error = '';
  searchResults$: Observable<Recipe[]> = of([]);
  selectedRecipe: Recipe | null = null;

  constructor(
    private fb: FormBuilder,
    private recipeLinkService: RecipeLinkService,
    public dialogRef: MatDialogRef<RecipeLinkDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RecipeLinkDialogData
  ) {
    this.form = this.fb.group({
      recipeSearch: [''],
      linkedRecipeResourceId: ['', Validators.required],
      ingredientIndex: [null],
      displayText: ['', Validators.maxLength(200)],
      includeInTotalTime: [true],
      portionUsed: [null, [Validators.min(0.01), Validators.max(100)]]
    });
  }

  ngOnInit() {
    // Set up recipe search autocomplete
    this.searchResults$ = this.form.get('recipeSearch')!.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (typeof query === 'string' && query.trim().length > 0) {
          return this.recipeLinkService.searchLinkableRecipes(query);
        }
        return of([]);
      })
    );

    // If editing existing link
    if (this.data.existingLink) {
      this.form.patchValue({
        linkedRecipeResourceId: this.data.existingLink.recipeResourceId,
        ingredientIndex: this.data.existingLink.ingredientIndex,
        displayText: this.data.existingLink.displayText || '',
        includeInTotalTime: this.data.existingLink.includeInTotalTime,
        portionUsed: this.data.existingLink.portionUsed
      });
      this.selectedRecipe = {
        recipeResourceId: this.data.existingLink.recipeResourceId,
        title: this.data.existingLink.title
      } as Recipe;
    }
  }

  onRecipeSelected(recipe: Recipe) {
    this.selectedRecipe = recipe;
    this.form.patchValue({
      linkedRecipeResourceId: recipe.recipeResourceId,
      displayText: recipe.title
    });
  }

  displayRecipeName(recipe: Recipe): string {
    return recipe?.title || '';
  }

  onSubmit() {
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    const formValue = this.form.value;
    const request = {
      linkedRecipeResourceId: formValue.linkedRecipeResourceId,
      ingredientIndex: formValue.ingredientIndex,
      displayText: formValue.displayText,
      includeInTotalTime: formValue.includeInTotalTime,
      portionUsed: formValue.portionUsed
    };

    const operation = this.data.existingLink
      ? this.recipeLinkService.updateRecipeLink(
          this.data.parentRecipeId,
          this.data.existingLink.recipeLinkResourceId,
          request
        )
      : this.recipeLinkService.createRecipeLink(this.data.parentRecipeId, request);

    operation.subscribe({
      next: (result) => {
        this.dialogRef.close(result);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'An error occurred while saving the recipe link';
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
