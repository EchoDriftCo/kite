import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NutritionService } from '../../../services/nutrition.service';
import { RecipeNutrition } from '../../../models/nutrition.model';

@Component({
  selector: 'app-nutrition-panel',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './nutrition-panel.component.html',
  styleUrls: ['./nutrition-panel.component.scss']
})
export class NutritionPanelComponent implements OnInit {
  @Input() recipeResourceId!: string;
  @Input() recipeYield: number = 1;

  nutrition: RecipeNutrition | null = null;
  loading = false;
  error: string | null = null;
  analyzing = false;

  constructor(private nutritionService: NutritionService) {}

  ngOnInit(): void {
    this.loadNutrition();
  }

  loadNutrition(): void {
    this.loading = true;
    this.error = null;

    this.nutritionService.getRecipeNutrition(this.recipeResourceId).subscribe({
      next: (nutrition) => {
        this.nutrition = nutrition;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading nutrition:', err);
        this.loading = false;
        // 404 = not analyzed yet, 401/403 = auth/tier issue (shouldn't happen if gated)
        if (err.status !== 404 && err.status !== 401 && err.status !== 403) {
          this.error = 'Failed to load nutrition data';
        }
      }
    });
  }

  analyzeNutrition(): void {
    this.analyzing = true;
    this.error = null;

    this.nutritionService.analyzeRecipeNutrition(this.recipeResourceId).subscribe({
      next: (nutrition) => {
        this.nutrition = nutrition;
        this.analyzing = false;
      },
      error: (err) => {
        console.error('Error analyzing nutrition:', err);
        this.error = 'Failed to analyze nutrition. Please try again.';
        this.analyzing = false;
      }
    });
  }

  getPercentDV(nutrient: 'protein' | 'carbs' | 'fat' | 'fiber' | 'sodium', value?: number): number {
    if (!value) return 0;

    const dailyValues = {
      protein: 50,       // grams
      carbs: 275,        // grams
      fat: 78,           // grams
      fiber: 28,         // grams
      sodium: 2300       // mg
    };

    return Math.round((value / dailyValues[nutrient]) * 100);
  }

  get coverageGood(): boolean {
    return this.nutrition ? this.nutrition.coveragePercent >= 75 : false;
  }

  get coverageOkay(): boolean {
    return this.nutrition ? this.nutrition.coveragePercent >= 50 && this.nutrition.coveragePercent < 75 : false;
  }

  get coveragePoor(): boolean {
    return this.nutrition ? this.nutrition.coveragePercent < 50 : false;
  }
}
