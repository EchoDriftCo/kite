using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Nutrition analysis and USDA food search
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}")]
    [Authorize]
    public class NutritionController : ControllerBase {
        private readonly INutritionFacade _facade;

        public NutritionController(INutritionFacade facade) {
            _facade = facade;
        }

        /// <summary>
        /// Analyze nutrition for a recipe
        /// </summary>
        /// <param name="id">Recipe Resource ID</param>
        [HttpPost("recipes/{id}/nutrition/analyze")]
        [ProducesResponseType(typeof(RecipeNutritionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AnalyzeRecipeNutritionAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await _facade.AnalyzeRecipeNutritionAsync(id).ConfigureAwait(false);
                return Ok(dto);
            }
        }

        /// <summary>
        /// Get cached nutrition for a recipe
        /// </summary>
        /// <param name="id">Recipe Resource ID</param>
        [HttpGet("recipes/{id}/nutrition")]
        [ProducesResponseType(typeof(RecipeNutritionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecipeNutritionAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await _facade.GetRecipeNutritionAsync(id).ConfigureAwait(false);
                if (dto == null) {
                    return NotFound();
                }
                return Ok(dto);
            }
        }

        /// <summary>
        /// Update ingredient nutrition (manual override)
        /// </summary>
        /// <param name="id">Recipe Resource ID</param>
        /// <param name="ingredientIndex">Ingredient index</param>
        /// <param name="input">Updated nutrition data</param>
        [HttpPut("recipes/{id}/ingredients/{ingredientIndex}/nutrition")]
        [ProducesResponseType(typeof(IngredientNutritionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateIngredientNutritionAsync(Guid id, int ingredientIndex, [FromBody] UpdateIngredientNutritionDto input) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                using (LogContext.PushProperty("IngredientIndex", ingredientIndex)) {
                    var dto = await _facade.UpdateIngredientNutritionAsync(id, ingredientIndex, input).ConfigureAwait(false);
                    return Ok(dto);
                }
            }
        }

        /// <summary>
        /// Search USDA FoodData Central
        /// </summary>
        /// <param name="query">Search query</param>
        [HttpGet("nutrition/search")]
        [ProducesResponseType(typeof(FoodSearchDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchFoodsAsync([FromQuery] string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                return BadRequest("Query parameter is required");
            }

            if (query.Length < 2) {
                return BadRequest("Query must be at least 2 characters");
            }

            if (query.Length > 200) {
                return BadRequest("Query must not exceed 200 characters");
            }

            var results = await _facade.SearchFoodsAsync(query).ConfigureAwait(false);
            return Ok(results);
        }
    }
}
