using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// User pantry management endpoints
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/pantry")]
    [Authorize]
    public class PantryController : ControllerBase {
        private readonly IUserPantryFacade facade;

        /// <summary>
        /// Initializes a new instance of the PantryController
        /// </summary>
        public PantryController(IUserPantryFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Gets the current user's pantry items
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPantryAsync() {
            try {
                var items = await facade.GetUserPantryAsync().ConfigureAwait(false);
                return Ok(new { items });
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Adds an item to the user's pantry
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(UserPantryItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddPantryItemAsync([FromBody] CreatePantryItemDto dto) {
            try {
                var created = await facade.AddPantryItemAsync(dto).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetUserPantryAsync), null, created);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a pantry item
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserPantryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePantryItemAsync(int id, [FromBody] UpdatePantryItemDto dto) {
            try {
                var updated = await facade.UpdatePantryItemAsync(id, dto).ConfigureAwait(false);
                return Ok(updated);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a pantry item
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePantryItemAsync(int id) {
            try {
                await facade.DeletePantryItemAsync(id).ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the default pantry staples list
        /// </summary>
        [HttpGet("defaults")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDefaultStaplesAsync() {
            var staples = await facade.GetDefaultStaplesAsync().ConfigureAwait(false);
            return Ok(staples);
        }
    }
}
