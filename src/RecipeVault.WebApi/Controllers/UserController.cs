using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// User onboarding and setup
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/user")]
    [Authorize]
    public class UserController : ControllerBase {
        private readonly IOnboardingFacade facade;
        private readonly OnboardingModelMapper mapper;
        private readonly ILogger<UserController> logger;

        /// <summary>
        /// Initializes a new instance of the UserController
        /// </summary>
        public UserController(IOnboardingFacade facade, OnboardingModelMapper mapper, ILogger<UserController> logger) {
            this.facade = facade;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Get current user's onboarding status
        /// </summary>
        [HttpGet("onboarding-status")]
        [ProducesResponseType(typeof(OnboardingStatusModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOnboardingStatusAsync() {
            try {
                var dto = await facade.GetOnboardingStatusAsync().ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            } catch (Exception ex) {
                logger.LogError(ex, "Error getting onboarding status");
                throw;
            }
        }

        /// <summary>
        /// Update onboarding progress (partial save per step)
        /// </summary>
        [HttpPatch("onboarding-progress")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateOnboardingProgressAsync([FromBody] UpdateOnboardingProgressModel progress) {
            try {
                await facade.UpdateOnboardingProgressAsync(mapper.MapToDto(progress)).ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                logger.LogError(ex, "Error updating onboarding progress");
                throw;
            }
        }

        /// <summary>
        /// Mark onboarding as completed
        /// </summary>
        [HttpPost("complete-onboarding")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CompleteOnboardingAsync() {
            try {
                await facade.CompleteOnboardingAsync().ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                logger.LogError(ex, "Error completing onboarding");
                throw;
            }
        }

        /// <summary>
        /// Reset onboarding (re-run from settings)
        /// </summary>
        [HttpPost("reset-onboarding")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ResetOnboardingAsync() {
            try {
                await facade.ResetOnboardingAsync().ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                logger.LogError(ex, "Error resetting onboarding");
                throw;
            }
        }

        /// <summary>
        /// Add sample recipes to user's library (fork from system account)
        /// </summary>
        [HttpPost("add-sample-recipes")]
        [ProducesResponseType(typeof(AddSampleRecipesResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddSampleRecipesAsync() {
            try {
                var result = await facade.AddSampleRecipesAsync().ConfigureAwait(false);
                return Ok(mapper.MapResult(result));
            } catch (Exception ex) {
                logger.LogError(ex, "Error adding sample recipes");
                throw;
            }
        }

        /// <summary>
        /// Remove all sample recipes from user's library
        /// </summary>
        [HttpDelete("sample-recipes")]
        [ProducesResponseType(typeof(RemoveSampleRecipesResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveSampleRecipesAsync() {
            try {
                var result = await facade.RemoveSampleRecipesAsync().ConfigureAwait(false);
                return Ok(mapper.MapRemoveResult(result));
            } catch (Exception ex) {
                logger.LogError(ex, "Error removing sample recipes");
                throw;
            }
        }
    }
}
