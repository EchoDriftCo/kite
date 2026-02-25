import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DietaryProfileService } from '../../services/dietary-profile.service';
import {
  DietaryProfile,
  UpdateDietaryProfile,
  AddDietaryRestriction,
  AddAvoidedIngredient
} from '../../models/dietary-profile.model';

@Component({
  selector: 'app-dietary-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dietary-profile.component.html',
  styleUrls: ['./dietary-profile.component.scss']
})
export class DietaryProfileComponent implements OnInit {
  profiles: DietaryProfile[] = [];
  selectedProfile?: DietaryProfile;
  isCreating = false;
  isEditing = false;

  // Form models
  newProfile: UpdateDietaryProfile = { profileName: '', isDefault: false };
  newRestriction: AddDietaryRestriction = { restrictionCode: '', restrictionType: 'Allergy', severity: 'Strict' };
  newAvoidedIngredient: AddAvoidedIngredient = { ingredientName: '', reason: '' };

  // Predefined restriction codes
  commonRestrictions = [
    { code: 'peanuts', type: 'Allergy', label: 'Peanuts' },
    { code: 'tree-nuts', type: 'Allergy', label: 'Tree Nuts' },
    { code: 'shellfish', type: 'Allergy', label: 'Shellfish' },
    { code: 'fish', type: 'Allergy', label: 'Fish' },
    { code: 'eggs', type: 'Allergy', label: 'Eggs' },
    { code: 'dairy', type: 'Allergy', label: 'Dairy' },
    { code: 'wheat', type: 'Allergy', label: 'Wheat' },
    { code: 'gluten', type: 'Intolerance', label: 'Gluten' },
    { code: 'soy', type: 'Allergy', label: 'Soy' },
    { code: 'lactose', type: 'Intolerance', label: 'Lactose' },
    { code: 'vegetarian', type: 'DietaryChoice', label: 'Vegetarian' },
    { code: 'vegan', type: 'DietaryChoice', label: 'Vegan' },
    { code: 'pescatarian', type: 'DietaryChoice', label: 'Pescatarian' },
    { code: 'keto', type: 'DietaryChoice', label: 'Keto' },
    { code: 'paleo', type: 'DietaryChoice', label: 'Paleo' },
    { code: 'halal', type: 'DietaryChoice', label: 'Halal' },
    { code: 'kosher', type: 'DietaryChoice', label: 'Kosher' }
  ];

  constructor(private dietaryProfileService: DietaryProfileService) {}

  ngOnInit(): void {
    this.loadProfiles();
  }

  loadProfiles(): void {
    this.dietaryProfileService.getProfiles().subscribe({
      next: (profiles) => {
        this.profiles = profiles;
        if (profiles.length > 0 && !this.selectedProfile) {
          this.selectedProfile = profiles.find(p => p.isDefault) || profiles[0];
        }
      },
      error: (error) => console.error('Error loading profiles:', error)
    });
  }

  selectProfile(profile: DietaryProfile): void {
    this.selectedProfile = profile;
    this.isCreating = false;
    this.isEditing = false;
  }

  startCreateProfile(): void {
    this.isCreating = true;
    this.isEditing = false;
    this.selectedProfile = undefined;
    this.newProfile = { profileName: '', isDefault: false };
  }

  createProfile(): void {
    if (!this.newProfile.profileName.trim()) return;

    this.dietaryProfileService.createProfile(this.newProfile).subscribe({
      next: (profile) => {
        this.profiles.push(profile);
        this.selectedProfile = profile;
        this.isCreating = false;
        this.newProfile = { profileName: '', isDefault: false };
      },
      error: (error) => console.error('Error creating profile:', error)
    });
  }

  cancelCreate(): void {
    this.isCreating = false;
    this.newProfile = { profileName: '', isDefault: false };
  }

  deleteProfile(profile: DietaryProfile): void {
    if (!confirm(`Delete profile "${profile.profileName}"?`)) return;

    this.dietaryProfileService.deleteProfile(profile.dietaryProfileResourceId).subscribe({
      next: () => {
        this.profiles = this.profiles.filter(p => p.dietaryProfileResourceId !== profile.dietaryProfileResourceId);
        if (this.selectedProfile?.dietaryProfileResourceId === profile.dietaryProfileResourceId) {
          this.selectedProfile = this.profiles[0];
        }
      },
      error: (error) => console.error('Error deleting profile:', error)
    });
  }

  addRestriction(): void {
    if (!this.selectedProfile || !this.newRestriction.restrictionCode.trim()) return;

    this.dietaryProfileService.addRestriction(
      this.selectedProfile.dietaryProfileResourceId,
      this.newRestriction
    ).subscribe({
      next: (updatedProfile) => {
        this.updateProfileInList(updatedProfile);
        this.newRestriction = { restrictionCode: '', restrictionType: 'Allergy', severity: 'Strict' };
      },
      error: (error) => console.error('Error adding restriction:', error)
    });
  }

  removeRestriction(restrictionCode: string): void {
    if (!this.selectedProfile) return;

    this.dietaryProfileService.removeRestriction(
      this.selectedProfile.dietaryProfileResourceId,
      restrictionCode
    ).subscribe({
      next: () => {
        if (this.selectedProfile) {
          this.selectedProfile.restrictions = this.selectedProfile.restrictions.filter(
            r => r.restrictionCode !== restrictionCode
          );
        }
      },
      error: (error) => console.error('Error removing restriction:', error)
    });
  }

  addAvoidedIngredient(): void {
    if (!this.selectedProfile || !this.newAvoidedIngredient.ingredientName.trim()) return;

    this.dietaryProfileService.addAvoidedIngredient(
      this.selectedProfile.dietaryProfileResourceId,
      this.newAvoidedIngredient
    ).subscribe({
      next: (updatedProfile) => {
        this.updateProfileInList(updatedProfile);
        this.newAvoidedIngredient = { ingredientName: '', reason: '' };
      },
      error: (error) => console.error('Error adding avoided ingredient:', error)
    });
  }

  removeAvoidedIngredient(ingredientId: number): void {
    if (!this.selectedProfile) return;

    this.dietaryProfileService.removeAvoidedIngredient(
      this.selectedProfile.dietaryProfileResourceId,
      ingredientId
    ).subscribe({
      next: () => {
        if (this.selectedProfile) {
          this.selectedProfile.avoidedIngredients = this.selectedProfile.avoidedIngredients.filter(
            ai => ai.avoidedIngredientId !== ingredientId
          );
        }
      },
      error: (error) => console.error('Error removing avoided ingredient:', error)
    });
  }

  setCommonRestriction(restriction: { code: string; type: string }): void {
    this.newRestriction.restrictionCode = restriction.code;
    this.newRestriction.restrictionType = restriction.type;
  }

  private updateProfileInList(updatedProfile: DietaryProfile): void {
    const index = this.profiles.findIndex(p => p.dietaryProfileResourceId === updatedProfile.dietaryProfileResourceId);
    if (index !== -1) {
      this.profiles[index] = updatedProfile;
      this.selectedProfile = updatedProfile;
    }
  }
}
