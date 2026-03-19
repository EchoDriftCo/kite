import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { RecipeMixingService, MixedRecipePreview } from '../../../services/recipe-mixing.service';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { Observable } from 'rxjs';
import { map, startWith, debounceTime, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-recipe-mixer',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    MatSnackBarModule,
    MatAutocompleteModule
  ],
  templateUrl: './recipe-mixer.component.html',
  styleUrls: ['./recipe-mixer.component.scss']
})
export class RecipeMixerComponent implements OnInit {
  recipeAControl = new FormControl('');
  recipeBControl = new FormControl('');
  filteredRecipesA!: Observable<Recipe[]>;
  filteredRecipesB!: Observable<Recipe[]>;
  
  selectedRecipeA?: Recipe;
  selectedRecipeB?: Recipe;
  mode: 'guided' | 'surprise' | 'bestOfBoth' = 'bestOfBoth';
  intent = '';
  
  preview?: MixedRecipePreview;
  isLoading = false;
  isSaving = false;
  refinementNotes = '';

  constructor(
    private mixingService: RecipeMixingService,
    private recipeService: RecipeService,
    private snackBar: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Setup autocomplete for recipe A
    this.filteredRecipesA = this.recipeAControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(value => this._filterRecipes(value || ''))
    );

    // Setup autocomplete for recipe B
    this.filteredRecipesB = this.recipeBControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(value => this._filterRecipes(value || ''))
    );

    // Check for pre-selected recipe from route params
    this.route.queryParams.subscribe(params => {
      if (params['recipeId']) {
        this.recipeService.getRecipe(params['recipeId']).subscribe({
          next: (recipe) => {
            this.selectedRecipeA = recipe;
            this.recipeAControl.setValue(recipe.title);
          },
          error: (err) => console.error('Failed to load pre-selected recipe', err)
        });
      }
    });
  }

  private _filterRecipes(value: string): Observable<Recipe[]> {
    if (typeof value !== 'string' || value.length < 2) {
      return new Observable(observer => observer.next([]));
    }

    return this.recipeService.searchRecipes({ 
      title: value, 
      pageNumber: 1, 
      pageSize: 10 
    }).pipe(
      map(result => result.items || [])
    );
  }

  displayRecipe(recipe: Recipe): string {
    return recipe ? recipe.title : '';
  }

  selectRecipeA(recipe: Recipe) {
    this.selectedRecipeA = recipe;
  }

  selectRecipeB(recipe: Recipe) {
    this.selectedRecipeB = recipe;
  }

  canMix(): boolean {
    return !!(this.selectedRecipeA && this.selectedRecipeB && 
              (this.mode !== 'guided' || this.intent.trim().length > 0));
  }

  mixRecipes() {
    if (!this.canMix()) return;

    this.isLoading = true;
    this.mixingService.mixRecipes({
      recipeAId: this.selectedRecipeA!.recipeResourceId,
      recipeBId: this.selectedRecipeB!.recipeResourceId,
      intent: this.intent,
      mode: this.mode
    }).subscribe({
      next: (preview) => {
        this.preview = preview;
        this.isLoading = false;
        this.snackBar.open('Recipe mixed successfully!', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open('Failed to mix recipes: ' + (err.error?.error || err.message), 'Close', { duration: 5000 });
      }
    });
  }

  regenerate() {
    this.mixRecipes();
  }

  refine() {
    if (!this.preview || !this.refinementNotes.trim()) return;

    this.isLoading = true;
    this.mixingService.refineMix({
      preview: this.preview,
      refinementNotes: this.refinementNotes
    }).subscribe({
      next: (preview) => {
        this.preview = preview;
        this.isLoading = false;
        this.refinementNotes = '';
        this.snackBar.open('Recipe refined!', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open('Failed to refine recipe: ' + (err.error?.error || err.message), 'Close', { duration: 5000 });
      }
    });
  }

  save() {
    if (!this.preview) return;

    this.isSaving = true;
    this.mixingService.saveMixedRecipe(this.preview).subscribe({
      next: (recipe) => {
        this.isSaving = false;
        this.snackBar.open('Mixed recipe saved!', 'Close', { duration: 3000 });
        this.router.navigate(['/recipes', recipe.recipeResourceId]);
      },
      error: (err) => {
        this.isSaving = false;
        this.snackBar.open('Failed to save recipe: ' + (err.error?.error || err.message), 'Close', { duration: 5000 });
      }
    });
  }

  reset() {
    this.preview = undefined;
    this.refinementNotes = '';
  }
}
