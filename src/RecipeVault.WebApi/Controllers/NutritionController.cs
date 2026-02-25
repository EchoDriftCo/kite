using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Nutrition calculation and food search endpoints
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/nutrition")]
    [Authorize]
    public class NutritionController : ControllerBase {
        private readonly INutritionFacade facade;

        public NutritionController(INutritionFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Calculate nutrition for a recipe
        /// </summary>
        [HttpPost("recipes/{recipeId}/calculate")]
        [ProducesResponseType(typeof(RecipeNutritionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CalculateRecipeNutritionAsync(Guid recipeId) {
            if (!facade.IsAvailable()) {
                return StatusCode(503, new { message = "Nutrition service is not available" });
            }

            var nutrition = await facade.CalculateRecipeNutritionAsync(recipeId);
            return Ok(nutrition);
        }

        /// <summary>
        /// Get nutrition data for a recipe
        /// </summary>
        [HttpGet("recipes/{recipeId}")]
        [ProducesResponseType(typeof(RecipeNutritionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecipeNutritionAsync(Guid recipeId) {
            var nutrition = await facade.GetRecipeNutritionAsync(recipeId);
            return Ok(nutrition);
        }

        /// <summary>
        /// Search USDA foods
        /// </summary>
        [HttpGet("foods/search")]
        [ProducesResponseType(typeof(List<FoodSearchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SearchFoodsAsync([FromQuery] string query) {
            if (!facade.IsAvailable()) {
                return StatusCode(503, new { message = "Nutrition service is not available" });
            }

            if (string.IsNullOrWhiteSpace(query)) {
                return BadRequest(new { message = "Query parameter is required" });
            }

            var results = await facade.SearchFoodsAsync(query);
            return Ok(results);
        }

        /// <summary>
        /// Check if nutrition service is available
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStatus() {
            return Ok(new { available = facade.IsAvailable() });
        }
    }
}
