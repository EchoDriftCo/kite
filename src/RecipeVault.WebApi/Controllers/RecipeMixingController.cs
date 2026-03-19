using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Domain.Enums;
using RecipeVault.Facade;
using RecipeVault.WebApi.Filters;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Recipe mixing (AI Fusion) operations
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/recipes/mix")]
    [Authorize]
    [RequiresTier(AccountTier.Premium)]
    public class RecipeMixingController : ControllerBase {
        private readonly IRecipeMixingFacade facade;
        private readonly ILogger<RecipeMixingController> logger;

        public RecipeMixingController(IRecipeMixingFacade facade, ILogger<RecipeMixingController> logger) {
            this.facade = facade;
            this.logger = logger;
        }

        /// <summary>
        /// Mix two recipes together using AI
        /// </summary>
        /// <param name="request">Mix recipes request with recipe IDs, intent, and mode</param>
        [HttpPost("")]
        [EnableRateLimiting("upload")]
        [ProducesResponseType(typeof(MixedRecipePreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> MixRecipesAsync([FromBody] MixRecipesRequestDto request) {
            try {
                logger.LogInformation("Mixing recipes: {RecipeA} + {RecipeB}, mode={Mode}",
                    request.RecipeAId, request.RecipeBId, request.Mode);

                var preview = await facade.MixRecipesAsync(request);
                return Ok(preview);
            } catch (ArgumentException ex) {
                logger.LogWarning(ex, "Invalid mix request");
                return BadRequest(new { error = ex.Message });
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to mix recipes");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Failed to mix recipes. Please try again." });
            }
        }

        /// <summary>
        /// Refine a mixed recipe preview based on user feedback
        /// </summary>
        /// <param name="request">Refine request with current preview and refinement notes</param>
        [HttpPost("refine")]
        [EnableRateLimiting("upload")]
        [ProducesResponseType(typeof(MixedRecipePreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> RefineMixAsync([FromBody] RefineMixRequestDto request) {
            try {
                logger.LogInformation("Refining mixed recipe: {Title}", request.Preview?.Title);

                var preview = await facade.RefineMixedRecipeAsync(request);
                return Ok(preview);
            } catch (ArgumentException ex) {
                logger.LogWarning(ex, "Invalid refine request");
                return BadRequest(new { error = ex.Message });
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to refine mixed recipe");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Failed to refine recipe. Please try again." });
            }
        }

        /// <summary>
        /// Save a mixed recipe preview as a real recipe
        /// </summary>
        /// <param name="preview">The mixed recipe preview to save</param>
        [HttpPost("save")]
        [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveMixedRecipeAsync([FromBody] MixedRecipePreviewDto preview) {
            try {
                logger.LogInformation("Saving mixed recipe: {Title}", preview.Title);

                var recipe = await facade.SaveMixedRecipeAsync(preview);
                return CreatedAtAction("GetRecipe", "Recipes", new { id = recipe.RecipeResourceId }, recipe);
            } catch (ArgumentException ex) {
                logger.LogWarning(ex, "Invalid save request");
                return BadRequest(new { error = ex.Message });
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to save mixed recipe");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to save recipe. Please try again." });
            }
        }
    }
}
