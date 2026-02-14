import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { FractionPipe } from '../../../pipes/fraction.pipe';
import { TagSelectorComponent } from '../../shared/tag-selector/tag-selector.component';

@Component({
  selector: 'app-recipe-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatDialogModule,
    MatTooltipModule,
    MatExpansionModule,
    FractionPipe,
    TagSelectorComponent
  ],
  templateUrl: './recipe-detail.component.html',
  styleUrl: './recipe-detail.component.scss'
})
export class RecipeDetailComponent implements OnInit {
  recipe: Recipe | null = null;
  loading = false;
  error = '';
  recipeId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.recipeId = this.route.snapshot.paramMap.get('id');
    
    if (this.recipeId) {
      this.loadRecipe();
    }
  }

  loadRecipe() {
    if (!this.recipeId) return;

    this.loading = true;
    this.error = '';

    this.recipeService.getRecipe(this.recipeId).subscribe({
      next: (recipe) => {
        this.recipe = recipe;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load recipe';
        this.loading = false;
        console.error('Error loading recipe:', err);
      }
    });
  }

  goBack() {
    this.router.navigate(['/recipes']);
  }

  editRecipe() {
    if (this.recipeId) {
      this.router.navigate(['/recipes', this.recipeId, 'edit']);
    }
  }

  deleteRecipe() {
    if (!this.recipeId || !this.recipe) return;

    const confirmed = confirm(`Are you sure you want to delete "${this.recipe.title}"?`);
    
    if (confirmed) {
      this.recipeService.deleteRecipe(this.recipeId).subscribe({
        next: () => {
          this.router.navigate(['/recipes']);
        },
        error: (err) => {
          this.error = err.message || 'Failed to delete recipe';
          console.error('Error deleting recipe:', err);
        }
      });
    }
  }

  getTotalTime(): string {
    if (!this.recipe) return 'N/A';

    if (this.recipe.totalTimeMinutes) {
      return this.formatTime(this.recipe.totalTimeMinutes);
    }
    
    const total = (this.recipe.prepTimeMinutes || 0) + (this.recipe.cookTimeMinutes || 0);
    return total > 0 ? this.formatTime(total) : 'N/A';
  }

  formatTime(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} minutes`;
    }
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours} hour${hours > 1 ? 's' : ''} ${mins} minutes` : `${hours} hour${hours > 1 ? 's' : ''}`;
  }

  toggleVisibility() {
    if (!this.recipeId || !this.recipe) return;

    const newVisibility = !this.recipe.isPublic;
    this.recipeService.setVisibility(this.recipeId, newVisibility).subscribe({
      next: (updated) => {
        this.recipe = updated;
      },
      error: (err) => {
        this.error = err.message || 'Failed to update visibility';
        console.error('Error toggling visibility:', err);
      }
    });
  }

  toggleFavorite() {
    if (!this.recipeId || !this.recipe) return;

    const newFavorite = !this.recipe.isFavorite;
    this.recipeService.setFavorite(this.recipeId, newFavorite).subscribe({
      next: (updated) => {
        this.recipe = updated;
      },
      error: (err) => {
        this.error = err.message || 'Failed to update favorite';
        console.error('Error toggling favorite:', err);
      }
    });
  }

  setRating(stars: number) {
    if (!this.recipeId || !this.recipe) return;

    // Click same star to clear rating
    const newRating = this.recipe.rating === stars ? null : stars;
    this.recipeService.setRating(this.recipeId, newRating).subscribe({
      next: (updated) => {
        this.recipe = updated;
      },
      error: (err) => {
        this.error = err.message || 'Failed to update rating';
        console.error('Error setting rating:', err);
      }
    });
  }

  hoverRating = 0;

  printRecipe() {
    window.print();
  }
}
