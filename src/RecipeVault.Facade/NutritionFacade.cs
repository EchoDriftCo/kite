using System;
using System.Linq;
using System.Threading.Tasks;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public class NutritionFacade : INutritionFacade {
        private readonly INutritionService _nutritionService;
        private readonly IRecipeService _recipeService;

        public NutritionFacade(INutritionService nutritionService, IRecipeService recipeService) {
            _nutritionService = nutritionService;
            _recipeService = recipeService;
        }

        public async Task<RecipeNutritionDto> AnalyzeRecipeNutritionAsync(Guid recipeResourceId) {
            // Analysis replaces the stored nutrition (including manual overrides), so owner only
            var recipe = await _recipeService.GetOwnRecipeAsync(recipeResourceId);

            var recipeNutrition = await _nutritionService.AnalyzeRecipeNutritionAsync(recipe.RecipeId);
            return MapToDto(recipeNutrition);
        }

        public async Task<RecipeNutritionDto> GetRecipeNutritionAsync(Guid recipeResourceId) {
            // Readable by whoever can read the recipe: the owner, or anyone if it's public
            var recipe = await _recipeService.GetRecipeAsync(recipeResourceId);

            var recipeNutrition = await _nutritionService.GetRecipeNutritionAsync(recipe.RecipeId);
            return recipeNutrition != null ? MapToDto(recipeNutrition) : null;
        }

        public async Task<IngredientNutritionDto> UpdateIngredientNutritionAsync(Guid recipeResourceId, int ingredientIndex, UpdateIngredientNutritionDto dto) {
            var recipe = await _recipeService.GetOwnRecipeAsync(recipeResourceId);

            if (ingredientIndex < 0 || ingredientIndex >= recipe.Ingredients.Count) {
                throw new InvalidOperationException("Recipe or ingredient not found");
            }
            
            var ingredient = recipe.Ingredients.OrderBy(i => i.SortOrder).Skip(ingredientIndex).First();
            
            var ingredientNutrition = await _nutritionService.UpdateIngredientNutritionAsync(
                ingredient.RecipeIngredientId,
                dto.FdcId,
                dto.MatchedFoodName,
                dto.MatchConfidence
            );
            return MapToDto(ingredientNutrition);
        }

        public async Task<FoodSearchDto[]> SearchFoodsAsync(string query) {
            var searchResponse = await _nutritionService.SearchFoodsAsync(query);
            return searchResponse.Foods.Select(f => new FoodSearchDto {
                FdcId = f.FdcId,
                Description = f.Description,
                DataType = f.DataType,
                BrandOwner = f.BrandOwner,
                Nutrients = f.FoodNutrients?.Select(n => new NutrientDto {
                    NutrientId = n.NutrientId,
                    Name = n.NutrientName,
                    Number = n.NutrientNumber,
                    Unit = n.UnitName,
                    Value = n.Value
                }).ToList()
            }).ToArray();
        }

        private RecipeNutritionDto MapToDto(Domain.Entities.RecipeNutrition entity) {
            if (entity == null) return null;

            return new RecipeNutritionDto {
                RecipeNutritionId = entity.RecipeNutritionId,
                RecipeId = entity.RecipeId,
                CaloriesPerServing = entity.CaloriesPerServing,
                ProteinPerServing = entity.ProteinPerServing,
                CarbsPerServing = entity.CarbsPerServing,
                FatPerServing = entity.FatPerServing,
                FiberPerServing = entity.FiberPerServing,
                SugarPerServing = entity.SugarPerServing,
                SodiumPerServing = entity.SodiumPerServing,
                IngredientsMatched = entity.IngredientsMatched,
                IngredientsTotal = entity.IngredientsTotal,
                CoveragePercent = entity.CoveragePercent,
                CalculatedDate = entity.CalculatedDate,
                IsStale = entity.IsStale
            };
        }

        private IngredientNutritionDto MapToDto(Domain.Entities.IngredientNutrition entity) {
            if (entity == null) return null;

            return new IngredientNutritionDto {
                IngredientNutritionId = entity.IngredientNutritionId,
                RecipeIngredientId = entity.RecipeIngredientId,
                FdcId = entity.FdcId,
                MatchedFoodName = entity.MatchedFoodName,
                MatchConfidence = entity.MatchConfidence,
                Calories = entity.Calories,
                ProteinGrams = entity.ProteinGrams,
                CarbsGrams = entity.CarbsGrams,
                FatGrams = entity.FatGrams,
                FiberGrams = entity.FiberGrams,
                SugarGrams = entity.SugarGrams,
                SodiumMg = entity.SodiumMg,
                GramsUsed = entity.GramsUsed,
                CalculatedDate = entity.CalculatedDate,
                IsManualOverride = entity.IsManualOverride
            };
        }
    }
}
