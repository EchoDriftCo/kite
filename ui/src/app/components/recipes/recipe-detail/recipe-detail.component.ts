import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
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
import { NutritionPanelComponent } from '../nutrition-panel/nutrition-panel.component';
import { CookingLogDialogComponent, CookingLogDialogData, CookingLogDialogResult } from '../cooking-log-dialog/cooking-log-dialog.component';
import { CookingLogService } from '../../../services/cooking-log.service';
import { RecipePersonalStats } from '../../../models/cooking-log.model';
import { EquipmentService } from '../../../services/equipment.service';
import { EquipmentCheckResult } from '../../../models/equipment.model';
import { RecipeLinkService } from '../../../services/recipe-link.service';
import { LinkedRecipe, UsedInRecipe } from '../../../models/recipe-link.model';
import { RecipeLinksComponent } from '../../../shared/components/recipe-links/recipe-links.component';
import { GroceryService } from '../../../services/grocery.service';
import { GroceryCheckoutOptions, GroceryCheckoutUrl } from '../../../models/grocery.model';

@Component({
  selector: 'app-recipe-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatDialogModule,
    MatSnackBarModule,
    NutritionPanelComponent,
    MatTooltipModule,
      MatMenuModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    FractionPipe,
    TagSelectorComponent,
    RecipeLinksComponent
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

  // Personal stats
  personalStats: RecipePersonalStats | null = null;
  loadingStats = false;
  // Equipment
  equipmentCheck: EquipmentCheckResult | null = null;
  loadingEquipment = false;
  detectingEquipment = false;

  // Recipe linking
  linkedRecipes: LinkedRecipe[] = [];
  usedInRecipes: UsedInRecipe[] = [];

  // Grocery delivery links
  groceryOptions: GroceryCheckoutOptions | null = null;
  loadingGroceryOptions = false;
  groceryError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private cookingLogService: CookingLogService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private equipmentService: EquipmentService,
    private recipeLinkService: RecipeLinkService,
    private groceryService: GroceryService
  ) {}

  ngOnInit() {
    this.recipeId = this.route.snapshot.paramMap.get('id');
    
    if (this.recipeId) {
      this.loadRecipe();
      this.loadPersonalStats();
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
        this.loadRecipeEquipment();
        this.loadRecipeLinks(recipe.recipeResourceId);
      },
      error: (err) => {
        this.error = err.message || 'Failed to load recipe';
        this.loading = false;
        console.error('Error loading recipe:', err);
      }
    });
  }

  loadPersonalStats() {
    if (!this.recipeId) return;

    this.loadingStats = true;
    this.cookingLogService.getRecipeStats(this.recipeId).subscribe({
      next: (stats) => {
        this.personalStats = stats;
        this.loadingStats = false;
      },
      error: (err) => {
        // Personal stats are optional, so just log the error
        console.log('No personal stats available for this recipe');
        this.loadingStats = false;
      }
    });
  }

  loadRecipeEquipment() {
    if (!this.recipeId) return;

    this.loadingEquipment = true;
    this.equipmentService.checkRecipeEquipment(this.recipeId).subscribe({
      next: (result) => {
        this.equipmentCheck = result;
        this.loadingEquipment = false;
      },
      error: (err) => {
        console.error('Error loading equipment:', err);
        this.loadingEquipment = false;
      }
    });
  }

  loadRecipeLinks(recipeResourceId: string) {
    this.recipeLinkService.getLinkedRecipes(recipeResourceId).subscribe({
      next: (links) => {
        this.linkedRecipes = links;
      },
      error: (err) => {
        console.error('Error loading linked recipes:', err);
        this.linkedRecipes = [];
      }
    });

    this.recipeLinkService.getUsedInRecipes(recipeResourceId).subscribe({
      next: (usage) => {
        this.usedInRecipes = usage;
      },
      error: (err) => {
        console.error('Error loading used-in recipes:', err);
        this.usedInRecipes = [];
      }
    });
  }

  openCookingLogDialog() {
    if (!this.recipe) return;

    const dialogRef = this.dialog.open(CookingLogDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: {
        recipeId: this.recipe.recipeResourceId,
        recipeTitle: this.recipe.title,
        recipeYield: this.recipe.yield
      } as CookingLogDialogData
    });

    dialogRef.afterClosed().subscribe((result: CookingLogDialogResult | undefined) => {
      if (result && result.success) {
        this.snackBar.open('Cooking session logged! 🍳', 'OK', { duration: 3000 });
        // Reload personal stats to show updated data
        this.loadPersonalStats();
      }
    });
  }

  detectEquipment() {
    if (!this.recipeId) return;

    this.detectingEquipment = true;
    this.equipmentService.detectRecipeEquipment(this.recipeId).subscribe({
      next: () => {
        this.snackBar.open('Equipment detected!', 'Close', { duration: 3000 });
        this.loadRecipeEquipment();
        this.detectingEquipment = false;
      },
      error: (err) => {
        this.snackBar.open('Failed to detect equipment', 'Close', { duration: 3000 });
        console.error('Error detecting equipment:', err);
        this.detectingEquipment = false;
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  }

  isEquipmentOwned(equipmentCode: string): boolean {
    if (!this.equipmentCheck) return false;
    return !this.equipmentCheck.missingEquipment.some(e => e.code === equipmentCode);
  }

  getEquipmentStatusIcon(equipmentCode: string): string {
    return this.isEquipmentOwned(equipmentCode) ? 'check_circle' : 'cancel';
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

  getIngredientLink(index: number): LinkedRecipe | undefined {
    return this.linkedRecipes.find(link => link.ingredientIndex === index);
  }

  loadGroceryOptions() {
    if (!this.recipe?.ingredients?.length) {
      this.groceryError = 'No ingredients found for this recipe.';
      return;
    }

    const ingredientItems = this.recipe.ingredients
      .map(i => i.item)
      .filter((item): item is string => !!item?.trim());

    this.loadingGroceryOptions = true;
    this.groceryError = '';

    this.groceryService.getCheckoutOptions(ingredientItems).subscribe({
      next: (options) => {
        this.groceryOptions = options;
        this.loadingGroceryOptions = false;
      },
      error: (err) => {
        this.groceryOptions = null;
        this.groceryError = err?.message || 'Failed to load grocery checkout links.';
        this.loadingGroceryOptions = false;
      }
    });
  }

  openCheckoutUrl(option: GroceryCheckoutUrl) {
    window.open(option.url, '_blank', 'noopener,noreferrer');
  }

  getServiceDisplayName(service: string): string {
    const names: Record<string, string> = {
      instacart: 'Instacart',
      walmart: 'Walmart Grocery',
      amazonfresh: 'Amazon Fresh',
      shipt: 'Shipt',
      manual: 'Manual List'
    };

    return names[service] || service;
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
        this.snackBar.open('Substitutions applied! Opening in edit mode...', 'OK', { duration: 3000 });
        // Navigate to the forked recipe in edit mode
        this.router.navigate(['/recipes', result.forkedRecipeId, 'edit']);
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

  forkRecipe() {
    if (!this.recipeId || !this.recipe) return;

    const newTitle = `${this.recipe.title} (Copy)`;
    
    this.recipeService.forkRecipe(this.recipeId, newTitle).subscribe({
      next: (forkedRecipe) => {
        this.snackBar.open('Recipe forked! Opening in edit mode...', 'Close', { duration: 3000 });
        this.router.navigate(['/recipes', forkedRecipe.recipeResourceId, 'edit']);
      },
      error: (err) => {
        this.error = err.message || 'Failed to fork recipe';
        this.snackBar.open('Failed to fork recipe', 'Close', { duration: 3000 });
        console.error('Error forking recipe:', err);
      }
    });
  }

  canForkRecipe(): boolean {
    // Can fork if:
    // 1. Recipe exists
    // 2. User is not the owner
    // 3. Recipe is public or shared to user via circle
    return this.recipe != null && this.recipe.isOwner === false;
  }
}
