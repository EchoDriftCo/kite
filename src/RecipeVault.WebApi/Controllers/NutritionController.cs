using System;
using System.Net.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Domain.Enums;
using RecipeVault.Facade;
using RecipeVault.WebApi.Filters;
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
    [RequiresTier(AccountTier.Premium)]
    public class NutritionController : ControllerBase {
        private readonly INutritionFacade _facade;
        private readonly ILogger<NutritionController> _logger;

        public NutritionController(INutritionFacade facade, ILogger<NutritionController> logger) {
            _facade = facade;
            _logger = logger;
        }

        /// <summary>
        /// Analyze nutrition for a recipe
        /// </summary>
        /// <param name="id">Recipe Resource ID</param>
        [HttpPost("recipes/{id}/nutrition/analyze")]
        [ProducesResponseType(typeof(RecipeNutritionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> AnalyzeRecipeNutritionAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                try {
                    var dto = await _facade.AnalyzeRecipeNutritionAsync(id).ConfigureAwait(false);
                    return Ok(dto);
                } catch (HttpRequestException ex) {
                    _logger.LogError(ex, "USDA API unavailable while analyzing nutrition for recipe {RecipeId}", id);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Nutrition service temporarily unavailable");
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error analyzing nutrition for recipe {RecipeId}", id);
                    throw;
                }
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
                try {
                    var dto = await _facade.GetRecipeNutritionAsync(id).ConfigureAwait(false);
                    if (dto == null) {
                        return NotFound();
                    }
                    return Ok(dto);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error getting nutrition for recipe {RecipeId}", id);
                    throw;
                }
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
                    try {
                        var dto = await _facade.UpdateIngredientNutritionAsync(id, ingredientIndex, input).ConfigureAwait(false);
                        return Ok(dto);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Error updating nutrition for ingredient {IngredientIndex} in recipe {RecipeId}", ingredientIndex, id);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Search USDA FoodData Central
        /// </summary>
        /// <param name="query">Search query</param>
        [HttpGet("nutrition/search")]
        [ProducesResponseType(typeof(FoodSearchDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

            try {
                var results = await _facade.SearchFoodsAsync(query).ConfigureAwait(false);
                return Ok(results);
            } catch (HttpRequestException ex) {
                _logger.LogError(ex, "USDA API unavailable while searching for foods: {Query}", query);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Nutrition service temporarily unavailable");
            } catch (Exception ex) {
                _logger.LogError(ex, "Error searching foods for query: {Query}", query);
                throw;
            }
        }
    }
}
