import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CircleService } from '../../../services/circle.service';
import { Circle, CircleMember, CircleRecipe, CircleRole } from '../../../models/circle.model';
import { InviteDialogComponent } from '../invite-dialog/invite-dialog.component';
import { CircleFormDialogComponent } from '../circle-form-dialog/circle-form-dialog.component';

@Component({
  selector: 'app-circle-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatMenuModule,
    MatSnackBarModule
  ],
  templateUrl: './circle-detail.component.html',
  styleUrl: './circle-detail.component.scss'
})
export class CircleDetailComponent implements OnInit {
  circle: Circle | null = null;
  members: CircleMember[] = [];
  recipes: CircleRecipe[] = [];
  loading = false;
  error = '';
  circleId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private circleService: CircleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.circleId = this.route.snapshot.paramMap.get('id');
    
    if (this.circleId) {
      this.loadCircle();
      this.loadMembers();
      this.loadRecipes();
    }
  }

  loadCircle() {
    if (!this.circleId) return;

    this.loading = true;
    this.error = '';

    this.circleService.getCircle(this.circleId).subscribe({
      next: (circle) => {
        this.circle = circle;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load circle';
        this.loading = false;
        console.error('Error loading circle:', err);
      }
    });
  }

  loadMembers() {
    if (!this.circleId) return;

    this.circleService.getCircleMembers(this.circleId).subscribe({
      next: (response) => {
        this.members = response.items || [];
      },
      error: (err) => {
        console.error('Error loading members:', err);
      }
    });
  }

  loadRecipes() {
    if (!this.circleId) return;

    this.circleService.getCircleRecipes(this.circleId).subscribe({
      next: (response) => {
        this.recipes = response.items || [];
      },
      error: (err) => {
        console.error('Error loading recipes:', err);
      }
    });
  }

  goBack() {
    this.router.navigate(['/circles']);
  }

  openInviteDialog() {
    if (!this.circle) return;

    const dialogRef = this.dialog.open(InviteDialogComponent, {
      width: '500px',
      data: { circle: this.circle }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadMembers();
      }
    });
  }

  openEditDialog() {
    if (!this.circle) return;

    const dialogRef = this.dialog.open(CircleFormDialogComponent, {
      width: '500px',
      data: { isEdit: true, circle: this.circle }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCircle();
        this.snackBar.open('Circle updated successfully!', 'Close', { duration: 3000 });
      }
    });
  }

  deleteCircle() {
    if (!this.circle || !this.circleId) return;

    if (confirm(`Are you sure you want to delete "${this.circle.name}"? This cannot be undone.`)) {
      this.circleService.deleteCircle(this.circleId).subscribe({
        next: () => {
          this.snackBar.open('Circle deleted successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/circles']);
        },
        error: (err) => {
          this.snackBar.open('Failed to delete circle', 'Close', { duration: 3000 });
          console.error('Error deleting circle:', err);
        }
      });
    }
  }

  leaveCircle() {
    if (!this.circle || !this.circleId) return;

    if (confirm(`Are you sure you want to leave "${this.circle.name}"?`)) {
      // Get current user's member record
      const currentMember = this.members.find(m => !m.userEmail); // Simplified - in real app, check against current user
      if (currentMember) {
        this.circleService.removeMember(this.circleId, currentMember.subjectId).subscribe({
          next: () => {
            this.snackBar.open('Left circle successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/circles']);
          },
          error: (err) => {
            this.snackBar.open('Failed to leave circle', 'Close', { duration: 3000 });
            console.error('Error leaving circle:', err);
          }
        });
      }
    }
  }

  removeMember(member: CircleMember) {
    if (!this.circleId) return;

    if (confirm(`Remove ${member.userEmail || 'this member'} from the circle?`)) {
      this.circleService.removeMember(this.circleId, member.subjectId).subscribe({
        next: () => {
          this.snackBar.open('Member removed successfully', 'Close', { duration: 3000 });
          this.loadMembers();
        },
        error: (err) => {
          this.snackBar.open('Failed to remove member', 'Close', { duration: 3000 });
          console.error('Error removing member:', err);
        }
      });
    }
  }

  viewRecipe(recipe: CircleRecipe) {
    this.router.navigate(['/recipes', recipe.recipeId]);
  }

  getRoleColor(role: CircleRole): string {
    switch (role) {
      case CircleRole.Owner: return 'primary';
      case CircleRole.Admin: return 'accent';
      default: return '';
    }
  }

  canManageMembers(): boolean {
    return this.circle?.isOwner || false;
  }

  canEdit(): boolean {
    return this.circle?.isOwner || false;
  }
}
