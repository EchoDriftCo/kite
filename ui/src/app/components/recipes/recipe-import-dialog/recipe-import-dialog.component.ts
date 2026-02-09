import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { RecipeService } from '../../../services/recipe.service';

export interface ImportResult {
  success: boolean;
  parsedData?: any;
  confidence?: number;
  warnings?: string[];
  error?: string;
}

@Component({
  selector: 'app-recipe-import-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule
  ],
  templateUrl: './recipe-import-dialog.component.html',
  styleUrl: './recipe-import-dialog.component.scss'
})
export class RecipeImportDialogComponent {
  loading = false;
  error = '';
  selectedFile: File | null = null;
  previewUrl: string | null = null;
  dragOver = false;

  constructor(
    private dialogRef: MatDialogRef<RecipeImportDialogComponent>,
    private recipeService: RecipeService
  ) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.handleFile(input.files[0]);
    }
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.dragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files[0]) {
      this.handleFile(files[0]);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.dragOver = false;
  }

  handleFile(file: File) {
    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.error = 'Please select an image file';
      return;
    }

    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      this.error = 'Image must be less than 10MB';
      return;
    }

    this.selectedFile = file;
    this.error = '';

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.previewUrl = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  clearFile() {
    this.selectedFile = null;
    this.previewUrl = null;
    this.error = '';
  }

  async importRecipe() {
    if (!this.selectedFile) {
      this.error = 'Please select an image';
      return;
    }

    this.loading = true;
    this.error = '';

    try {
      // Convert image to base64
      const base64 = await this.fileToBase64(this.selectedFile);
      
      // Remove data URL prefix if present
      const base64Data = base64.includes(',') ? base64.split(',')[1] : base64;

      // Call parse API
      const response = await this.recipeService.parseRecipe({
        imageData: base64Data,
        imageUrl: undefined,
        recipeText: undefined
      }).toPromise();

      if (!response || !response.recipe) {
        throw new Error('Failed to parse recipe from image');
      }

      // Close dialog with parsed data
      this.dialogRef.close({
        success: true,
        parsedData: response.recipe,
        confidence: response.confidence,
        warnings: response.warnings
      } as ImportResult);

    } catch (err: any) {
      this.error = err.message || 'Failed to parse recipe. Please try again.';
      this.loading = false;
      console.error('Recipe import error:', err);
    }
  }

  cancel() {
    this.dialogRef.close({ success: false });
  }

  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = error => reject(error);
      reader.readAsDataURL(file);
    });
  }
}
