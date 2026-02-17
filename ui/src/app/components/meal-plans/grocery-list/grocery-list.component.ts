import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MealPlanService } from '../../../services/meal-plan.service';
import { GroceryItem, GroceryList } from '../../../models/meal-plan.model';

interface ManualGroceryItem {
  item: string;
}

interface CategoryGroup {
  category: string;
  items: DisplayItem[];
}

interface DisplayItem {
  key: string;
  item: string;
  quantity?: number;
  unit?: string;
  sources?: string[];
  isManual: boolean;
  originalIndex: number;
}

const CATEGORY_ORDER = [
  'Produce', 'Dairy', 'Meat & Seafood', 'Bakery', 'Frozen',
  'Beverages', 'Condiments & Spices', 'Pantry', 'Other'
];

@Component({
  selector: 'app-grocery-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './grocery-list.component.html',
  styleUrl: './grocery-list.component.scss'
})
export class GroceryListComponent implements OnInit {
  groceryList: GroceryList | null = null;
  loading = false;
  error = '';
  planId: string | null = null;

  checkedItems = new Set<string>();
  removedItems = new Set<number>();
  manualItems: ManualGroceryItem[] = [];
  groupedItems: CategoryGroup[] = [];
  newItemText = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private mealPlanService: MealPlanService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.planId = this.route.snapshot.paramMap.get('id');
    if (this.planId) {
      this.loadFromStorage();
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
        this.buildGroups();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load grocery list';
        this.loading = false;
      }
    });
  }

  buildGroups() {
    const groupMap = new Map<string, DisplayItem[]>();

    // Add API items (excluding removed)
    if (this.groceryList?.items) {
      this.groceryList.items.forEach((item, index) => {
        if (this.removedItems.has(index)) return;

        const category = item.category || 'Other';
        if (!groupMap.has(category)) {
          groupMap.set(category, []);
        }
        groupMap.get(category)!.push({
          key: `api:${index}`,
          item: item.item,
          quantity: item.quantity,
          unit: item.unit,
          sources: item.sources,
          isManual: false,
          originalIndex: index
        });
      });
    }

    // Add manual items
    this.manualItems.forEach((item, index) => {
      const category = 'Other';
      if (!groupMap.has(category)) {
        groupMap.set(category, []);
      }
      groupMap.get(category)!.push({
        key: `manual:${index}`,
        item: item.item,
        isManual: true,
        originalIndex: index
      });
    });

    // Sort into category order
    this.groupedItems = CATEGORY_ORDER
      .filter(cat => groupMap.has(cat))
      .map(cat => ({
        category: cat,
        items: groupMap.get(cat)!
      }));

    // Add any categories not in the predefined order
    groupMap.forEach((items, cat) => {
      if (!CATEGORY_ORDER.includes(cat)) {
        this.groupedItems.push({ category: cat, items });
      }
    });
  }

  toggleItem(key: string) {
    if (this.checkedItems.has(key)) {
      this.checkedItems.delete(key);
    } else {
      this.checkedItems.add(key);
    }
    this.saveCheckedItems();
  }

  isChecked(key: string): boolean {
    return this.checkedItems.has(key);
  }

  removeItem(displayItem: DisplayItem) {
    if (displayItem.isManual) {
      this.manualItems.splice(displayItem.originalIndex, 1);
      this.saveManualItems();
    } else {
      this.removedItems.add(displayItem.originalIndex);
      this.saveRemovedItems();
    }
    // Remove from checked if it was checked
    this.checkedItems.delete(displayItem.key);
    this.saveCheckedItems();
    this.buildGroups();
  }

  addManualItem() {
    const text = this.newItemText.trim();
    if (!text) return;

    this.manualItems.push({ item: text });
    this.newItemText = '';
    this.saveManualItems();
    this.buildGroups();
  }

  formatQuantity(item: DisplayItem): string {
    if (!item.quantity) return '';
    const qty = Math.round(item.quantity * 100) / 100;
    return `${qty}`;
  }

  getCategoryCheckedCount(group: CategoryGroup): number {
    return group.items.filter(i => this.checkedItems.has(i.key)).length;
  }

  get checkedCount(): number {
    let count = 0;
    for (const group of this.groupedItems) {
      count += group.items.filter(i => this.checkedItems.has(i.key)).length;
    }
    return count;
  }

  get totalCount(): number {
    let count = 0;
    for (const group of this.groupedItems) {
      count += group.items.length;
    }
    return count;
  }

  copyToClipboard() {
    const text = this.formatForClipboard();
    navigator.clipboard.writeText(text).then(() => {
      this.snackBar.open('Grocery list copied to clipboard', '', {
        duration: 2000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom'
      });
    });
  }

  private formatForClipboard(): string {
    const lines: string[] = [];
    for (const group of this.groupedItems) {
      const unchecked = group.items.filter(i => !this.checkedItems.has(i.key));
      if (unchecked.length === 0) continue;

      lines.push(`\n${group.category}`);
      lines.push('─'.repeat(group.category.length));
      for (const item of unchecked) {
        let line = '  ';
        if (item.quantity) {
          line += `${this.formatQuantity(item)} `;
        }
        if (item.unit) {
          line += `${item.unit} `;
        }
        line += item.item;
        lines.push(line);
      }
    }
    return lines.join('\n').trim();
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

  // localStorage persistence
  private storageKey(suffix: string): string {
    return `grocery-${suffix}:${this.planId}`;
  }

  private loadFromStorage() {
    try {
      const checked = localStorage.getItem(this.storageKey('checked'));
      if (checked) {
        this.checkedItems = new Set(JSON.parse(checked));
      }

      const manual = localStorage.getItem(this.storageKey('manual'));
      if (manual) {
        this.manualItems = JSON.parse(manual);
      }

      const removed = localStorage.getItem(this.storageKey('removed'));
      if (removed) {
        this.removedItems = new Set(JSON.parse(removed));
      }
    } catch {
      // Ignore corrupt storage
    }
  }

  private saveCheckedItems() {
    localStorage.setItem(this.storageKey('checked'), JSON.stringify([...this.checkedItems]));
  }

  private saveManualItems() {
    localStorage.setItem(this.storageKey('manual'), JSON.stringify(this.manualItems));
  }

  private saveRemovedItems() {
    localStorage.setItem(this.storageKey('removed'), JSON.stringify([...this.removedItems]));
  }
}
