using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
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
        private readonly IWebHostEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the RecipesController
        /// </summary>
        public RecipesController(IRecipeFacade facade, RecipeModelMapper recipeMapper, IWebHostEnvironment environment) {
            this.facade = facade;
            this.recipeMapper = recipeMapper;
            this.environment = environment;
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

        /// <summary>
        /// Assign tags to a recipe
        /// </summary>
        /// <param name="id">the resource id of the recipe</param>
        /// <param name="input"></param>
        [HttpPost("{id}/tags")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignTagsAsync(Guid id, [FromBody] AssignTagsModel input) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var tagDtos = input.Tags?.Select(t => new AssignTagDto {
                    TagResourceId = t.TagResourceId,
                    Name = t.Name,
                    Category = t.Category
                }).ToList() ?? new List<AssignTagDto>();
                var dto = await facade.AssignTagsAsync(id, tagDtos).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Remove a tag from a recipe
        /// </summary>
        /// <param name="id">the resource id of the recipe</param>
        /// <param name="tagId">the resource id of the tag to remove</param>
        [HttpDelete("{id}/tags/{tagId}")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveTagAsync(Guid id, Guid tagId) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await facade.RemoveTagAsync(id, tagId).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Trigger AI dietary tag analysis for a recipe
        /// </summary>
        /// <param name="id">the resource id of the recipe</param>
        [HttpPost("{id}/analyze-tags")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AnalyzeTagsAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var dto = await facade.AnalyzeDietaryTagsAsync(id).ConfigureAwait(false);
                return Ok(recipeMapper.Map(dto));
            }
        }

        /// <summary>
        /// Upload a recipe image
        /// </summary>
        [HttpPost("images")]
        [ProducesResponseType(typeof(UploadImageResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(15_000_000)] // 15MB to cover base64 overhead
        public async Task<IActionResult> UploadImageAsync([FromBody] UploadImageRequestModel input) {
            if (string.IsNullOrWhiteSpace(input?.ImageData)) {
                return BadRequest("Image data is required");
            }

            // Determine file extension from MIME type
            var extension = input.MimeType?.ToLowerInvariant() switch {
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".jpg"
            };

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDir = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "recipes");
            Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);
            var imageBytes = Convert.FromBase64String(input.ImageData);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes).ConfigureAwait(false);

            var url = $"/uploads/recipes/{fileName}";
            return Ok(new UploadImageResponseModel { Url = url });
        }
    }
}
