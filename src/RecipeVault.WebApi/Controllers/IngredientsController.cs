using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Ingredient search and suggestion endpoints
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}")]
    [Authorize]
    public class IngredientsController : ControllerBase {
        private readonly IIngredientSearchFacade facade;

        /// <summary>
        /// Initializes a new instance of the IngredientsController
        /// </summary>
        public IngredientsController(IIngredientSearchFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Search recipes by available ingredients
        /// </summary>
        [HttpPost("recipes/search/by-ingredients")]
        [ProducesResponseType(typeof(PagedList<IngredientSearchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchByIngredientsAsync([FromBody] IngredientSearchRequestDto request) {
            if (request.Ingredients == null || request.Ingredients.Count == 0) {
                return BadRequest("At least one ingredient is required");
            }

            if (request.Ingredients.Count > 30) {
                return BadRequest("Maximum 30 ingredients allowed per search");
            }

            try {
                var results = await facade.SearchByIngredientsAsync(request).ConfigureAwait(false);
                return Ok(results);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get ingredient autocomplete suggestions
        /// </summary>
        [HttpGet("ingredients/suggest")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> SuggestIngredientsAsync(
            [FromQuery] string query,
            [FromQuery] int limit = 10) {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2) {
                return Ok(new { suggestions = new List<IngredientSuggestionDto>() });
            }

            try {
                var suggestions = await facade.SuggestAsync(query, Math.Min(limit, 25)).ConfigureAwait(false);
                return Ok(new { suggestions });
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
