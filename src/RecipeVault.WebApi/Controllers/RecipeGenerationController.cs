using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Domain.Enums;
using RecipeVault.Facade;
using RecipeVault.WebApi.Filters;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// AI-powered recipe generation endpoints
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/recipes/generate")]
    [Authorize]
    [RequiresTier(AccountTier.Premium)]
    public class RecipeGenerationController : ControllerBase {
        private readonly IRecipeGenerationFacade facade;

        /// <summary>
        /// Initializes a new instance of the RecipeGenerationController
        /// </summary>
        public RecipeGenerationController(IRecipeGenerationFacade facade) {
            this.facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Generate new recipes based on natural language prompt
        /// </summary>
        /// <param name="request">Generation request with prompt and constraints</param>
        /// <returns>Generated recipe(s) for preview</returns>
        [HttpPost("")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(List<GeneratedRecipeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GenerateRecipesAsync([FromBody] GenerateRecipeRequestDto request) {
            if (request == null) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid request",
                    Detail = "Request body is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (string.IsNullOrWhiteSpace(request.Prompt)) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid request",
                    Detail = "Prompt is required for recipe generation",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try {
                var recipes = await facade.GenerateRecipesAsync(request).ConfigureAwait(false);
                return Ok(recipes);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Daily generation limit")) {
                return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails {
                    Title = "Rate limit exceeded",
                    Detail = ex.Message,
                    Status = StatusCodes.Status429TooManyRequests
                });
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails {
                    Title = "Recipe generation failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Refine a previously generated recipe
        /// </summary>
        /// <param name="request">Refinement request with previous recipe and refinement instructions</param>
        /// <returns>Refined recipe</returns>
        [HttpPost("refine")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(GeneratedRecipeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RefineRecipeAsync([FromBody] RefineRecipeRequestDto request) {
            if (request?.PreviousRecipe == null) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid request",
                    Detail = "Previous recipe is required for refinement",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (string.IsNullOrWhiteSpace(request.Refinement)) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid request",
                    Detail = "Refinement instructions are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try {
                var refinedRecipe = await facade.RefineRecipeAsync(request).ConfigureAwait(false);
                return Ok(refinedRecipe);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Daily generation limit")) {
                return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails {
                    Title = "Rate limit exceeded",
                    Detail = ex.Message,
                    Status = StatusCodes.Status429TooManyRequests
                });
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails {
                    Title = "Recipe refinement failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Save a generated recipe to user's library
        /// </summary>
        /// <param name="generatedRecipe">The generated recipe to save</param>
        /// <returns>Saved recipe</returns>
        [HttpPost("save")]
        [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveGeneratedRecipeAsync([FromBody] GeneratedRecipeDto generatedRecipe) {
            if (generatedRecipe == null) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid request",
                    Detail = "Recipe data is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try {
                var savedRecipe = await facade.SaveGeneratedRecipeAsync(generatedRecipe).ConfigureAwait(false);
                
                using (LogContext.PushProperty("RecipeResourceId", savedRecipe.RecipeResourceId)) {
                    return CreatedAtAction("GetRecipeAsync", "Recipes", new { id = savedRecipe.RecipeResourceId }, savedRecipe);
                }
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails {
                    Title = "Failed to save recipe",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Get remaining recipe generation quota for today
        /// </summary>
        /// <returns>Number of remaining generations</returns>
        [HttpGet("quota")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRemainingGenerationsAsync() {
            var remaining = await facade.GetRemainingGenerationsAsync().ConfigureAwait(false);
            return Ok(new { remaining });
        }
    }
}
