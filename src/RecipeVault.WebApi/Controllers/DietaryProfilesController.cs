using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Represents the shared functionality/resources of the dietary profile resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/dietary-profiles")]
    [Authorize]
    public class DietaryProfilesController : ControllerBase {
        private readonly IDietaryProfileFacade facade;
        private readonly DietaryProfileModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the DietaryProfilesController
        /// </summary>
        public DietaryProfilesController(IDietaryProfileFacade facade, DietaryProfileModelMapper mapper) {
            this.facade = facade;
            this.mapper = mapper;
        }

        /// <summary>
        /// Gets dietary profiles for the current user
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<DietaryProfileModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDietaryProfilesAsync() {
            var results = await facade.GetProfilesBySubjectAsync().ConfigureAwait(false);
            var models = results.Select(x => mapper.Map(x)).ToList();
            return Ok(models);
        }

        /// <summary>
        /// Gets a dietary profile by id
        /// </summary>
        /// <param name="id">the resource id of the dietary profile to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetDietaryProfileAsync))]
        [ProducesResponseType(typeof(DietaryProfileModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDietaryProfileAsync(Guid id) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                var dto = await facade.GetProfileAsync(id).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            }
        }

        /// <summary>
        /// Create a new dietary profile
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(DietaryProfileModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateDietaryProfileAsync([FromBody] UpdateDietaryProfileModel input) {
            var inputDto = mapper.MapToDto(input);
            var dto = await facade.CreateProfileAsync(inputDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetDietaryProfileAsync), new { id = dto.DietaryProfileResourceId }, mapper.Map(dto));
        }

        /// <summary>
        /// Update a dietary profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DietaryProfileModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDietaryProfileAsync(Guid id, [FromBody] UpdateDietaryProfileModel input) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                var inputDto = mapper.MapToDto(input);
                var dto = await facade.UpdateProfileAsync(id, inputDto).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            }
        }

        /// <summary>
        /// Delete a dietary profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteDietaryProfileAsync(Guid id) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                await facade.DeleteProfileAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Add a dietary restriction to a profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile</param>
        /// <param name="input"></param>
        [HttpPost("{id}/restrictions")]
        [ProducesResponseType(typeof(DietaryProfileModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddRestrictionAsync(Guid id, [FromBody] AddDietaryRestrictionModel input) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                var inputDto = mapper.MapToDto(input);
                var dto = await facade.AddRestrictionAsync(id, inputDto).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            }
        }

        /// <summary>
        /// Remove a dietary restriction from a profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile</param>
        /// <param name="restrictionCode">the code of the restriction to remove</param>
        [HttpDelete("{id}/restrictions/{restrictionCode}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveRestrictionAsync(Guid id, string restrictionCode) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                await facade.RemoveRestrictionAsync(id, restrictionCode).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Add an avoided ingredient to a profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile</param>
        /// <param name="input"></param>
        [HttpPost("{id}/avoided-ingredients")]
        [ProducesResponseType(typeof(DietaryProfileModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddAvoidedIngredientAsync(Guid id, [FromBody] AddAvoidedIngredientModel input) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                var inputDto = mapper.MapToDto(input);
                var dto = await facade.AddAvoidedIngredientAsync(id, inputDto).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            }
        }

        /// <summary>
        /// Remove an avoided ingredient from a profile
        /// </summary>
        /// <param name="id">the resource id of the dietary profile</param>
        /// <param name="avoidedIngredientId">the id of the avoided ingredient to remove</param>
        [HttpDelete("{id}/avoided-ingredients/{avoidedIngredientId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAvoidedIngredientAsync(Guid id, int avoidedIngredientId) {
            using (LogContext.PushProperty("DietaryProfileResourceId", id)) {
                await facade.RemoveAvoidedIngredientAsync(id, avoidedIngredientId).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Check a recipe for dietary conflicts
        /// </summary>
        /// <param name="recipeId">the resource id of the recipe to check</param>
        /// <param name="profileId">optional resource id of the dietary profile (uses default if not specified)</param>
        [HttpGet("~/api/v{version:apiVersion}/recipes/{recipeId}/dietary-check")]
        [ProducesResponseType(typeof(DietaryConflictCheckModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRecipeAsync(Guid recipeId, [FromQuery] Guid? profileId = null) {
            using (LogContext.PushProperty("RecipeResourceId", recipeId)) {
                var dto = await facade.CheckRecipeAsync(recipeId, profileId).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            }
        }
    }
}
