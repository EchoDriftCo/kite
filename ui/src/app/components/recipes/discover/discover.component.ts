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
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';

@Component({
  selector: 'app-discover',
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
    MatSelectModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './discover.component.html',
  styleUrl: './discover.component.scss'
})
export class DiscoverComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = false;
  error = '';
  searchTitle = '';
  sortBy = 'newest';
  forkingId: string | null = null;

  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;

  readonly sortOptions = [
    { value: 'newest', label: 'Newest' },
    { value: 'popular', label: 'Most Forked' },
    { value: 'rating', label: 'Top Rated' }
  ];

  constructor(
    private recipeService: RecipeService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadRecipes();
  }

  loadRecipes() {
    this.loading = true;
    this.error = '';

    this.recipeService.discoverRecipes({
      title: this.searchTitle || undefined,
      sortBy: this.sortBy,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.recipes = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load recipes';
        this.loading = false;
      }
    });
  }

  onSearch() {
    this.pageNumber = 1;
    this.loadRecipes();
  }

  onSortChange() {
    this.pageNumber = 1;
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

  forkToVault(recipe: Recipe, event: Event) {
    event.stopPropagation();
    this.forkingId = recipe.recipeResourceId;
    this.recipeService.forkRecipe(recipe.recipeResourceId).subscribe({
      next: (forked) => {
        this.forkingId = null;
        this.snackBar.open(`"${forked.title}" added to your vault!`, 'View', { duration: 4000 })
          .onAction().subscribe(() => {
            this.router.navigate(['/recipes', forked.recipeResourceId]);
          });
        // Update fork count in UI
        const idx = this.recipes.findIndex(r => r.recipeResourceId === recipe.recipeResourceId);
        if (idx >= 0) {
          this.recipes[idx] = { ...this.recipes[idx], forkCount: (this.recipes[idx].forkCount || 0) + 1 };
        }
      },
      error: (err) => {
        this.forkingId = null;
        this.snackBar.open(err.message || 'Failed to fork recipe', 'Dismiss', { duration: 4000 });
      }
    });
  }

  getTotalTime(recipe: Recipe): string {
    const total = (recipe.prepTimeMinutes || 0) + (recipe.cookTimeMinutes || 0);
    if (total === 0) return 'N/A';
    if (total < 60) return `${total} min`;
    const h = Math.floor(total / 60);
    const m = total % 60;
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
  }

  getCreatorName(recipe: Recipe): string {
    return (recipe as any).createdSubject
      ? `${(recipe as any).createdSubject.givenName || ''} ${(recipe as any).createdSubject.familyName || ''}`.trim()
      : 'Unknown';
  }
}
