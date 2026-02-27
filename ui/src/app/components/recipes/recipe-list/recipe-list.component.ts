import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../../services/recipe.service';
import { TagService } from '../../../services/tag.service';
import { Recipe, RecipeSearchRequest } from '../../../models/recipe.model';
import { Tag, getCategoryName } from '../../../models/tag.model';
import { EquipmentFilterComponent } from '../../equipment-filter/equipment-filter.component';

@Component({
  selector: 'app-recipe-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonToggleModule,
    MatTooltipModule,
    EquipmentFilterComponent
  ],
  templateUrl: './recipe-list.component.html',
  styleUrl: './recipe-list.component.scss'
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = false;
  error = '';
  searchTitle = '';
  viewMode: 'mine' | 'public' = 'mine';
  favoritesOnly = false;
  showTagFilters = false;
  availableTags: Tag[] = [];
  selectedTagIds: string[] = [];
  equipmentFilterEnabled = false;

  // Pagination
  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;

  constructor(
    private recipeService: RecipeService,
    private tagService: TagService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadRecipes();
    this.loadTags();
  }

  loadRecipes() {
    this.loading = true;
    this.error = '';

    const request: RecipeSearchRequest = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      title: this.searchTitle || undefined
    };

    if (this.viewMode === 'public') {
      request.includePublic = true;
      request.isPublic = true;
    }

    if (this.favoritesOnly) {
      request.isFavorite = true;
    }

    if (this.selectedTagIds.length > 0) {
      request.tagResourceIds = this.selectedTagIds;
    }

    if (this.equipmentFilterEnabled) {
      request.hasRequiredEquipment = true;
    }

    this.recipeService.searchRecipes(request).subscribe({
      next: (response) => {
        this.recipes = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load recipes';
        this.loading = false;
        console.error('Error loading recipes:', err);
      }
    });
  }

  onViewModeChange() {
    this.pageNumber = 1;
    this.loadRecipes();
  }

  onSearch() {
    this.pageNumber = 1; // Reset to first page
    this.loadRecipes();
  }

  onPageChange(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadRecipes();
  }

  viewRecipe(recipe: Recipe) {
    this.router.navigate(['/recipes', recipe.recipeResourceId]);
  }

  editRecipe(recipe: Recipe, event: Event) {
    event.stopPropagation();
    this.router.navigate(['/recipes', recipe.recipeResourceId, 'edit']);
  }

  createNew() {
    this.router.navigate(['/recipes/new']);
  }

  getTotalTime(recipe: Recipe): string {
    if (recipe.totalTimeMinutes) {
      return this.formatTime(recipe.totalTimeMinutes);
    }

    const total = (recipe.prepTimeMinutes || 0) + (recipe.cookTimeMinutes || 0);
    return total > 0 ? this.formatTime(total) : 'N/A';
  }

  formatTime(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} min`;
    }
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  }

  getIngredientCount(recipe: Recipe): number {
    return recipe.ingredients?.length || 0;
  }

  getStepCount(recipe: Recipe): number {
    return recipe.instructions?.length || 0;
  }

  toggleFavoritesFilter() {
    this.favoritesOnly = !this.favoritesOnly;
    this.pageNumber = 1;
    this.loadRecipes();
  }

  onEquipmentFilterChange(enabled: boolean) {
    this.equipmentFilterEnabled = enabled;
    this.pageNumber = 1;
    this.loadRecipes();
  }

  loadTags() {
    this.tagService.searchTags({ pageSize: 100, sortBy: 'Name' }).subscribe({
      next: (response) => {
        this.availableTags = response.items || [];
      },
      error: (err) => {
        console.error('Error loading tags:', err);
      }
    });
  }

  toggleTagFilter(tag: Tag) {
    const index = this.selectedTagIds.indexOf(tag.tagResourceId);
    if (index >= 0) {
      this.selectedTagIds.splice(index, 1);
    } else {
      this.selectedTagIds.push(tag.tagResourceId);
    }
    this.pageNumber = 1;
    this.loadRecipes();
  }

  isTagSelected(tag: Tag): boolean {
    return this.selectedTagIds.includes(tag.tagResourceId);
  }

  clearTagFilters() {
    this.selectedTagIds = [];
    this.pageNumber = 1;
    this.loadRecipes();
  }

  getCategoryName = getCategoryName;

  toggleFavorite(recipe: Recipe, event: Event) {
    event.stopPropagation();
    const newFavorite = !recipe.isFavorite;
    this.recipeService.setFavorite(recipe.recipeResourceId, newFavorite).subscribe({
      next: (updated) => {
        const index = this.recipes.findIndex(r => r.recipeResourceId === updated.recipeResourceId);
        if (index >= 0) {
          this.recipes[index] = updated;
        }
      },
      error: (err) => {
        console.error('Error toggling favorite:', err);
      }
    });
  }
}
