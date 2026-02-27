using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Represents the kitchen equipment resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/equipment")]
    [Authorize]
    public class EquipmentController : ControllerBase {
        private readonly IEquipmentFacade facade;
        private readonly ISubjectPrincipal subjectPrincipal;

        /// <summary>
        /// Initializes a new instance of the EquipmentController
        /// </summary>
        public EquipmentController(IEquipmentFacade facade, ISubjectPrincipal subjectPrincipal) {
            this.facade = facade;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        /// <summary>
        /// Gets all available equipment options
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(List<EquipmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEquipmentAsync() {
            try {
                var equipment = await facade.GetAllEquipmentAsync().ConfigureAwait(false);
                return Ok(equipment);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets my equipment (user's equipment)
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<UserEquipmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyEquipmentAsync() {
            try {
                var equipment = await facade.GetUserEquipmentAsync(CurrentSubjectId).ConfigureAwait(false);
                return Ok(equipment);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Adds equipment to my kitchen
        /// </summary>
        [HttpPost("my")]
        [ProducesResponseType(typeof(UserEquipmentDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddMyEquipmentAsync([FromBody] AddUserEquipmentDto dto) {
            try {
                var equipment = await facade.AddUserEquipmentAsync(CurrentSubjectId, dto).ConfigureAwait(false);
                return Created($"/api/v1/equipment/my", equipment);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Removes equipment from my kitchen
        /// </summary>
        /// <param name="code">the equipment code to remove</param>
        [HttpDelete("my/{code}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveMyEquipmentAsync(string code) {
            try {
                await facade.RemoveUserEquipmentAsync(CurrentSubjectId, code).ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets equipment for a recipe
        /// </summary>
        /// <param name="id">the recipe resource id</param>
        [HttpGet("recipes/{id}")]
        [ProducesResponseType(typeof(List<RecipeEquipmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecipeEquipmentAsync(Guid id) {
            try {
                using (LogContext.PushProperty("RecipeResourceId", id)) {
                    var equipment = await facade.GetRecipeEquipmentAsync(id).ConfigureAwait(false);
                    return Ok(equipment);
                }
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Auto-detects equipment from recipe instructions
        /// </summary>
        /// <param name="id">the recipe resource id</param>
        [HttpPost("recipes/{id}/detect")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DetectRecipeEquipmentAsync(Guid id) {
            try {
                using (LogContext.PushProperty("RecipeResourceId", id)) {
                    var equipment = await facade.DetectEquipmentFromInstructionsAsync(id).ConfigureAwait(false);
                    return Ok(equipment);
                }
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Checks if I can make this recipe with my equipment
        /// </summary>
        /// <param name="id">the recipe resource id</param>
        [HttpGet("recipes/{id}/check")]
        [ProducesResponseType(typeof(EquipmentCheckDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRecipeEquipmentAsync(Guid id) {
            try {
                using (LogContext.PushProperty("RecipeResourceId", id)) {
                    var check = await facade.CheckRecipeEquipmentAsync(CurrentSubjectId, id).ConfigureAwait(false);
                    return Ok(check);
                }
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
