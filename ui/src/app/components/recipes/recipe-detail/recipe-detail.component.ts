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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { FractionPipe } from '../../../pipes/fraction.pipe';
import { TagSelectorComponent } from '../../shared/tag-selector/tag-selector.component';
import {
  SubstitutionDialogComponent,
  SubstitutionDialogData,
  SubstitutionDialogResult
} from '../substitution-dialog/substitution-dialog.component';
import { ShareRecipeDialogComponent } from '../../circles/share-recipe-dialog/share-recipe-dialog.component';

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
    MatSnackBarModule,
    MatTooltipModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
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

  // Scaling
  scaledServings = 0;
  scaleFactor = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
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
        this.scaledServings = recipe.yield;
        this.scaleFactor = 1;
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

  updateScale() {
    if (!this.recipe || this.scaledServings < 1) return;
    this.scaleFactor = this.scaledServings / this.recipe.yield;
  }

  setMultiplier(multiplier: number) {
    if (!this.recipe) return;
    this.scaleFactor = multiplier;
    this.scaledServings = Math.round(this.recipe.yield * multiplier);
  }

  resetScale() {
    if (!this.recipe) return;
    this.scaledServings = this.recipe.yield;
    this.scaleFactor = 1;
  }

  getScaledQuantity(quantity: number | undefined): number | undefined {
    if (quantity == null) return undefined;
    return quantity * this.scaleFactor;
  }

  printRecipe() {
    window.print();
  }

  shareRecipe() {
    if (!this.recipeId || !this.recipe) return;

    if (this.recipe.shareToken) {
      this.copyShareLink(this.recipe.shareToken);
    } else {
      this.recipeService.generateShareToken(this.recipeId).subscribe({
        next: (updated) => {
          this.recipe = updated;
          if (updated.shareToken) {
            this.copyShareLink(updated.shareToken);
          }
        },
        error: (err) => {
          this.error = err.message || 'Failed to generate share link';
          console.error('Error generating share token:', err);
        }
      });
    }
  }

  revokeShareLink() {
    if (!this.recipeId || !this.recipe) return;

    this.recipeService.revokeShareToken(this.recipeId).subscribe({
      next: (updated) => {
        this.recipe = updated;
        this.snackBar.open('Share link revoked', 'OK', { duration: 3000 });
      },
      error: (err) => {
        this.error = err.message || 'Failed to revoke share link';
        console.error('Error revoking share token:', err);
      }
    });
  }

  private copyShareLink(token: string) {
    const url = `${window.location.origin}/share/${token}`;
    navigator.clipboard.writeText(url).then(() => {
      this.snackBar.open('Share link copied to clipboard!', 'OK', { duration: 4000 });
    }).catch(() => {
      this.snackBar.open(`Share link: ${url}`, 'OK', { duration: 8000 });
    });
  }

  openSubstitutionDialog() {
    if (!this.recipe) return;

    const dialogRef = this.dialog.open(SubstitutionDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      data: {
        recipe: this.recipe
      } as SubstitutionDialogData,
      disableClose: false
    });

    dialogRef.afterClosed().subscribe((result: SubstitutionDialogResult | undefined) => {
      if (result && result.forkedRecipeId) {
        this.snackBar.open('Substitutions applied! Opening new recipe...', 'OK', { duration: 3000 });
        // Navigate to the forked recipe
        this.router.navigate(['/recipes', result.forkedRecipeId]);
      }
    });
  }

  shareToCircles() {
    if (!this.recipeId || !this.recipe) return;

    const dialogRef = this.dialog.open(ShareRecipeDialogComponent, {
      width: '500px',
      data: {
        recipeId: this.recipeId,
        recipeTitle: this.recipe.title
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Recipe shared to circles!', 'Close', { duration: 3000 });
      }
    });
  }
}
