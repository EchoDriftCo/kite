import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe, CreateRecipeRequest, RecipeIngredient, RecipeInstruction, ParsedRecipe } from '../../../models/recipe.model';
import { RecipeImportDialogComponent, ImportResult } from '../recipe-import-dialog/recipe-import-dialog.component';
import { TagSelectorComponent } from '../../shared/tag-selector/tag-selector.component';
import { RecipeTag, AssignTagItem } from '../../../models/tag.model';

@Component({
  selector: 'app-recipe-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSlideToggleModule,
    TagSelectorComponent
  ],
  templateUrl: './recipe-form.component.html',
  styleUrl: './recipe-form.component.scss'
})
export class RecipeFormComponent implements OnInit {
  recipeForm!: FormGroup;
  loading = false;
  saving = false;
  error = '';
  recipeId: string | null = null;
  isEditMode = false;
  currentRecipeTags: RecipeTag[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private dialog: MatDialog
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.recipeId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.recipeId;

    if (this.isEditMode && this.recipeId) {
      this.loadRecipe();
    } else {
      // Add one empty ingredient and instruction for new recipes
      this.addIngredient();
      this.addInstruction();
    }
  }

  initForm() {
    this.recipeForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      yield: [4, [Validators.required, Validators.min(1), Validators.max(100)]],
      prepTimeMinutes: [null, [Validators.min(0), Validators.max(1440)]],
      cookTimeMinutes: [null, [Validators.min(0), Validators.max(1440)]],
      source: ['', Validators.maxLength(500)],
      originalImageUrl: ['', Validators.maxLength(2000)],
      isPublic: [false],
      ingredients: this.fb.array([]),
      instructions: this.fb.array([])
    });
  }

  get ingredients(): FormArray {
    return this.recipeForm.get('ingredients') as FormArray;
  }

  get instructions(): FormArray {
    return this.recipeForm.get('instructions') as FormArray;
  }

  loadRecipe() {
    if (!this.recipeId) return;

    this.loading = true;
    this.error = '';

    this.recipeService.getRecipe(this.recipeId).subscribe({
      next: (recipe) => {
        this.populateForm(recipe);
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load recipe';
        this.loading = false;
        console.error('Error loading recipe:', err);
      }
    });
  }

  populateForm(recipe: Recipe) {
    this.recipeForm.patchValue({
      title: recipe.title,
      description: recipe.description,
      yield: recipe.yield,
      prepTimeMinutes: recipe.prepTimeMinutes,
      cookTimeMinutes: recipe.cookTimeMinutes,
      source: recipe.source,
      originalImageUrl: recipe.originalImageUrl,
      isPublic: recipe.isPublic || false
    });

    // Store tags
    this.currentRecipeTags = recipe.tags || [];

    // Clear and populate ingredients
    this.ingredients.clear();
    (recipe.ingredients || []).forEach(ingredient => {
      this.addIngredient(ingredient);
    });

    // Clear and populate instructions
    this.instructions.clear();
    (recipe.instructions || []).forEach(instruction => {
      this.addInstruction(instruction);
    });
  }

  createIngredientGroup(ingredient?: RecipeIngredient): FormGroup {
    return this.fb.group({
      sortOrder: [ingredient?.sortOrder || this.ingredients.length + 1],
      quantity: [ingredient?.quantity || null],
      unit: [ingredient?.unit || ''],
      item: [ingredient?.item || '', Validators.required],
      preparation: [ingredient?.preparation || '']
    });
  }

  createInstructionGroup(instruction?: RecipeInstruction): FormGroup {
    return this.fb.group({
      stepNumber: [instruction?.stepNumber || this.instructions.length + 1],
      instruction: [instruction?.instruction || '', Validators.required]
    });
  }

  addIngredient(ingredient?: RecipeIngredient) {
    this.ingredients.push(this.createIngredientGroup(ingredient));
  }

  removeIngredient(index: number) {
    this.ingredients.removeAt(index);
    // Update sort orders
    this.ingredients.controls.forEach((control, i) => {
      control.patchValue({ sortOrder: i + 1 });
    });
  }

  addInstruction(instruction?: RecipeInstruction) {
    this.instructions.push(this.createInstructionGroup(instruction));
  }

  removeInstruction(index: number) {
    this.instructions.removeAt(index);
    // Update step numbers
    this.instructions.controls.forEach((control, i) => {
      control.patchValue({ stepNumber: i + 1 });
    });
  }

  moveIngredientUp(index: number) {
    if (index === 0) return;
    const ingredient = this.ingredients.at(index);
    this.ingredients.removeAt(index);
    this.ingredients.insert(index - 1, ingredient);
    this.updateIngredientSortOrders();
  }

  moveIngredientDown(index: number) {
    if (index === this.ingredients.length - 1) return;
    const ingredient = this.ingredients.at(index);
    this.ingredients.removeAt(index);
    this.ingredients.insert(index + 1, ingredient);
    this.updateIngredientSortOrders();
  }

  moveInstructionUp(index: number) {
    if (index === 0) return;
    const instruction = this.instructions.at(index);
    this.instructions.removeAt(index);
    this.instructions.insert(index - 1, instruction);
    this.updateInstructionStepNumbers();
  }

  moveInstructionDown(index: number) {
    if (index === this.instructions.length - 1) return;
    const instruction = this.instructions.at(index);
    this.instructions.removeAt(index);
    this.instructions.insert(index + 1, instruction);
    this.updateInstructionStepNumbers();
  }

  updateIngredientSortOrders() {
    this.ingredients.controls.forEach((control, i) => {
      control.patchValue({ sortOrder: i + 1 });
    });
  }

  updateInstructionStepNumbers() {
    this.instructions.controls.forEach((control, i) => {
      control.patchValue({ stepNumber: i + 1 });
    });
  }

  onSubmit() {
    if (this.recipeForm.invalid) {
      this.recipeForm.markAllAsTouched();
      this.error = 'Please fix the errors in the form';
      return;
    }

    const formValue = this.recipeForm.value;
    const request: CreateRecipeRequest = {
      title: formValue.title,
      description: formValue.description || undefined,
      yield: formValue.yield,
      prepTimeMinutes: formValue.prepTimeMinutes || undefined,
      cookTimeMinutes: formValue.cookTimeMinutes || undefined,
      source: formValue.source || undefined,
      originalImageUrl: formValue.originalImageUrl || undefined,
      isPublic: formValue.isPublic || false,
      ingredients: formValue.ingredients,
      instructions: formValue.instructions
    };

    this.saving = true;
    this.error = '';

    const operation = this.isEditMode && this.recipeId
      ? this.recipeService.updateRecipe(this.recipeId, request)
      : this.recipeService.createRecipe(request);

    operation.subscribe({
      next: (recipe) => {
        this.saving = false;
        this.router.navigate(['/recipes', recipe.recipeResourceId]);
      },
      error: (err) => {
        this.error = err.message || 'Failed to save recipe';
        this.saving = false;
        console.error('Error saving recipe:', err);
      }
    });
  }

  cancel() {
    if (this.isEditMode && this.recipeId) {
      this.router.navigate(['/recipes', this.recipeId]);
    } else {
      this.router.navigate(['/recipes']);
    }
  }

  openImportDialog() {
    const dialogRef = this.dialog.open(RecipeImportDialogComponent, {
      width: '700px',
      maxWidth: '95vw',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe((result: ImportResult) => {
      if (result?.success && result.parsedData) {
        this.populateFromParsedData(result.parsedData);
        
        // Show success message with warnings if any
        if (result.warnings && result.warnings.length > 0) {
          console.warn('Import warnings:', result.warnings);
          // Could show a snackbar here with warnings
        }
      }
    });
  }

  populateFromParsedData(parsed: ParsedRecipe) {
    // Populate basic fields
    this.recipeForm.patchValue({
      title: parsed.title || '',
      yield: parsed.yield || 4,
      prepTimeMinutes: parsed.prepTimeMinutes,
      cookTimeMinutes: parsed.cookTimeMinutes
    });

    // Clear and populate ingredients
    this.ingredients.clear();
    if (parsed.ingredients && parsed.ingredients.length > 0) {
      parsed.ingredients.forEach((ing, index) => {
        const ingredientGroup = this.fb.group({
          sortOrder: [index + 1],
          quantity: [ing.quantity || null],
          unit: [ing.unit || ''],
          item: [ing.item || '', Validators.required],
          preparation: [ing.preparation || '']
        });
        this.ingredients.push(ingredientGroup);
      });
    } else {
      // Add one empty if none parsed
      this.addIngredient();
    }

    // Clear and populate instructions
    this.instructions.clear();
    if (parsed.instructions && parsed.instructions.length > 0) {
      parsed.instructions.forEach((inst, index) => {
        const instructionGroup = this.fb.group({
          stepNumber: [index + 1],
          instruction: [inst.instruction || '', Validators.required]
        });
        this.instructions.push(instructionGroup);
      });
    } else {
      // Add one empty if none parsed
      this.addInstruction();
    }
  }

  onTagsChanged(tags: AssignTagItem[]) {
    if (!this.isEditMode || !this.recipeId) return;

    this.recipeService.assignTags(this.recipeId, { tags }).subscribe({
      next: (recipe) => {
        this.currentRecipeTags = recipe.tags || [];
      },
      error: (err) => {
        console.error('Error assigning tags:', err);
        this.error = 'Failed to assign tags';
      }
    });
  }

  onTagRemoved(tag: RecipeTag) {
    if (!this.isEditMode || !this.recipeId) return;

    this.recipeService.removeTag(this.recipeId, tag.tagResourceId).subscribe({
      next: (recipe) => {
        this.currentRecipeTags = recipe.tags || [];
      },
      error: (err) => {
        console.error('Error removing tag:', err);
        this.error = 'Failed to remove tag';
      }
    });
  }
}
