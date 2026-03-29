using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
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
    /// Represents the shared functionality/resources of the circle resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/circles")]
    [Authorize]
    public class CirclesController : ControllerBase {
        private readonly ICircleFacade facade;
        private readonly CircleModelMapper circleMapper;
        private readonly RecipeModelMapper recipeMapper;

        /// <summary>
        /// Initializes a new instance of the CirclesController
        /// </summary>
        public CirclesController(ICircleFacade facade, CircleModelMapper circleMapper, RecipeModelMapper recipeMapper) {
            this.facade = facade;
            this.circleMapper = circleMapper;
            this.recipeMapper = recipeMapper;
        }

        /// <summary>
        /// Gets circles
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<CircleModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCirclesAsync([FromQuery] CircleSearchModel search) {
            var searchDto = circleMapper.MapToDto(search);
            var results = await facade.SearchCirclesAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => circleMapper.Map(x));
            return Ok(models);
        }

        /// <summary>
        /// Gets a circle by id
        /// </summary>
        /// <param name="id">the resource id of the circle to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetCircleAsync))]
        [ProducesResponseType(typeof(CircleModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCircleAsync(Guid id) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var dto = await facade.GetCircleAsync(id).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Create a new circle
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(CircleModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCircleAsync([FromBody] UpdateCircleModel input) {
            var inputDto = circleMapper.MapToDto(input);
            var dto = await facade.CreateCircleAsync(inputDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetCircleAsync), new { id = dto.CircleResourceId }, circleMapper.Map(dto));
        }

        /// <summary>
        /// Update a circle
        /// </summary>
        /// <param name="id">the resource id of the circle to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CircleModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCircleAsync(Guid id, [FromBody] UpdateCircleModel input) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var inputDto = circleMapper.MapToDto(input);
                var dto = await facade.UpdateCircleAsync(id, inputDto).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Delete a circle
        /// </summary>
        /// <param name="id">the resource id of the circle to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCircleAsync(Guid id) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                await facade.DeleteCircleAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Invite a member to the circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="input"></param>
        [HttpPost("{id}/invite")]
        [ProducesResponseType(typeof(CircleInviteModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> InviteToCircleAsync(Guid id, [FromBody] InviteToCircleModel input) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var inputDto = circleMapper.MapToDto(input);
                var dto = await facade.InviteToCircleAsync(id, inputDto).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Accept a circle invite
        /// </summary>
        /// <param name="token">the invite token</param>
        [HttpPost("join/{token}")]
        [ProducesResponseType(typeof(CircleModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcceptInviteAsync(Guid token) {
            using (LogContext.PushProperty("InviteToken", token)) {
                var dto = await facade.AcceptInviteAsync(token).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Get invite details
        /// </summary>
        /// <param name="token">the invite token</param>
        [HttpGet("invite/{token}")]
        [ProducesResponseType(typeof(CircleInviteModel), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetInviteDetailsAsync(Guid token) {
            using (LogContext.PushProperty("InviteToken", token)) {
                var dto = await facade.GetInviteDetailsAsync(token).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Get members of a circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        [HttpGet("{id}/members")]
        [ProducesResponseType(typeof(PagedList<CircleMemberModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCircleMembersAsync(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var results = await facade.GetCircleMembersAsync(id, pageNumber, pageSize).ConfigureAwait(false);
                // Map CircleMemberDto to CircleMemberModel
                var models = results.Convert(x => new CircleMemberModel {
                    SubjectId = x.SubjectId,
                    Email = x.Email,
                    Role = x.Role,
                    Status = x.Status,
                    JoinedDate = x.JoinedDate
                });
                return Ok(models);
            }
        }

        /// <summary>
        /// Remove a member from the circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="subjectId">the subject id of the member to remove</param>
        [HttpDelete("{id}/members/{subjectId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveMemberAsync(Guid id, Guid subjectId) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                await facade.RemoveMemberAsync(id, subjectId).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Leave a circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        [HttpPost("{id}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LeaveCircleAsync(Guid id) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                await facade.LeaveCircleAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Share a recipe to the circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="input"></param>
        [HttpPost("{id}/recipes")]
        [ProducesResponseType(typeof(CircleModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ShareRecipeToCircleAsync(Guid id, [FromBody] ShareRecipeToCircleModel input) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var inputDto = circleMapper.MapToDto(input);
                var dto = await facade.ShareRecipeToCircleAsync(id, inputDto).ConfigureAwait(false);
                return Ok(circleMapper.Map(dto));
            }
        }

        /// <summary>
        /// Unshare a recipe from the circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="recipeId">the resource id of the recipe</param>
        [HttpDelete("{id}/recipes/{recipeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnshareRecipeFromCircleAsync(Guid id, Guid recipeId) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                using (LogContext.PushProperty("RecipeResourceId", recipeId)) {
                    await facade.UnshareRecipeFromCircleAsync(id, recipeId).ConfigureAwait(false);
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            }
        }

        /// <summary>
        /// Get recipes in a circle
        /// </summary>
        /// <param name="id">the resource id of the circle</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        [HttpGet("{id}/recipes")]
        [ProducesResponseType(typeof(PagedList<RecipeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCircleRecipesAsync(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) {
            using (LogContext.PushProperty("CircleResourceId", id)) {
                var results = await facade.GetCircleRecipesAsync(id, pageNumber, pageSize).ConfigureAwait(false);
                var models = results.Convert(x => recipeMapper.Map(x));
                return Ok(models);
            }
        }
    }
}
