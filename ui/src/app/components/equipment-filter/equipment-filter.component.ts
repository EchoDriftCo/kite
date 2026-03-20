import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-equipment-filter',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './equipment-filter.component.html',
  styleUrls: ['./equipment-filter.component.scss']
})
export class EquipmentFilterComponent {
  @Output() filterChange = new EventEmitter<boolean>();
  
  filterEnabled = false;

  toggleFilter(): void {
    this.filterEnabled = !this.filterEnabled;
    this.filterChange.emit(this.filterEnabled);
  }
}
