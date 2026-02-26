/**
 * Request to generate new recipes from AI
 */
export interface GenerateRecipeRequest {
  prompt: string;
  maxTime?: number;
  dietary?: string[];
  skillLevel?: string;
  variations?: number;
}

/**
 * Request to refine a generated recipe
 */
export interface RefineRecipeRequest {
  previousRecipe: GeneratedRecipe;
  refinement: string;
}

/**
 * Generated recipe preview (before saving)
 */
export interface GeneratedRecipe {
  title: string;
  description: string;
  yield: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  ingredients: GeneratedIngredient[];
  instructions: GeneratedInstruction[];
  tags: string[];
}

/**
 * Generated ingredient
 */
export interface GeneratedIngredient {
  quantity?: number;
  unit?: string;
  item: string;
  preparation?: string;
}

/**
 * Generated instruction
 */
export interface GeneratedInstruction {
  stepNumber: number;
  instruction: string;
}

/**
 * Generation quota response
 */
export interface GenerationQuota {
  remaining: number;
}
