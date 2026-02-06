import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe, CreateRecipeRequest, UpdateRecipeRequest } from '../../../models/recipe.model';

@Component({
    selector: 'app-recipe-form',
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule
    ],
    templateUrl: './recipe-form.component.html',
    styleUrls: ['./recipe-form.component.scss']
})
export class RecipeFormComponent implements OnInit {
  recipeForm: FormGroup;
  isEditMode = false;
  recipeId: string | null = null;
  loading = false;
  error: string | null = null;
  selectedFile: File | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService
  ) {
    this.recipeForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      category: [''],
      prepTime: [null],
      cookTime: [null],
      servings: [null],
      ingredients: [''],
      instructions: ['']
    });
  }

  ngOnInit(): void {
    this.recipeId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.recipeId;

    if (this.isEditMode && this.recipeId) {
      this.loadRecipe(this.recipeId);
    }
  }

  loadRecipe(id: string): void {
    this.loading = true;
    this.recipeService.getRecipeById(id).subscribe({
      next: (recipe) => {
        this.recipeForm.patchValue({
          name: recipe.name,
          description: recipe.description,
          category: recipe.category,
          prepTime: recipe.prepTime,
          cookTime: recipe.cookTime,
          servings: recipe.servings,
          ingredients: recipe.ingredients?.join('\n'),
          instructions: recipe.instructions
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load recipe.';
        this.loading = false;
        console.error('Error loading recipe:', err);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onSubmit(): void {
    if (this.recipeForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = null;

    const formValue = this.recipeForm.value;
    const recipeData = {
      name: formValue.name,
      description: formValue.description,
      category: formValue.category,
      prepTime: formValue.prepTime,
      cookTime: formValue.cookTime,
      servings: formValue.servings,
      ingredients: formValue.ingredients ? formValue.ingredients.split('\n').filter((i: string) => i.trim()) : [],
      instructions: formValue.instructions
    };

    const request$ = this.isEditMode && this.recipeId
      ? this.recipeService.updateRecipe({ ...recipeData, id: this.recipeId } as UpdateRecipeRequest)
      : this.recipeService.createRecipe(recipeData as CreateRecipeRequest);

    request$.subscribe({
      next: (recipe) => {
        // If there's a file to upload, do that next
        if (this.selectedFile && recipe.id) {
          this.recipeService.uploadRecipeImage(recipe.id, this.selectedFile).subscribe({
            next: () => {
              this.router.navigate(['/recipes', recipe.id]);
            },
            error: (err) => {
              console.error('Error uploading image:', err);
              // Still navigate even if image upload fails
              this.router.navigate(['/recipes', recipe.id]);
            }
          });
        } else {
          this.router.navigate(['/recipes', recipe.id]);
        }
      },
      error: (err) => {
        this.error = `Failed to ${this.isEditMode ? 'update' : 'create'} recipe.`;
        this.loading = false;
        console.error('Error saving recipe:', err);
      }
    });
  }

  onCancel(): void {
    if (this.isEditMode && this.recipeId) {
      this.router.navigate(['/recipes', this.recipeId]);
    } else {
      this.router.navigate(['/recipes']);
    }
  }
}
