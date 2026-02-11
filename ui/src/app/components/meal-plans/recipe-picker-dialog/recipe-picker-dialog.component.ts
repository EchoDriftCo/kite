import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

export interface RecipePickerData {
  date: string;
  mealSlot: number;
}

export interface RecipePickerResult {
  recipeResourceId: string;
  recipeTitle: string;
  servings?: number;
}

@Component({
  selector: 'app-recipe-picker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatListModule
  ],
  templateUrl: './recipe-picker-dialog.component.html',
  styleUrl: './recipe-picker-dialog.component.scss'
})
export class RecipePickerDialogComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = false;
  searchTerm = '';
  servings: number | null = null;
  selectedRecipe: Recipe | null = null;

  private searchSubject = new Subject<string>();

  constructor(
    private dialogRef: MatDialogRef<RecipePickerDialogComponent>,
    private recipeService: RecipeService,
    @Inject(MAT_DIALOG_DATA) public data: RecipePickerData
  ) {}

  ngOnInit() {
    this.loadRecipes();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        this.loading = true;
        return this.recipeService.searchRecipes({
          pageNumber: 1,
          pageSize: 20,
          title: term || undefined,
          includePublic: true
        });
      })
    ).subscribe({
      next: (result) => {
        this.recipes = result.items || [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadRecipes() {
    this.loading = true;
    this.recipeService.searchRecipes({
      pageNumber: 1,
      pageSize: 20,
      includePublic: true
    }).subscribe({
      next: (result) => {
        this.recipes = result.items || [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(term: string) {
    this.searchSubject.next(term);
  }

  selectRecipe(recipe: Recipe) {
    this.selectedRecipe = recipe;
    this.servings = recipe.yield;
  }

  confirm() {
    if (!this.selectedRecipe) return;

    const result: RecipePickerResult = {
      recipeResourceId: this.selectedRecipe.recipeResourceId,
      recipeTitle: this.selectedRecipe.title,
      servings: this.servings || undefined
    };

    this.dialogRef.close(result);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
