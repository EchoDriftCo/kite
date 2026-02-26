export interface Equipment {
  equipmentId: number;
  name: string;
  code: string;
  category: 'Appliance' | 'Cookware' | 'Bakeware' | 'Tool';
  description: string;
  isCommon: boolean;
}

export interface UserEquipment {
  equipmentCode: string;
  equipment: Equipment;
}

export interface RecipeEquipment {
  equipmentCode: string;
  equipment: Equipment;
}

export interface AddEquipmentRequest {
  equipmentCode: string;
}

export interface EquipmentCheckResult {
  recipeResourceId: string;
  requiredEquipment: RecipeEquipment[];
  hasAllEquipment: boolean;
  missingEquipment: Equipment[];
  unlockedRecipeCount?: number;
}
