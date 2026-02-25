import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DietaryProfileService } from '../../services/dietary-profile.service';
import { DietaryConflictCheck } from '../../models/dietary-profile.model';

@Component({
  selector: 'app-dietary-warning',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dietary-warning.component.html',
  styleUrls: ['./dietary-warning.component.scss']
})
export class DietaryWarningComponent implements OnChanges {
  @Input() recipeId!: string;
  @Input() profileId?: string;

  conflictCheck?: DietaryConflictCheck;
  loading = false;
  error?: string;

  constructor(private dietaryProfileService: DietaryProfileService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['recipeId'] && this.recipeId) {
      this.checkRecipe();
    }
  }

  checkRecipe(): void {
    this.loading = true;
    this.error = undefined;

    this.dietaryProfileService.checkRecipe(this.recipeId, this.profileId).subscribe({
      next: (result) => {
        this.conflictCheck = result;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Unable to check dietary restrictions';
        this.loading = false;
        console.error('Error checking recipe:', err);
      }
    });
  }

  getSeverityClass(severity: string): string {
    return severity.toLowerCase() === 'strict' ? 'severity-strict' : 'severity-flexible';
  }

  getTypeClass(type: string): string {
    return `type-${type.toLowerCase().replace(/\s+/g, '-')}`;
  }
}
