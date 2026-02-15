import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { FractionPipe } from '../../../pipes/fraction.pipe';
import { TagSelectorComponent } from '../../shared/tag-selector/tag-selector.component';

@Component({
  selector: 'app-shared-recipe',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    FractionPipe,
    TagSelectorComponent
  ],
  templateUrl: './shared-recipe.component.html',
  styleUrl: './shared-recipe.component.scss'
})
export class SharedRecipeComponent implements OnInit {
  recipe: Recipe | null = null;
  loading = false;
  error = '';

  // Scaling
  scaledServings = 0;
  scaleFactor = 1;

  constructor(
    private route: ActivatedRoute,
    private recipeService: RecipeService
  ) {}

  ngOnInit() {
    const token = this.route.snapshot.paramMap.get('token');
    if (token) {
      this.loadSharedRecipe(token);
    }
  }

  loadSharedRecipe(token: string) {
    this.loading = true;
    this.error = '';
    this.recipeService.getSharedRecipe(token).subscribe({
      next: (recipe) => {
        this.recipe = recipe;
        this.scaledServings = recipe.yield;
        this.scaleFactor = 1;
        this.loading = false;
      },
      error: () => {
        this.error = 'This shared recipe link is no longer valid.';
        this.loading = false;
      }
    });
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
}
