import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CollectionService } from '../../../services/collection.service';
import { Collection } from '../../../models/collection.model';
import { CollectionFormDialogComponent } from '../collection-form-dialog/collection-form-dialog.component';

@Component({
  selector: 'app-collection-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  template: `
    <div class="collection-list-container">
      <div class="header">
        <h1>My Collections</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          New Collection
        </button>
      </div>

      <div class="collections-grid">
        <mat-card *ngFor="let collection of collections" class="collection-card" (click)="viewCollection(collection.collectionResourceId)">
          <mat-card-header>
            <mat-card-title>{{ collection.name }}</mat-card-title>
            <button mat-icon-button (click)="editCollection(collection, $event)" class="edit-btn">
              <mat-icon>edit</mat-icon>
            </button>
          </mat-card-header>
          <img *ngIf="collection.coverImageUrl" mat-card-image [src]="collection.coverImageUrl" [alt]="collection.name" />
          <mat-card-content>
            <p>{{ collection.description }}</p>
            <div class="collection-stats">
              <span><mat-icon>restaurant</mat-icon> {{ collection.recipeCount }} recipes</span>
              <span *ngIf="collection.isPublic"><mat-icon>public</mat-icon> Public</span>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="collection-card empty-state" *ngIf="collections.length === 0">
          <mat-card-content>
            <mat-icon>collections_bookmark</mat-icon>
            <p>No collections yet</p>
            <button mat-button color="primary" (click)="openCreateDialog()">Create your first collection</button>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .collection-list-container {
      padding: var(--spacing-lg);
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--spacing-lg);
    }

    .collections-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: var(--spacing-md);
    }

    .collection-card {
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
      position: relative;
    }

    .collection-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    }

    .collection-card mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .edit-btn {
      position: absolute;
      top: 8px;
      right: 8px;
    }

    .collection-stats {
      display: flex;
      gap: var(--spacing-md);
      margin-top: var(--spacing-sm);
      color: var(--text-secondary);
    }

    .collection-stats span {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .empty-state {
      grid-column: 1 / -1;
      text-align: center;
      padding: var(--spacing-xl) var(--spacing-lg);
      max-width: 400px;
      margin: 48px auto 0;
      cursor: default;
    }

    .empty-state:hover {
      transform: none;
      box-shadow: none;
    }

    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: var(--text-secondary);
      margin-bottom: 16px;
    }

    .empty-state p {
      font-size: 1.1rem;
      margin-bottom: 16px;
      color: var(--text-secondary);
    }
  `]
})
export class CollectionListComponent implements OnInit {
  collections: Collection[] = [];

  constructor(
    private collectionService: CollectionService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadCollections();
  }

  loadCollections() {
    this.collectionService.getMyCollections().subscribe({
      next: (collections) => {
        this.collections = collections;
      },
      error: (error) => {
        console.error('Error loading collections:', error);
        this.snackBar.open('Failed to load collections', 'Close', { duration: 3000 });
      }
    });
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(CollectionFormDialogComponent, {
      width: '500px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCollections();
      }
    });
  }

  editCollection(collection: Collection, event: Event) {
    event.stopPropagation();
    const dialogRef = this.dialog.open(CollectionFormDialogComponent, {
      width: '500px',
      data: { mode: 'edit', collection }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCollections();
      }
    });
  }

  viewCollection(collectionId: string) {
    this.router.navigate(['/collections', collectionId]);
  }
}
