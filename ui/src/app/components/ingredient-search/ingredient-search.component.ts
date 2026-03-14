import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, switchMap, of } from 'rxjs';
import { IngredientSearchService } from '../../services/ingredient-search.service';
import {
  IngredientSearchResult,
  IngredientSuggestion,
  UserPantryItem,
  CreatePantryItem
} from '../../models/ingredient-search.model';

@Component({
  selector: 'app-ingredient-search',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule, MatAutocompleteModule,
    MatChipsModule, MatIconModule, MatButtonModule,
    MatSelectModule, MatCheckboxModule, MatExpansionModule,
    MatPaginatorModule, MatCardModule, MatProgressSpinnerModule,
    MatTabsModule, MatDatepickerModule, MatNativeDateModule,
    MatSlideToggleModule, MatListModule, MatTooltipModule
  ],
  templateUrl: './ingredient-search.component.html',
  styleUrls: ['./ingredient-search.component.scss']
})
export class IngredientSearchComponent implements OnInit {
  private service = inject(IngredientSearchService);

  // Search Tab
  ingredientControl = new FormControl('');
  selectedIngredients = signal<string[]>([]);
  suggestions = signal<IngredientSuggestion[]>([]);
  results = signal<IngredientSearchResult[]>([]);
  loading = signal(false);
  searched = signal(false);

  // Filters
  maxMissing = signal(3);
  includePantryStaples = signal(true);
  includeSubstitutions = signal(false);
  maxCookTime = signal<number | null>(null);
  sortBy = signal<'matchPercentage' | 'rating' | 'cookTime'>('matchPercentage');

  // Pagination
  totalCount = signal(0);
  pageSize = signal(25);
  pageNumber = signal(1);

  // My Pantry Tab
  pantryItems = signal<UserPantryItem[]>([]);
  pantryLoading = signal(false);
  newPantryItemName = new FormControl('');
  newPantryItemIsStaple = signal(false);
  newPantryItemExpiration = signal<Date | null>(null);

  // Edit state
  editingItemId = signal<number | null>(null);
  editItemName = new FormControl('');
  editItemExpiration = signal<Date | null>(null);

  ngOnInit(): void {
    // Autocomplete with debounce and client-side caching
    this.ingredientControl.valueChanges.pipe(
      debounceTime(300),
      switchMap(query => query && query.length >= 2
        ? this.service.suggestIngredients(query)
        : of([]))
    ).subscribe(suggestions => this.suggestions.set(suggestions));

    // Load user pantry
    this.loadPantry();
  }

  // =========================================================================
  // Search Tab
  // =========================================================================

  addIngredient(name: string): void {
    const trimmed = name.trim().toLowerCase();
    if (trimmed && !this.selectedIngredients().includes(trimmed)) {
      this.selectedIngredients.update(items => [...items, trimmed]);
    }
    this.ingredientControl.setValue('');
    this.suggestions.set([]);
  }

  removeIngredient(name: string): void {
    this.selectedIngredients.update(items => items.filter(i => i !== name));
  }

  search(): void {
    if (this.selectedIngredients().length === 0) return;

    this.loading.set(true);
    this.searched.set(true);
    this.service.searchByIngredients({
      ingredients: this.selectedIngredients(),
      maxMissingIngredients: this.maxMissing(),
      includePantryStaples: this.includePantryStaples(),
      includeSubstitutions: this.includeSubstitutions(),
      maxCookTimeMinutes: this.maxCookTime() ?? undefined,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy()
    }).subscribe({
      next: result => {
        this.results.set(result.items);
        this.totalCount.set(result.totalItems);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.search();
  }

  getMatchColor(percentage: number): string {
    if (percentage >= 75) return '#4caf50';
    if (percentage >= 50) return '#ff9800';
    return '#f44336';
  }

  loadPantryIntoSearch(): void {
    const pantryIngredientNames = this.pantryItems().map(item => item.ingredientName);
    const current = this.selectedIngredients();
    const combined = [...new Set([...current, ...pantryIngredientNames])];
    this.selectedIngredients.set(combined);
  }

  // =========================================================================
  // My Pantry Tab
  // =========================================================================

  loadPantry(): void {
    this.pantryLoading.set(true);
    this.service.getUserPantry().subscribe({
      next: items => {
        this.pantryItems.set(items);
        this.pantryLoading.set(false);
      },
      error: () => this.pantryLoading.set(false)
    });
  }

  addPantryItem(): void {
    const name = this.newPantryItemName.value?.trim();
    if (!name) return;

    const newItem: CreatePantryItem = {
      ingredientName: name,
      isStaple: this.newPantryItemIsStaple(),
      expirationDate: this.newPantryItemExpiration()?.toISOString()
    };

    this.service.addPantryItem(newItem).subscribe({
      next: created => {
        this.pantryItems.update(items => [...items, created]);
        this.newPantryItemName.setValue('');
        this.newPantryItemIsStaple.set(false);
        this.newPantryItemExpiration.set(null);
      }
    });
  }

  deletePantryItem(item: UserPantryItem): void {
    if (!item.userPantryItemId) return;

    this.service.deletePantryItem(item.userPantryItemId).subscribe({
      next: () => {
        this.pantryItems.update(items =>
          items.filter(i => i.userPantryItemId !== item.userPantryItemId)
        );
      }
    });
  }

  toggleStaple(item: UserPantryItem): void {
    if (!item.userPantryItemId) return;

    const updated = {
      ingredientName: item.ingredientName,
      isStaple: !item.isStaple,
      expirationDate: item.expirationDate
    };
    this.service.updatePantryItem(item.userPantryItemId, updated).subscribe({
      next: result => {
        this.pantryItems.update(items =>
          items.map(i => i.userPantryItemId === result.userPantryItemId ? result : i)
        );
      }
    });
  }

  get pantryStaples(): UserPantryItem[] {
    return this.pantryItems().filter(item => item.isStaple);
  }

  get pantryIngredients(): UserPantryItem[] {
    return this.pantryItems().filter(item => !item.isStaple);
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      const value = this.ingredientControl.value?.trim();
      if (value) {
        this.addIngredient(value);
        event.preventDefault();
      }
    }
  }
}
