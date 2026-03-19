using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Represents the shared functionality/resources of the collection resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/collections")]
    [Authorize]
    public class CollectionsController : ControllerBase {
        private readonly ICollectionFacade facade;
        private readonly CollectionModelMapper mapper;
        private readonly ILogger<CollectionsController> logger;

        /// <summary>
        /// Initializes a new instance of the CollectionsController
        /// </summary>
        public CollectionsController(ICollectionFacade facade, CollectionModelMapper mapper, ILogger<CollectionsController> logger) {
            this.facade = facade;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Gets collections
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<CollectionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCollectionsAsync([FromQuery] CollectionSearchModel search) {
            try {
                var searchDto = mapper.MapToDto(search);
                var results = await facade.SearchCollectionsAsync(searchDto).ConfigureAwait(false);
                var models = results.Convert(x => mapper.Map(x));
                return Ok(models);
            } catch (Exception ex) {
                logger.LogError(ex, "Error searching collections");
                throw;
            }
        }

        /// <summary>
        /// Gets my collections (user's collections)
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<CollectionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCollectionsAsync() {
            try {
                var results = await facade.GetUserCollectionsAsync().ConfigureAwait(false);
                var models = results.Select(x => mapper.Map(x)).ToList();
                return Ok(models);
            } catch (Exception ex) {
                logger.LogError(ex, "Error getting user collections");
                throw;
            }
        }

        /// <summary>
        /// Gets featured collections
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedList<CollectionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedCollectionsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) {
            try {
                var results = await facade.GetFeaturedCollectionsAsync(pageNumber, pageSize).ConfigureAwait(false);
                var models = results.Convert(x => mapper.Map(x));
                return Ok(models);
            } catch (Exception ex) {
                logger.LogError(ex, "Error getting featured collections");
                throw;
            }
        }

        /// <summary>
        /// Gets public collections
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedList<CollectionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicCollectionsAsync([FromQuery] string search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) {
            try {
                var results = await facade.GetPublicCollectionsAsync(search, pageNumber, pageSize).ConfigureAwait(false);
                var models = results.Convert(x => mapper.Map(x));
                return Ok(models);
            } catch (Exception ex) {
                logger.LogError(ex, "Error getting public collections with search: {Search}", search);
                throw;
            }
        }

        /// <summary>
        /// Gets a collection by id
        /// </summary>
        /// <param name="id">the resource id of the collection to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetCollectionAsync))]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCollectionAsync(Guid id) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                try {
                    var dto = await facade.GetCollectionAsync(id).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                } catch (Exception ex) {
                    logger.LogError(ex, "Error getting collection {CollectionId}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Create a new collection
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCollectionAsync([FromBody] UpdateCollectionModel input) {
            try {
                var inputDto = mapper.MapToDto(input);
                var dto = await facade.CreateCollectionAsync(inputDto).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetCollectionAsync), new { id = dto.CollectionResourceId }, mapper.Map(dto));
            } catch (Exception ex) {
                logger.LogError(ex, "Error creating collection");
                throw;
            }
        }

        /// <summary>
        /// Update a collection
        /// </summary>
        /// <param name="id">the resource id of the collection to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCollectionAsync(Guid id, [FromBody] UpdateCollectionModel input) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                try {
                    var inputDto = mapper.MapToDto(input);
                    var dto = await facade.UpdateCollectionAsync(id, inputDto).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                } catch (Exception ex) {
                    logger.LogError(ex, "Error updating collection {CollectionId}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete a collection
        /// </summary>
        /// <param name="id">the resource id of the collection to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCollectionAsync(Guid id) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                try {
                    await facade.DeleteCollectionAsync(id).ConfigureAwait(false);
                    return NoContent();
                } catch (Exception ex) {
                    logger.LogError(ex, "Error deleting collection {CollectionId}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Add a recipe to a collection
        /// </summary>
        /// <param name="id">the resource id of the collection</param>
        /// <param name="input"></param>
        [HttpPost("{id}/recipes")]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddRecipeToCollectionAsync(Guid id, [FromBody] AddRecipeToCollectionModel input) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                try {
                    var inputDto = mapper.MapToDto(input);
                    var dto = await facade.AddRecipeToCollectionAsync(id, inputDto).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                } catch (Exception ex) {
                    logger.LogError(ex, "Error adding recipe to collection {CollectionId}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Remove a recipe from a collection
        /// </summary>
        /// <param name="id">the resource id of the collection</param>
        /// <param name="recipeId">the resource id of the recipe to remove</param>
        [HttpDelete("{id}/recipes/{recipeId}")]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRecipeFromCollectionAsync(Guid id, Guid recipeId) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                using (LogContext.PushProperty("RecipeResourceId", recipeId)) {
                    try {
                        var dto = await facade.RemoveRecipeFromCollectionAsync(id, recipeId).ConfigureAwait(false);
                        return Ok(mapper.Map(dto));
                    } catch (Exception ex) {
                        logger.LogError(ex, "Error removing recipe {RecipeId} from collection {CollectionId}", recipeId, id);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Reorder recipes within a collection
        /// </summary>
        /// <param name="id">the resource id of the collection</param>
        /// <param name="input"></param>
        [HttpPut("{id}/recipes/reorder")]
        [ProducesResponseType(typeof(CollectionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReorderCollectionRecipesAsync(Guid id, [FromBody] ReorderCollectionRecipesModel input) {
            using (LogContext.PushProperty("CollectionResourceId", id)) {
                try {
                    var inputDto = mapper.MapToDto(input);
                    var dto = await facade.ReorderCollectionRecipesAsync(id, inputDto).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                } catch (Exception ex) {
                    logger.LogError(ex, "Error reordering recipes in collection {CollectionId}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Reorder user's collections
        /// </summary>
        /// <param name="input"></param>
        [HttpPut("reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ReorderCollectionsAsync([FromBody] ReorderCollectionsModel input) {
            try {
                var inputDto = mapper.MapToDto(input);
                await facade.ReorderCollectionsAsync(inputDto).ConfigureAwait(false);
                return NoContent();
            } catch (Exception ex) {
                logger.LogError(ex, "Error reordering collections");
                throw;
            }
        }
    }
}
