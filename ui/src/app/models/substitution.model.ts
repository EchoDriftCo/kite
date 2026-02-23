import { ParsedIngredient } from './recipe.model';

/**
 * Request to get substitution suggestions
 */
export interface SubstitutionRequest {
  ingredientIndices?: number[];
  dietaryConstraints?: string[];
}

/**
 * A single substitution option for an ingredient
 */
export interface SubstitutionOption {
  name: string;
  ingredients: ParsedIngredient[];
  notes: string;
  techniqueAdjustments?: string;
}

/**
 * Substitutions for a single ingredient
 */
export interface IngredientSubstitution {
  originalIndex: number;
  originalText: string;
  reason?: string;
  options: SubstitutionOption[];
}

/**
 * Response from substitution suggestions endpoint
 */
export interface SubstitutionResponse {
  substitutions: IngredientSubstitution[];
  cached?: boolean;
}

/**
 * User's selection of which substitution to apply
 */
export interface SubstitutionSelection {
  ingredientIndex: number;
  optionIndex: number; // Index into the options array, or -1 for "none of these"
}

/**
 * Request to apply substitutions and fork the recipe
 */
export interface ApplySubstitutionsRequest {
  selections: SubstitutionSelection[];
  forkTitle?: string;
}

/**
 * Available dietary constraints
 */
export const DIETARY_CONSTRAINTS = [
  'Gluten-Free',
  'Dairy-Free',
  'Vegan',
  'Vegetarian',
  'Nut-Free',
  'Low-Sodium',
  'Keto'
] as const;

export type DietaryConstraint = typeof DIETARY_CONSTRAINTS[number];
