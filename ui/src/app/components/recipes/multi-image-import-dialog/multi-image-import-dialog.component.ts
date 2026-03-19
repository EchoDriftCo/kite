import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RecipeService } from '../../../services/recipe.service';

@Component({
  selector: 'app-multi-image-import-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>Import Recipe from Multiple Images</h2>
    <mat-dialog-content>
      <div class="upload-area" 
           (drop)="onDrop($event)" 
           (dragover)="onDragOver($event)"
           (click)="fileInput.click()">
        <input #fileInput
               type="file"
               accept="image/*,image/heic,image/heif,.heic,.heif"
               multiple 
               (change)="onFileSelected($event)"
               style="display: none"
               [disabled]="loading">
        
        <div *ngIf="selectedFiles.length === 0" class="upload-prompt">
          <mat-icon>cloud_upload</mat-icon>
          <p>Drop images here or click to select (1-4 images)</p>
        </div>

        <div *ngIf="selectedFiles.length > 0" class="image-preview-grid">
          <div *ngFor="let file of selectedFiles; let i = index" class="image-preview">
            <img [src]="file.preview" [alt]="'Preview ' + (i + 1)">
            <button mat-icon-button 
                    class="remove-btn" 
                    (click)="removeFile(i); $event.stopPropagation()"
                    [disabled]="loading">
              <mat-icon>close</mat-icon>
            </button>
            <div class="image-number">{{ i + 1 }}</div>
          </div>
        </div>
      </div>

      <div *ngIf="loading" class="loading-overlay">
        <mat-spinner></mat-spinner>
        <p>Processing images with AI...</p>
      </div>

      <div *ngIf="error" class="error-message">
        {{ error }}
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()" [disabled]="loading">Cancel</button>
      <button mat-raised-button 
              color="primary" 
              (click)="import()"
              [disabled]="selectedFiles.length === 0 || loading">
        Import Recipe
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .upload-area {
      min-height: 300px;
      border: 2px dashed var(--border-color);
      border-radius: 8px;
      padding: 20px;
      text-align: center;
      cursor: pointer;
      position: relative;
    }

    .upload-area:hover {
      background-color: var(--hover-bg);
    }

    .upload-prompt {
      padding: 60px 20px;
    }

    .upload-prompt mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: var(--text-secondary);
    }

    .image-preview-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: 16px;
    }

    .image-preview {
      position: relative;
      aspect-ratio: 1;
      border-radius: 8px;
      overflow: hidden;
    }

    .image-preview img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .remove-btn {
      position: absolute;
      top: 4px;
      right: 4px;
      background-color: rgba(0, 0, 0, 0.6);
    }

    .image-number {
      position: absolute;
      bottom: 4px;
      left: 4px;
      background-color: var(--primary-color);
      color: white;
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: bold;
    }

    .loading-overlay {
      text-align: center;
      padding: 20px;
    }

    .error-message {
      color: var(--error-color);
      padding: 16px;
      margin-top: 16px;
      border-radius: 4px;
      background-color: var(--error-bg);
    }
  `]
})
export class MultiImageImportDialogComponent {
  selectedFiles: Array<{ file: File; preview: string }> = [];
  loading = false;
  error: string | null = null;

  constructor(
    private dialogRef: MatDialogRef<MultiImageImportDialogComponent>,
    private recipeService: RecipeService
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.addFiles(Array.from(input.files));
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer?.files) {
      this.addFiles(Array.from(event.dataTransfer.files));
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  private static readonly HEIC_EXTENSIONS = ['.heic', '.heif'];

  private isImageFile(file: File): boolean {
    if (file.type.startsWith('image/')) return true;
    const ext = file.name.toLowerCase();
    return MultiImageImportDialogComponent.HEIC_EXTENSIONS.some(e => ext.endsWith(e));
  }

  addFiles(files: File[]): void {
    const imageFiles = files.filter(f => this.isImageFile(f));
    const availableSlots = 4 - this.selectedFiles.length;
    const filesToAdd = imageFiles.slice(0, availableSlots);

    filesToAdd.forEach(file => {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.selectedFiles.push({
          file,
          preview: e.target?.result as string
        });
      };
      reader.readAsDataURL(file);
    });

    if (imageFiles.length > availableSlots) {
      this.error = `Only ${availableSlots} more image(s) can be added (max 4 total)`;
      setTimeout(() => this.error = null, 3000);
    }
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  async import(): Promise<void> {
    if (this.selectedFiles.length === 0) return;

    this.loading = true;
    this.error = null;

    try {
      const formData = new FormData();
      this.selectedFiles.forEach((item, index) => {
        formData.append('images', item.file);
      });
      formData.append('processingMode', 'sequential');

      const recipe = await this.recipeService.importFromMultipleImages(formData).toPromise();
      this.dialogRef.close(recipe);
    } catch (err: any) {
      this.error = err.error?.message || 'Failed to import recipe from images';
      this.loading = false;
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
