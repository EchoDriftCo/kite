import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { EquipmentService } from '../../services/equipment.service';
import { Equipment, UserEquipment } from '../../models/equipment.model';
import { forkJoin } from 'rxjs';

interface EquipmentWithSelection extends Equipment {
  selected: boolean;
}

interface EquipmentByCategory {
  Appliance: EquipmentWithSelection[];
  Cookware: EquipmentWithSelection[];
  Bakeware: EquipmentWithSelection[];
  Tool: EquipmentWithSelection[];
}

@Component({
  selector: 'app-equipment-setup',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './equipment-setup.component.html',
  styleUrls: ['./equipment-setup.component.scss']
})
export class EquipmentSetupComponent implements OnInit {
  loading = true;
  saving = false;
  
  equipmentByCategory: EquipmentByCategory = {
    Appliance: [],
    Cookware: [],
    Bakeware: [],
    Tool: []
  };
  
  categories = ['Appliance', 'Cookware', 'Bakeware', 'Tool'] as const;
  
  categoryIcons = {
    Appliance: 'kitchen',
    Cookware: 'soup_kitchen',
    Bakeware: 'cake',
    Tool: 'construction'
  };

  constructor(
    private equipmentService: EquipmentService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadEquipment();
  }

  loadEquipment(): void {
    this.loading = true;
    
    forkJoin({
      allEquipment: this.equipmentService.getAllEquipment(),
      myEquipment: this.equipmentService.getMyEquipment()
    }).subscribe({
      next: (result) => {
        const myEquipmentCodes = new Set(
          result.myEquipment.map(ue => ue.equipmentCode)
        );
        
        // Group equipment by category and mark selected ones
        result.allEquipment.forEach(equipment => {
          const equipmentWithSelection: EquipmentWithSelection = {
            ...equipment,
            selected: myEquipmentCodes.has(equipment.code)
          };
          this.equipmentByCategory[equipment.category].push(equipmentWithSelection);
        });
        
        // Sort by name within each category
        this.categories.forEach(category => {
          this.equipmentByCategory[category].sort((a, b) => 
            a.name.localeCompare(b.name)
          );
        });
        
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading equipment:', error);
        this.snackBar.open('Failed to load equipment', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  saveEquipment(): void {
    this.saving = true;
    
    // Collect all selected equipment codes
    const selectedCodes = new Set<string>();
    this.categories.forEach(category => {
      this.equipmentByCategory[category]
        .filter(e => e.selected)
        .forEach(e => selectedCodes.add(e.code));
    });
    
    // Get current equipment to determine what to add/remove
    this.equipmentService.getMyEquipment().subscribe({
      next: (currentEquipment) => {
        const currentCodes = new Set(currentEquipment.map(ue => ue.equipmentCode));
        
        // Determine additions and removals
        const toAdd = Array.from(selectedCodes).filter(code => !currentCodes.has(code));
        const toRemove = Array.from(currentCodes).filter(code => !selectedCodes.has(code));
        
        // Build array of observables
        const operations = [
          ...toAdd.map(code => this.equipmentService.addEquipment({ equipmentCode: code })),
          ...toRemove.map(code => this.equipmentService.removeEquipment(code))
        ];
        
        if (operations.length === 0) {
          this.snackBar.open('No changes to save', 'Close', { duration: 2000 });
          this.saving = false;
          return;
        }
        
        forkJoin(operations).subscribe({
          next: () => {
            this.snackBar.open(
              `Equipment updated! ${toAdd.length} added, ${toRemove.length} removed.`,
              'Close',
              { duration: 3000 }
            );
            this.saving = false;
          },
          error: (error) => {
            console.error('Error saving equipment:', error);
            this.snackBar.open('Failed to save equipment', 'Close', { duration: 3000 });
            this.saving = false;
          }
        });
      },
      error: (error) => {
        console.error('Error getting current equipment:', error);
        this.snackBar.open('Failed to save equipment', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  getSelectedCount(category: 'Appliance' | 'Cookware' | 'Bakeware' | 'Tool'): number {
    return this.equipmentByCategory[category].filter(e => e.selected).length;
  }

  getTotalSelectedCount(): number {
    return this.categories.reduce((sum, category) => 
      sum + this.getSelectedCount(category), 0
    );
  }
}
