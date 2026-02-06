import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe, RecipeSearchRequest } from '../../../models/recipe.model';

@Component({
  selector: 'app-recipe-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './recipe-list.component.html',
  styleUrl: './recipe-list.component.scss'
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = false;
  error = '';
  searchTitle = '';
  
  // Pagination
  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;

  constructor(
    private recipeService: RecipeService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadRecipes();
  }

  loadRecipes() {
    this.loading = true;
    this.error = '';

    const request: RecipeSearchRequest = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      title: this.searchTitle || undefined
    };

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
}
