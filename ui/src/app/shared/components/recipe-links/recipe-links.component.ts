import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LinkedRecipe, UsedInRecipe } from '../../../models/recipe-link.model';
import { RecipeLinkDialogComponent } from '../recipe-link-dialog/recipe-link-dialog.component';
import { RecipeLinkService } from '../../../services/recipe-link.service';

@Component({
  selector: 'app-recipe-links',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatTooltipModule
  ],
  templateUrl: './recipe-links.component.html',
  styleUrl: './recipe-links.component.scss'
})
export class RecipeLinksComponent {
  @Input() recipeId!: string;
  @Input() linkedRecipes: LinkedRecipe[] = [];
  @Input() usedInRecipes: UsedInRecipe[] = [];
  @Input() canEdit = false;
  @Output() refreshRequested = new EventEmitter<void>();

  expandedLinkIds = new Set<string>();

  constructor(
    private dialog: MatDialog,
    private recipeLinkService: RecipeLinkService
  ) {}

  openAddLinkDialog() {
    const dialogRef = this.dialog.open(RecipeLinkDialogComponent, {
      width: '500px',
      data: {
        parentRecipeId: this.recipeId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.refreshRequested.emit();
      }
    });
  }

  openEditLinkDialog(link: LinkedRecipe) {
    const dialogRef = this.dialog.open(RecipeLinkDialogComponent, {
      width: '500px',
      data: {
        parentRecipeId: this.recipeId,
        existingLink: link
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.refreshRequested.emit();
      }
    });
  }

  deleteLink(link: LinkedRecipe) {
    if (confirm(`Are you sure you want to remove the link to "${link.title}"?`)) {
      this.recipeLinkService.deleteRecipeLink(this.recipeId, link.recipeLinkResourceId).subscribe({
        next: () => {
          this.refreshRequested.emit();
        },
        error: (err) => {
          alert('Error deleting link: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }

  toggleExpanded(link: LinkedRecipe) {
    if (this.expandedLinkIds.has(link.recipeLinkResourceId)) {
      this.expandedLinkIds.delete(link.recipeLinkResourceId);
    } else {
      this.expandedLinkIds.add(link.recipeLinkResourceId);
    }
  }

  isExpanded(link: LinkedRecipe): boolean {
    return this.expandedLinkIds.has(link.recipeLinkResourceId);
  }

  hasExpandableDetails(link: LinkedRecipe): boolean {
    return (link.ingredients?.length ?? 0) > 0 || (link.instructions?.length ?? 0) > 0;
  }

  formatTime(minutes?: number): string {
    if (!minutes) return 'N/A';
    if (minutes < 60) return `${minutes}min`;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}min` : `${hours}h`;
  }
}
