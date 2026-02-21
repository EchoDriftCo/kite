using System;
using System.Linq;
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
    /// Represents the shared functionality/resources of the tag resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/tags")]
    [Authorize]
    public class TagsController : ControllerBase {
        private readonly ITagFacade facade;
        private readonly TagModelMapper tagMapper;

        /// <summary>
        /// Initializes a new instance of the TagsController
        /// </summary>
        public TagsController(ITagFacade facade, TagModelMapper tagMapper) {
            this.facade = facade;
            this.tagMapper = tagMapper;
        }

        /// <summary>
        /// Gets tags (global + user's own)
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<TagModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTagsAsync([FromQuery] TagSearchModel search) {
            var searchDto = tagMapper.MapToDto(search);
            var results = await facade.SearchTagsAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => tagMapper.Map(x));
            return Ok(models);
        }

        /// <summary>
        /// Gets a tag by id
        /// </summary>
        /// <param name="id">the resource id of the tag to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetTagAsync))]
        [ProducesResponseType(typeof(TagModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTagAsync(Guid id) {
            using (LogContext.PushProperty("TagResourceId", id)) {
                var dto = await facade.GetTagAsync(id).ConfigureAwait(false);
                return Ok(tagMapper.Map(dto));
            }
        }

        /// <summary>
        /// Create a new user tag
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(TagModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTagAsync([FromBody] UpdateTagModel input) {
            var inputDto = tagMapper.MapToDto(input);
            var dto = await facade.CreateTagAsync(inputDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetTagAsync), new { id = dto.TagResourceId }, tagMapper.Map(dto));
        }

        /// <summary>
        /// Update a user tag (own only, not global)
        /// </summary>
        /// <param name="id">the resource id of the tag to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TagModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTagAsync(Guid id, [FromBody] UpdateTagModel input) {
            using (LogContext.PushProperty("TagResourceId", id)) {
                var inputDto = tagMapper.MapToDto(input);
                var dto = await facade.UpdateTagAsync(id, inputDto).ConfigureAwait(false);
                return Ok(tagMapper.Map(dto));
            }
        }

        /// <summary>
        /// Delete a user tag (own only, not global)
        /// </summary>
        /// <param name="id">the resource id of the tag to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTagAsync(Guid id) {
            using (LogContext.PushProperty("TagResourceId", id)) {
                await facade.DeleteTagAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Set or update an alias for a tag
        /// </summary>
        /// <param name="id">the resource id of the tag</param>
        /// <param name="input">the alias details</param>
        [HttpPut("{id}/alias")]
        [ProducesResponseType(typeof(TagModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetAliasAsync(Guid id, [FromBody] SetAliasModel input) {
            using (LogContext.PushProperty("TagResourceId", id)) {
                var inputDto = tagMapper.MapToDto(input);
                var dto = await facade.SetAliasAsync(id, inputDto).ConfigureAwait(false);
                return Ok(tagMapper.Map(dto));
            }
        }

        /// <summary>
        /// Remove an alias for a tag
        /// </summary>
        /// <param name="id">the resource id of the tag</param>
        [HttpDelete("{id}/alias")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAliasAsync(Guid id) {
            using (LogContext.PushProperty("TagResourceId", id)) {
                await facade.RemoveAliasAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Get all tag aliases for the current user
        /// </summary>
        [HttpGet("aliases")]
        [ProducesResponseType(typeof(TagModel[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserAliasesAsync() {
            var aliases = await facade.GetUserAliasesAsync().ConfigureAwait(false);
            var models = aliases.Select(x => tagMapper.Map(x)).ToList();
            return Ok(models);
        }
    }
}
