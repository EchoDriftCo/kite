export interface DietaryProfile {
  dietaryProfileResourceId: string;
  subjectId: string;
  profileName: string;
  isDefault: boolean;
  restrictions: DietaryRestriction[];
  avoidedIngredients: AvoidedIngredient[];
  createdDate: Date;
  lastModifiedDate: Date;
}

export interface DietaryRestriction {
  restrictionType: string; // 'Allergy', 'Intolerance', 'DietaryChoice'
  restrictionCode: string;
  severity: string; // 'Strict', 'Flexible'
}

export interface AvoidedIngredient {
  avoidedIngredientId: number;
  ingredientName: string;
  reason?: string;
}

export interface UpdateDietaryProfile {
  profileName: string;
  isDefault: boolean;
}

export interface AddDietaryRestriction {
  restrictionCode: string;
  restrictionType: string;
  severity: string;
}

export interface AddAvoidedIngredient {
  ingredientName: string;
  reason?: string;
}

export interface DietaryConflictCheck {
  canEat: boolean;
  conflicts: DietaryConflict[];
}

export interface DietaryConflict {
  ingredientIndex: number;
  ingredientText: string;
  restrictionCode: string;
  restrictionType: string;
  severity: string;
  message: string;
}
