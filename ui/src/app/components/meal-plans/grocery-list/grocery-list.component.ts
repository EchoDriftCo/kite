import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MealPlanService } from '../../../services/meal-plan.service';
import { GroceryItem, GroceryList } from '../../../models/meal-plan.model';

@Component({
  selector: 'app-grocery-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatDividerModule
  ],
  templateUrl: './grocery-list.component.html',
  styleUrl: './grocery-list.component.scss'
})
export class GroceryListComponent implements OnInit {
  groceryList: GroceryList | null = null;
  loading = false;
  error = '';
  planId: string | null = null;
  checkedItems = new Set<number>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private mealPlanService: MealPlanService
  ) {}

  ngOnInit() {
    this.planId = this.route.snapshot.paramMap.get('id');
    if (this.planId) {
      this.loadGroceryList();
    }
  }

  loadGroceryList() {
    if (!this.planId) return;

    this.loading = true;
    this.error = '';

    this.mealPlanService.getGroceryList(this.planId).subscribe({
      next: (list) => {
        this.groceryList = list;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load grocery list';
        this.loading = false;
      }
    });
  }

  toggleItem(index: number) {
    if (this.checkedItems.has(index)) {
      this.checkedItems.delete(index);
    } else {
      this.checkedItems.add(index);
    }
  }

  isChecked(index: number): boolean {
    return this.checkedItems.has(index);
  }

  formatQuantity(item: GroceryItem): string {
    if (!item.quantity) return '';
    // Round to 2 decimal places to avoid floating point display issues
    const qty = Math.round(item.quantity * 100) / 100;
    return `${qty}`;
  }

  get checkedCount(): number {
    return this.checkedItems.size;
  }

  get totalCount(): number {
    return this.groceryList?.items?.length || 0;
  }

  printList() {
    window.print();
  }

  goBack() {
    if (this.planId) {
      this.router.navigate(['/meal-plans', this.planId]);
    } else {
      this.router.navigate(['/meal-plans']);
    }
  }
}
