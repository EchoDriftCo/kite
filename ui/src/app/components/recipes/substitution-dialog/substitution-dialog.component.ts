import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { SubstitutionService } from '../../../services/substitution.service';
import {
  Recipe,
  RecipeIngredient
} from '../../../models/recipe.model';
import {
  DIETARY_CONSTRAINTS,
  DietaryConstraint,
  IngredientSubstitution,
  SubstitutionSelection
} from '../../../models/substitution.model';

export interface SubstitutionDialogData {
  recipe: Recipe;
}

export interface SubstitutionDialogResult {
  forkedRecipeId: string;
}

interface IngredientSelectionItem {
  index: number;
  ingredient: RecipeIngredient;
  selected: boolean;
}

interface SubstitutionWithSelection extends IngredientSubstitution {
  selectedOptionIndex: number; // -1 for "none of these"
}

@Component({
  selector: 'app-substitution-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatStepperModule,
    MatCheckboxModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatDividerModule
  ],
  templateUrl: './substitution-dialog.component.html',
  styleUrl: './substitution-dialog.component.scss'
})
export class SubstitutionDialogComponent implements OnInit {
  // Step 1: Selection
  ingredientItems: IngredientSelectionItem[] = [];
  availableConstraints = DIETARY_CONSTRAINTS;
  selectedConstraints: Set<DietaryConstraint> = new Set();

  // Step 2: Suggestions
  loading = false;
  error = '';
  substitutions: SubstitutionWithSelection[] = [];

  // Step 3: Apply
  applying = false;
  forkTitle = '';

  // Stepper control
  currentStep = 0;

  constructor(
    public dialogRef: MatDialogRef<SubstitutionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SubstitutionDialogData,
    private substitutionService: SubstitutionService
  ) {}

  ngOnInit() {
    // Initialize ingredient selection list
    this.ingredientItems = this.data.recipe.ingredients.map((ing, index) => ({
      index,
      ingredient: ing,
      selected: false
    }));

    // Auto-populate fork title
    this.forkTitle = `${this.data.recipe.title} - Modified`;
  }

  // Step 1 helpers
  toggleConstraint(constraint: DietaryConstraint) {
    if (this.selectedConstraints.has(constraint)) {
      this.selectedConstraints.delete(constraint);
    } else {
      this.selectedConstraints.add(constraint);
    }
  }

  hasConstraint(constraint: DietaryConstraint): boolean {
    return this.selectedConstraints.has(constraint);
  }

  canProceedToSuggestions(): boolean {
    const hasSelectedIngredients = this.ingredientItems.some(item => item.selected);
    const hasSelectedConstraints = this.selectedConstraints.size > 0;
    return hasSelectedIngredients || hasSelectedConstraints;
  }

  // Step 1 -> Step 2: Fetch suggestions
  fetchSuggestions() {
    if (!this.canProceedToSuggestions()) {
      this.error = 'Please select at least one ingredient or dietary constraint.';
      return;
    }

    this.loading = true;
    this.error = '';

    const selectedIndices = this.ingredientItems
      .filter(item => item.selected)
      .map(item => item.index);

    const constraints = Array.from(this.selectedConstraints);

    this.substitutionService.getSubstitutions(
      this.data.recipe.recipeResourceId,
      selectedIndices.length > 0 ? selectedIndices : undefined,
      constraints.length > 0 ? constraints : undefined
    ).subscribe({
      next: (response) => {
        this.substitutions = response.substitutions.map(sub => ({
          ...sub,
          selectedOptionIndex: -1 // Default to "none of these"
        }));
        this.loading = false;
        this.currentStep = 1; // Move to step 2
      },
      error: (err) => {
        this.error = err.error?.message || err.message || 'Failed to get substitutions';
        this.loading = false;
        console.error('Error fetching substitutions:', err);
      }
    });
  }

  // Step 2 helpers
  getIngredientText(ingredient: RecipeIngredient): string {
    const parts: string[] = [];
    
    if (ingredient.quantity != null) {
      parts.push(ingredient.quantity.toString());
    }
    
    if (ingredient.unit) {
      parts.push(ingredient.unit);
    }
    
    parts.push(ingredient.item);
    
    if (ingredient.preparation) {
      parts.push(`(${ingredient.preparation})`);
    }
    
    return parts.join(' ');
  }

  selectOption(substitution: SubstitutionWithSelection, optionIndex: number) {
    substitution.selectedOptionIndex = optionIndex;
  }

  canProceedToApply(): boolean {
    // At least one substitution must have an option selected (not -1)
    return this.substitutions.some(sub => sub.selectedOptionIndex >= 0);
  }

  // Step 2 -> Step 3: Prepare to apply
  proceedToApply() {
    if (!this.canProceedToApply()) {
      this.error = 'Please select at least one substitution option.';
      return;
    }
    this.currentStep = 2;
  }

  // Step 3: Apply and save
  applySubstitutions() {
    this.applying = true;
    this.error = '';

    const selections: SubstitutionSelection[] = this.substitutions
      .filter(sub => sub.selectedOptionIndex >= 0)
      .map(sub => ({
        ingredientIndex: sub.originalIndex,
        optionIndex: sub.selectedOptionIndex
      }));

    this.substitutionService.applySubstitutions(
      this.data.recipe.recipeResourceId,
      selections,
      this.forkTitle || undefined
    ).subscribe({
      next: (forkedRecipe) => {
        this.applying = false;
        this.dialogRef.close({
          forkedRecipeId: forkedRecipe.recipeResourceId
        } as SubstitutionDialogResult);
      },
      error: (err) => {
        this.error = err.error?.message || err.message || 'Failed to apply substitutions';
        this.applying = false;
        console.error('Error applying substitutions:', err);
      }
    });
  }

  // Navigation
  goBack() {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  cancel() {
    this.dialogRef.close();
  }

  // Utility
  getSelectedIngredientCount(): number {
    return this.ingredientItems.filter(item => item.selected).length;
  }

  getSelectedConstraintCount(): number {
    return this.selectedConstraints.size;
  }

  getAppliedSubstitutionCount(): number {
    return this.substitutions.filter(sub => sub.selectedOptionIndex >= 0).length;
  }
}
