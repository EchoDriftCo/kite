import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ImageCropperComponent, ImageCroppedEvent, LoadedImage } from 'ngx-image-cropper';
import { RecipeService } from '../../../services/recipe.service';

export interface ImportResult {
  success: boolean;
  parsedData?: any;
  confidence?: number;
  warnings?: string[];
  error?: string;
  imageData?: string;
  imageMimeType?: string;
  sourceUrl?: string;
  paprikaResult?: any;
}

@Component({
  selector: 'app-recipe-import-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    ImageCropperComponent
  ],
  templateUrl: './recipe-import-dialog.component.html',
  styleUrl: './recipe-import-dialog.component.scss'
})
export class RecipeImportDialogComponent {
  loading = false;
  error = '';
  activeTab: 'image' | 'url' | 'paprika' = 'image';

  // Image tab state
  selectedFile: File | null = null;
  previewUrl: string | null = null;
  dragOver = false;
  croppedImageBase64: string | null = null;
  rotation: number = 0;
  cropperReady = false;
  saveImage = true;

  // URL tab state
  recipeUrl = '';

  // Paprika tab state
  paprikaFile: File | null = null;
  paprikaDragOver = false;

  constructor(
    private dialogRef: MatDialogRef<RecipeImportDialogComponent>,
    private recipeService: RecipeService
  ) {}

  onTabChange(index: number) {
    if (index === 0) {
      this.activeTab = 'image';
    } else if (index === 1) {
      this.activeTab = 'url';
    } else {
      this.activeTab = 'paprika';
    }
    this.error = '';
  }

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
    this.croppedImageBase64 = null;
    this.rotation = 0;
    this.cropperReady = false;
  }

  rotateLeft() {
    this.rotation = (this.rotation - 1 + 4) % 4;
  }

  rotateRight() {
    this.rotation = (this.rotation + 1) % 4;
  }

  onImageCropped(event: ImageCroppedEvent) {
    this.croppedImageBase64 = event.base64 || null;
  }

  onImageLoaded(image: LoadedImage) {
    // Image loaded successfully
  }

  onCropperReady() {
    this.cropperReady = true;
  }

  onLoadImageFailed() {
    this.error = 'Failed to load image. Please try a different file.';
  }

  get isImportDisabled(): boolean {
    if (this.loading) return true;
    if (this.activeTab === 'image') return !this.selectedFile;
    if (this.activeTab === 'url') return !this.recipeUrl.trim();
    return !this.paprikaFile;
  }

  async importRecipe() {
    if (this.activeTab === 'url') {
      await this.importFromUrl();
    } else if (this.activeTab === 'paprika') {
      await this.importFromPaprika();
    } else {
      await this.importFromImage();
    }
  }

  private async importFromImage() {
    if (!this.selectedFile) {
      this.error = 'Please select an image';
      return;
    }

    this.loading = true;
    this.error = '';

    try {
      let base64Data: string;
      let mimeType: string;

      // Use cropped image if available, otherwise use full file
      if (this.croppedImageBase64) {
        // Cropped image is already base64 from the cropper
        base64Data = this.croppedImageBase64.includes(',')
          ? this.croppedImageBase64.split(',')[1]
          : this.croppedImageBase64;
        mimeType = 'image/png'; // Cropper outputs PNG
      } else {
        // No crop applied, use original file
        const base64 = await this.fileToBase64(this.selectedFile);
        base64Data = base64.includes(',') ? base64.split(',')[1] : base64;
        mimeType = this.selectedFile.type;
      }

      // Call parse API
      const response = await this.recipeService.parseRecipe({
        imageData: base64Data,
        mimeType: mimeType,
        imageUrl: undefined,
        recipeText: undefined
      }).toPromise();

      if (!response || !response.recipe) {
        throw new Error('Failed to parse recipe from image');
      }

      // Close dialog with parsed data (and image if save toggle is on)
      const result: ImportResult = {
        success: true,
        parsedData: response.recipe,
        confidence: response.confidence,
        warnings: response.warnings
      };

      if (this.saveImage) {
        result.imageData = base64Data;
        result.imageMimeType = mimeType;
      }

      this.dialogRef.close(result);

    } catch (err: any) {
      this.error = err.message || 'Failed to parse recipe. Please try again.';
      this.loading = false;
      console.error('Recipe import error:', err);
    }
  }

  private async importFromUrl() {
    const url = this.recipeUrl.trim();
    if (!url) {
      this.error = 'Please enter a URL';
      return;
    }

    this.loading = true;
    this.error = '';

    try {
      const response = await this.recipeService.parseRecipe({
        imageUrl: url
      }).toPromise();

      if (!response || !response.recipe) {
        throw new Error('Failed to extract recipe from URL');
      }

      const result: ImportResult = {
        success: true,
        parsedData: response.recipe,
        confidence: response.confidence,
        warnings: response.warnings,
        sourceUrl: url
      };

      this.dialogRef.close(result);

    } catch (err: any) {
      this.error = err.message || 'Failed to extract recipe from URL. Please try again.';
      this.loading = false;
      console.error('Recipe URL import error:', err);
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

  // Paprika import methods
  onPaprikaFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.handlePaprikaFile(input.files[0]);
    }
  }

  onPaprikaFileDrop(event: DragEvent) {
    event.preventDefault();
    this.paprikaDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files[0]) {
      this.handlePaprikaFile(files[0]);
    }
  }

  onPaprikaDragOver(event: DragEvent) {
    event.preventDefault();
    this.paprikaDragOver = true;
  }

  onPaprikaDragLeave(event: DragEvent) {
    event.preventDefault();
    this.paprikaDragOver = false;
  }

  handlePaprikaFile(file: File) {
    // Validate file extension
    if (!file.name.toLowerCase().endsWith('.paprikarecipes')) {
      this.error = 'Please select a .paprikarecipes file';
      return;
    }

    // Validate file size (max 50MB - Paprika files can be large with images)
    if (file.size > 50 * 1024 * 1024) {
      this.error = 'File must be less than 50MB';
      return;
    }

    this.paprikaFile = file;
    this.error = '';
  }

  clearPaprikaFile() {
    this.paprikaFile = null;
    this.error = '';
  }

  private async importFromPaprika() {
    if (!this.paprikaFile) {
      this.error = 'Please select a Paprika file';
      return;
    }

    this.loading = true;
    this.error = '';

    try {
      const result = await this.recipeService.importFromPaprika(this.paprikaFile).toPromise();

      if (!result) {
        throw new Error('Failed to import recipes from Paprika file');
      }

      // Close dialog with Paprika import result
      const importResult: ImportResult = {
        success: true,
        paprikaResult: result
      };

      this.dialogRef.close(importResult);

    } catch (err: any) {
      this.error = err.error?.message || err.message || 'Failed to import from Paprika file. Please try again.';
      this.loading = false;
      console.error('Paprika import error:', err);
    }
  }
}
