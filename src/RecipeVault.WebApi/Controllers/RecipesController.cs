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
    /// Represents the shared functionality/resources of the recipe resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/recipes")]
    [Authorize]
    public class RecipesController : ControllerBase {
        private readonly IRecipeFacade facade;
        private readonly RecipeModelMapper recipeMapper;

        /// <summary>
        /// Initializes a new instance of the RecipesController
        /// </summary>
        public RecipesController(IRecipeFacade facade, RecipeModelMapper recipeMapper) {
            this.facade = facade;
            this.recipeMapper = recipeMapper;
        }

        /// <summary>
        /// Gets recipes
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<RecipeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecipesAsync([FromQuery] RecipeSearchModel search) {
            var searchDto = recipeMapper.MapToDto(search);
            var results = await facade.SearchRecipesAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => recipeMapper.Map(x));
            return Ok(models);
        }

        /// <summary>
        /// Gets a recipe by id
        /// </summary>
        /// <param name="id">the resource id of the recipe to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetRecipeAsync))]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecipeAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await facade.GetRecipeAsync(id).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Create a new recipe
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRecipeAsync([FromBody] UpdateRecipeModel input) {
            var inputDto = recipeMapper.MapToDto(input);
            var dto = await facade.CreateRecipeAsync(inputDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetRecipeAsync), new { id = dto.RecipeResourceId }, recipeMapper.Map(dto));
        }

        /// <summary>
        /// Update a recipe
        /// </summary>
        /// <param name="id">the resource id of the recipe to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRecipeAsync(Guid id, [FromBody] UpdateRecipeModel input) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var inputDto = recipeMapper.MapToDto(input);
                var dto = await facade.UpdateRecipeAsync(id, inputDto).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Delete a recipe
        /// </summary>
        /// <param name="id">the resource id of the recipe to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteRecipeAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                await facade.DeleteRecipeAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Set recipe visibility (public/private)
        /// </summary>
        /// <param name="id">the resource id of the recipe</param>
        /// <param name="input"></param>
        [HttpPut("{id}/visibility")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetRecipeVisibilityAsync(Guid id, [FromBody] SetVisibilityModel input) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await facade.SetRecipeVisibilityAsync(id, input.IsPublic).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Parse a recipe image using AI
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("parse")]
        [ProducesResponseType(typeof(ParseRecipeResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ParseRecipeAsync([FromBody] ParseRecipeRequestModel input) {
            var requestDto = recipeMapper.MapToDto(input);
            var responseDto = await facade.ParseRecipeImageAsync(requestDto).ConfigureAwait(false);
            var responseModel = recipeMapper.Map(responseDto);
            return Ok(responseModel);
        }
    }
}
