using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;

namespace RecipeVault.Facade {
    public class NutritionFacade : INutritionFacade {
        private readonly IUnitOfWork uow;
        private readonly INutritionService nutritionService;
        private readonly IRecipeRepository recipeRepository;
        private readonly ILogger<NutritionFacade> logger;

        public NutritionFacade(
            IUnitOfWork uow,
            INutritionService nutritionService,
            IRecipeRepository recipeRepository,
            ILogger<NutritionFacade> logger) {
            this.uow = uow;
            this.nutritionService = nutritionService;
            this.recipeRepository = recipeRepository;
            this.logger = logger;
        }

        public bool IsAvailable() {
            return nutritionService.IsAvailable();
        }

        public async Task<RecipeNutritionDto> CalculateRecipeNutritionAsync(Guid recipeResourceId) {
            var recipe = await recipeRepository.GetAsync(recipeResourceId);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe {recipeResourceId} not found");
            }

            var nutrition = await nutritionService.CalculateRecipeNutritionAsync(recipe);
            if (nutrition != null) {
                // TODO: Save nutrition to database
                await uow.SaveChangesAsync();
            }

            return MapToDto(nutrition);
        }

        public async Task<RecipeNutritionDto> GetRecipeNutritionAsync(Guid recipeResourceId) {
            // TODO: Query from database
            // For now, calculate on the fly
            return await CalculateRecipeNutritionAsync(recipeResourceId);
        }

        public async Task<List<FoodSearchDto>> SearchFoodsAsync(string query) {
            var results = await nutritionService.SearchFoodsForIngredientAsync(query);
            return results.Select(r => new FoodSearchDto {
                FdcId = r.FdcId,
                Description = r.Description,
                DataType = r.DataType,
                BrandOwner = r.BrandOwner,
                Nutrients = r.FoodNutrients?.Select(n => new NutrientDto {
                    NutrientId = n.NutrientId,
                    Name = n.NutrientName,
                    Number = n.NutrientNumber,
                    Unit = n.UnitName,
                    Value = n.Value
                }).ToList()
            }).ToList();
        }

        private RecipeNutritionDto MapToDto(RecipeNutrition nutrition) {
            if (nutrition == null) {
                return null;
            }

            return new RecipeNutritionDto {
                RecipeNutritionId = nutrition.RecipeNutritionId,
                RecipeId = nutrition.RecipeId,
                CaloriesPerServing = nutrition.CaloriesPerServing,
                ProteinPerServing = nutrition.ProteinPerServing,
                CarbsPerServing = nutrition.CarbsPerServing,
                FatPerServing = nutrition.FatPerServing,
                FiberPerServing = nutrition.FiberPerServing,
                SugarPerServing = nutrition.SugarPerServing,
                SodiumPerServing = nutrition.SodiumPerServing,
                IngredientsMatched = nutrition.IngredientsMatched,
                IngredientsTotal = nutrition.IngredientsTotal,
                CoveragePercent = nutrition.CoveragePercent,
                CalculatedDate = nutrition.CalculatedDate,
                IsStale = nutrition.IsStale
            };
        }
    }
}
