import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CollectionService } from '../../../services/collection.service';
import { Collection, CreateCollectionRequest, UpdateCollectionRequest } from '../../../models/collection.model';

export interface CollectionFormDialogData {
  mode: 'create' | 'edit';
  collection?: Collection;
}

@Component({
  selector: 'app-collection-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.mode === 'create' ? 'Create Collection' : 'Edit Collection' }}</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Collection Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g., Holiday Favorites" required />
          <mat-error *ngIf="form.get('name')?.hasError('required')">Name is required</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3" placeholder="Describe this collection..."></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Cover Image URL (optional)</mat-label>
          <input matInput formControlName="coverImageUrl" placeholder="https://..." />
        </mat-form-field>

        <mat-checkbox formControlName="isPublic">Make this collection public</mat-checkbox>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="onCancel()">Cancel</button>
        <button 
          mat-raised-button 
          color="primary" 
          type="submit" 
          [disabled]="form.invalid || saving">
          {{ saving ? 'Saving...' : (data.mode === 'create' ? 'Create' : 'Save') }}
        </button>
        <button 
          *ngIf="data.mode === 'edit'" 
          mat-button 
          color="warn" 
          type="button" 
          (click)="onDelete()"
          [disabled]="saving">
          Delete
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    mat-dialog-content {
      min-width: 400px;
    }
  `]
})
export class CollectionFormDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private collectionService: CollectionService,
    private dialogRef: MatDialogRef<CollectionFormDialogComponent>,
    private snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: CollectionFormDialogData
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      coverImageUrl: [''],
      isPublic: [false]
    });
  }

  ngOnInit() {
    if (this.data.mode === 'edit' && this.data.collection) {
      this.form.patchValue({
        name: this.data.collection.name,
        description: this.data.collection.description,
        coverImageUrl: this.data.collection.coverImageUrl,
        isPublic: this.data.collection.isPublic
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      return;
    }

    this.saving = true;
    const formValue = this.form.value;

    if (this.data.mode === 'create') {
      const request: CreateCollectionRequest = {
        name: formValue.name,
        description: formValue.description,
        coverImageUrl: formValue.coverImageUrl,
        isPublic: formValue.isPublic
      };

      this.collectionService.createCollection(request).subscribe({
        next: () => {
          this.snackBar.open('Collection created successfully', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error creating collection:', error);
          this.snackBar.open('Failed to create collection', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
    } else {
      const request: UpdateCollectionRequest = {
        name: formValue.name,
        description: formValue.description,
        coverImageUrl: formValue.coverImageUrl,
        isPublic: formValue.isPublic
      };

      this.collectionService.updateCollection(this.data.collection!.collectionResourceId, request).subscribe({
        next: () => {
          this.snackBar.open('Collection updated successfully', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error updating collection:', error);
          this.snackBar.open('Failed to update collection', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
    }
  }

  onDelete() {
    if (!confirm('Are you sure you want to delete this collection?')) {
      return;
    }

    this.saving = true;
    this.collectionService.deleteCollection(this.data.collection!.collectionResourceId).subscribe({
      next: () => {
        this.snackBar.open('Collection deleted successfully', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error deleting collection:', error);
        this.snackBar.open('Failed to delete collection', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
