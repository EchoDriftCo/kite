import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';

@Component({
    selector: 'app-recipe-list',
    imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule],
    templateUrl: './recipe-list.component.html',
    styleUrls: ['./recipe-list.component.scss']
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = false;
  error: string | null = null;

  constructor(private recipeService: RecipeService) {}

  ngOnInit(): void {
    this.loadRecipes();
  }

  loadRecipes(): void {
    this.loading = true;
    this.error = null;
    
    this.recipeService.getAllRecipes().subscribe({
      next: (recipes) => {
        this.recipes = recipes;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load recipes. Make sure the API is running.';
        this.loading = false;
        console.error('Error loading recipes:', err);
      }
    });
  }

  deleteRecipe(id: string): void {
    if (confirm('Are you sure you want to delete this recipe?')) {
      this.recipeService.deleteRecipe(id).subscribe({
        next: () => {
          this.loadRecipes();
        },
        error: (err) => {
          this.error = 'Failed to delete recipe.';
          console.error('Error deleting recipe:', err);
        }
      });
    }
  }
}
