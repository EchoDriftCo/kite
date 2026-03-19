import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BreakpointObserver } from '@angular/cdk/layout';
import { OnboardingService } from '../../../services/onboarding.service';
import { TourService } from '../../../services/tour.service';
import { DietaryProfileService } from '../../../services/dietary-profile.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-onboarding-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './onboarding-dialog.component.html',
  styleUrl: './onboarding-dialog.component.scss'
})
export class OnboardingDialogComponent implements OnInit {
  currentStep = 0;
  totalSteps = 4;

  // Step 2: Dietary
  allergies = [
    { code: 'peanuts', label: 'Peanuts' },
    { code: 'tree-nuts', label: 'Tree Nuts' },
    { code: 'shellfish', label: 'Shellfish' },
    { code: 'fish', label: 'Fish' },
    { code: 'eggs', label: 'Eggs' },
    { code: 'dairy', label: 'Dairy' },
    { code: 'wheat', label: 'Wheat' },
    { code: 'soy', label: 'Soy' },
    { code: 'sesame', label: 'Sesame' }
  ];

  dietaryChoices = [
    { code: 'vegetarian', label: 'Vegetarian' },
    { code: 'vegan', label: 'Vegan' },
    { code: 'keto', label: 'Keto' },
    { code: 'kosher', label: 'Kosher' },
    { code: 'halal', label: 'Halal' },
    { code: 'paleo', label: 'Paleo' }
  ];

  selectedRestrictions: string[] = [];
  avoidedIngredients: string[] = [];
  avoidedInput = '';
  savingDietary = false;

  // Step 3: Recipes
  samplesAdded = false;
  addingSamples = false;

  isMobile = false;

  // Swipe tracking
  private touchStartX = 0;
  private touchStartY = 0;
  private swipeDebounce = false;

  constructor(
    public dialogRef: MatDialogRef<OnboardingDialogComponent>,
    private onboardingService: OnboardingService,
    private tourService: TourService,
    private dietaryProfileService: DietaryProfileService,
    private snackBar: MatSnackBar,
    private breakpointObserver: BreakpointObserver,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe(result => {
        this.isMobile = result.matches;
        if (this.isMobile) {
          this.dialogRef.updateSize('100vw', '100vh');
        } else {
          this.dialogRef.updateSize('600px', 'auto');
        }
      });
  }

  get progressPercent(): number {
    return ((this.currentStep + 1) / this.totalSteps) * 100;
  }

  nextStep(): void {
    if (this.currentStep < this.totalSteps - 1) {
      this.currentStep++;
    } else {
      this.finish();
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  skip(): void {
    this.onboardingService.completeOnboarding().subscribe();
    this.dialogRef.close({ completed: true, startTour: false });
  }

  // Step 2: Dietary
  toggleRestriction(code: string): void {
    const idx = this.selectedRestrictions.indexOf(code);
    if (idx >= 0) {
      this.selectedRestrictions.splice(idx, 1);
    } else {
      this.selectedRestrictions.push(code);
    }
  }

  isRestrictionSelected(code: string): boolean {
    return this.selectedRestrictions.includes(code);
  }

  addAvoidedIngredient(): void {
    const trimmed = this.avoidedInput.trim();
    if (trimmed && !this.avoidedIngredients.includes(trimmed)) {
      this.avoidedIngredients.push(trimmed);
    }
    this.avoidedInput = '';
  }

  removeAvoidedIngredient(name: string): void {
    this.avoidedIngredients = this.avoidedIngredients.filter(i => i !== name);
  }

  saveDietaryPreferences(): void {
    if (this.selectedRestrictions.length === 0 && this.avoidedIngredients.length === 0) {
      this.nextStep();
      return;
    }

    this.savingDietary = true;

    const allergyList = ['peanuts', 'tree-nuts', 'shellfish', 'fish', 'eggs', 'dairy', 'wheat', 'soy', 'sesame'];

    this.dietaryProfileService.createProfile({
      profileName: 'My Preferences',
      isDefault: true
    }).subscribe({
      next: (profile) => {
        // Add restrictions one at a time
        const addOps = this.selectedRestrictions.map(code => {
          const restrictionType = allergyList.includes(code) ? 'Allergy' : 'DietaryChoice';
          return this.dietaryProfileService.addRestriction(
            profile.dietaryProfileResourceId, {
              restrictionCode: code,
              restrictionType: restrictionType,
              severity: 'Strict'
            }
          );
        });

        const avoidOps = this.avoidedIngredients.map(name =>
          this.dietaryProfileService.addAvoidedIngredient(
            profile.dietaryProfileResourceId, {
              ingredientName: name,
              reason: 'Set during onboarding'
            }
          )
        );

        // Execute all sequentially-ish (fire and forget the individual adds)
        let completed = 0;
        const total = addOps.length + avoidOps.length;

        if (total === 0) {
          this.onboardingService.updateProgress({ dietaryProfileSet: true }).subscribe();
          this.savingDietary = false;
          this.nextStep();
          return;
        }

        const checkDone = () => {
          completed++;
          if (completed >= total) {
            this.onboardingService.updateProgress({ dietaryProfileSet: true }).subscribe();
            this.savingDietary = false;
            this.nextStep();
          }
        };

        addOps.forEach(op => op.subscribe({ next: checkDone, error: checkDone }));
        avoidOps.forEach(op => op.subscribe({ next: checkDone, error: checkDone }));
      },
      error: () => {
        this.savingDietary = false;
        this.snackBar.open('Failed to save dietary preferences', 'OK', { duration: 3000 });
      }
    });
  }

  // Step 3: Recipes
  addSampleRecipes(): void {
    if (this.samplesAdded || this.addingSamples) return;

    this.addingSamples = true;
    this.onboardingService.addSampleRecipes().subscribe({
      next: result => {
        this.samplesAdded = true;
        this.addingSamples = false;
        if (result.recipes?.length) {
          this.tourService.setSampleRecipeIds(result.recipes);
        }
        this.snackBar.open(
          `${result.recipesAdded} sample recipes added to your library!`,
          'OK',
          { duration: 3000 }
        );
        this.onboardingService.updateProgress({ samplesAdded: true }).subscribe();
      },
      error: () => {
        this.addingSamples = false;
        this.snackBar.open('Failed to add sample recipes', 'OK', { duration: 3000 });
      }
    });
  }

  openImportDialog(): void {
    this.dialogRef.close({ completed: false, startTour: false });
    this.router.navigate(['/recipes/new']);
  }

  // Step 4: Tour launch
  finish(): void {
    this.onboardingService.completeOnboarding().subscribe();
    this.dialogRef.close({ completed: true, startTour: true });
  }

  // Swipe gesture handling (mobile only)
  @HostListener('touchstart', ['$event'])
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.touches[0].clientX;
    this.touchStartY = event.touches[0].clientY;
  }

  @HostListener('touchend', ['$event'])
  onTouchEnd(event: TouchEvent): void {
    if (this.swipeDebounce) return;

    const touchEndX = event.changedTouches[0].clientX;
    const touchEndY = event.changedTouches[0].clientY;
    const deltaX = touchEndX - this.touchStartX;
    const deltaY = touchEndY - this.touchStartY;

    // Only handle horizontal swipes (ignore vertical scrolling)
    if (Math.abs(deltaX) > 50 && Math.abs(deltaX) > Math.abs(deltaY)) {
      this.swipeDebounce = true;
      setTimeout(() => this.swipeDebounce = false, 300);

      if (deltaX < 0) {
        // Swipe left = next
        this.nextStep();
      } else {
        // Swipe right = back
        this.prevStep();
      }
    }
  }
}
