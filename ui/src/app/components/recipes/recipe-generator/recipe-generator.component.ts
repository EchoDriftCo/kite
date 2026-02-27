import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { RecipeGenerationService } from '../../../services/recipe-generation.service';
import { DietaryProfileService } from '../../../services/dietary-profile.service';
import {
  GenerateRecipeRequest,
  GeneratedRecipe
} from '../../../models/recipe-generation.model';
import { FractionPipe } from '../../../pipes/fraction.pipe';

@Component({
  selector: 'app-recipe-generator',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDividerModule,
    MatSnackBarModule,
    MatExpansionModule,
    FractionPipe
  ],
  templateUrl: './recipe-generator.component.html',
  styleUrl: './recipe-generator.component.scss'
})
export class RecipeGeneratorComponent implements OnInit {
  // Form fields
  prompt = '';
  maxTime: number | null = null;
  skillLevel = '';
  selectedDietary: string[] = [];

  // Available options
  timeOptions = [15, 30, 45, 60, 90, 120];
  skillLevels = ['Beginner', 'Intermediate', 'Advanced'];
  dietaryOptions: string[] = [];

  // Generated recipe
  generatedRecipes: GeneratedRecipe[] = [];
  selectedRecipeIndex = 0;
  selectedRecipe: GeneratedRecipe | null = null;

  // Refinement
  refinementText = '';
  showRefinementInput = false;

  // State
  loading = false;
  quotaRemaining = 10;

  constructor(
    private generationService: RecipeGenerationService,
    private dietaryProfileService: DietaryProfileService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadQuota();
    this.loadDietaryOptions();
  }

  /**
   * Load remaining generation quota
   */
  loadQuota() {
    this.generationService.getRemainingQuota().subscribe({
      next: quota => {
        this.quotaRemaining = quota.remaining;
      },
      error: () => {
        // Silently fail, default to 10
      }
    });
  }

  /**
   * Load dietary options from user's profile
   */
  loadDietaryOptions() {
    this.dietaryProfileService.getProfiles().subscribe({
      next: profiles => {
        const defaultProfile = profiles.find(p => p.isDefault);
        if (defaultProfile?.restrictions) {
          this.dietaryOptions = defaultProfile.restrictions.map(r => r.restrictionCode);
        } else {
          // Fallback to common options
          this.dietaryOptions = [
            'Vegetarian',
            'Vegan',
            'Gluten-Free',
            'Dairy-Free',
            'Keto',
            'Paleo',
            'Low-Carb'
          ];
        }
      },
      error: () => {
        // Fallback to common options
        this.dietaryOptions = [
          'Vegetarian',
          'Vegan',
          'Gluten-Free',
          'Dairy-Free',
          'Keto',
          'Paleo',
          'Low-Carb'
        ];
      }
    });
  }

  /**
   * Generate recipes from prompt
   */
  generateRecipes() {
    if (!this.prompt.trim()) {
      this.snackBar.open('Please describe what recipe you want', 'Close', { duration: 3000 });
      return;
    }

    if (this.quotaRemaining <= 0) {
      this.snackBar.open('Daily generation limit reached. Try again tomorrow!', 'Close', { duration: 5000 });
      return;
    }

    const request: GenerateRecipeRequest = {
      prompt: this.prompt.trim(),
      variations: 1 // Start with 1, can add more later
    };

    if (this.maxTime) {
      request.maxTime = this.maxTime;
    }

    if (this.skillLevel) {
      request.skillLevel = this.skillLevel.toLowerCase();
    }

    if (this.selectedDietary.length > 0) {
      request.dietary = this.selectedDietary;
    }

    this.loading = true;
    this.generatedRecipes = [];
    this.selectedRecipe = null;

    this.generationService.generateRecipes(request).subscribe({
      next: recipes => {
        this.loading = false;
        if (recipes && recipes.length > 0) {
          this.generatedRecipes = recipes;
          this.selectedRecipeIndex = 0;
          this.selectedRecipe = recipes[0];
          this.showRefinementInput = false;
          this.refinementText = '';
          this.loadQuota(); // Refresh quota
          this.snackBar.open('Recipe generated successfully!', 'Close', { duration: 3000 });
        } else {
          this.snackBar.open('No recipes generated. Try a different prompt.', 'Close', { duration: 4000 });
        }
      },
      error: err => {
        this.loading = false;
        const message = err.error?.detail || 'Failed to generate recipe. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  /**
   * Refine the current recipe
   */
  refineRecipe() {
    if (!this.selectedRecipe) return;

    if (!this.refinementText.trim()) {
      this.snackBar.open('Please describe how to refine the recipe', 'Close', { duration: 3000 });
      return;
    }

    if (this.quotaRemaining <= 0) {
      this.snackBar.open('Daily generation limit reached. Try again tomorrow!', 'Close', { duration: 5000 });
      return;
    }

    this.loading = true;

    this.generationService.refineRecipe({
      previousRecipe: this.selectedRecipe,
      refinement: this.refinementText.trim()
    }).subscribe({
      next: refined => {
        this.loading = false;
        this.selectedRecipe = refined;
        this.generatedRecipes[this.selectedRecipeIndex] = refined;
        this.showRefinementInput = false;
        this.refinementText = '';
        this.loadQuota(); // Refresh quota
        this.snackBar.open('Recipe refined successfully!', 'Close', { duration: 3000 });
      },
      error: err => {
        this.loading = false;
        const message = err.error?.detail || 'Failed to refine recipe. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  /**
   * Save the generated recipe to user's library
   */
  saveRecipe() {
    if (!this.selectedRecipe) return;

    this.loading = true;

    this.generationService.saveGeneratedRecipe(this.selectedRecipe).subscribe({
      next: savedRecipe => {
        this.loading = false;
        this.snackBar.open('Recipe saved to your library!', 'View', { duration: 5000 })
          .onAction().subscribe(() => {
            this.router.navigate(['/recipes', savedRecipe.recipeResourceId]);
          });
        
        // Clear form for next generation
        this.prompt = '';
        this.generatedRecipes = [];
        this.selectedRecipe = null;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to save recipe. Please try again.', 'Close', { duration: 4000 });
      }
    });
  }

  /**
   * Regenerate with same constraints
   */
  regenerate() {
    this.generateRecipes();
  }

  /**
   * Toggle refinement input visibility
   */
  toggleRefinement() {
    this.showRefinementInput = !this.showRefinementInput;
    if (!this.showRefinementInput) {
      this.refinementText = '';
    }
  }

  /**
   * Clear form and start over
   */
  startOver() {
    this.prompt = '';
    this.maxTime = null;
    this.skillLevel = '';
    this.selectedDietary = [];
    this.generatedRecipes = [];
    this.selectedRecipe = null;
    this.showRefinementInput = false;
    this.refinementText = '';
  }

  /**
   * Handle dietary checkbox changes
   */
  onDietaryChange(option: string, checked: boolean) {
    if (checked) {
      if (!this.selectedDietary.includes(option)) {
        this.selectedDietary.push(option);
      }
    } else {
      this.selectedDietary = this.selectedDietary.filter(d => d !== option);
    }
  }

  /**
   * Format ingredient text
   */
  formatIngredient(ingredient: any): string {
    const parts: string[] = [];
    
    if (ingredient.quantity) {
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
}
