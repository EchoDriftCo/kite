import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CircleService } from '../../../services/circle.service';
import { Circle } from '../../../models/circle.model';

export interface CircleFormDialogData {
  isEdit: boolean;
  circle?: Circle;
}

@Component({
  selector: 'app-circle-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './circle-form-dialog.component.html',
  styleUrl: './circle-form-dialog.component.scss'
})
export class CircleFormDialogComponent implements OnInit {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private circleService: CircleService,
    public dialogRef: MatDialogRef<CircleFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CircleFormDialogData
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit() {
    if (this.data.isEdit && this.data.circle) {
      this.form.patchValue({
        name: this.data.circle.name,
        description: this.data.circle.description || ''
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    const formData = this.form.value;

    if (this.data.isEdit && this.data.circle) {
      // Update existing circle
      this.circleService.updateCircle(this.data.circle.circleResourceId, formData).subscribe({
        next: (circle) => {
          this.loading = false;
          this.dialogRef.close(circle);
        },
        error: (err) => {
          this.error = err.message || 'Failed to update circle';
          this.loading = false;
          console.error('Error updating circle:', err);
        }
      });
    } else {
      // Create new circle
      this.circleService.createCircle(formData).subscribe({
        next: (circle) => {
          this.loading = false;
          this.dialogRef.close(circle);
        },
        error: (err) => {
          this.error = err.message || 'Failed to create circle';
          this.loading = false;
          console.error('Error creating circle:', err);
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
