using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Import recipes from various formats
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/import")]
    [Authorize]
    [EnableRateLimiting("upload")]
    public class ImportController : ControllerBase {
        private readonly IImportFacade facade;
        private readonly ImportModelMapper mapper;
        private readonly ILogger<ImportController> logger;

        /// <summary>
        /// Initializes a new instance of the ImportController
        /// </summary>
        private readonly RecipeModelMapper recipeMapper;

        /// <summary>
        /// Initializes a new instance of the ImportController
        /// </summary>
        public ImportController(IImportFacade facade, ImportModelMapper mapper, RecipeModelMapper recipeMapper, ILogger<ImportController> logger) {
            this.facade = facade;
            this.mapper = mapper;
            this.recipeMapper = recipeMapper;
            this.logger = logger;
        }

        /// <summary>
        /// Import recipes from Paprika format (.paprikarecipes file)
        /// </summary>
        /// <param name="file">The .paprikarecipes file to import</param>
        [HttpPost("paprika")]
        [ProducesResponseType(typeof(ImportResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ImportFromPaprikaAsync(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest("No file provided");
            }

            const long maxFileSize = 50 * 1024 * 1024; // 50MB
            if (file.Length > maxFileSize) {
                return BadRequest("File size exceeds 50MB limit");
            }

            if (!file.FileName.EndsWith(".paprikarecipes", StringComparison.OrdinalIgnoreCase)) {
                return BadRequest("File must be a .paprikarecipes file");
            }

            using (LogContext.PushProperty("FileName", file.FileName))
            using (LogContext.PushProperty("FileSize", file.Length)) {
                try {
                    await using var stream = file.OpenReadStream();
                    var resultDto = await facade.ImportFromPaprikaAsync(stream).ConfigureAwait(false);
                    var resultModel = mapper.Map(resultDto);
                    
                    return Ok(resultModel);
                } catch (InvalidDataException ex) {
                    logger.LogError(ex, "Invalid Paprika file format: {FileName}", file.FileName);
                    return UnprocessableEntity($"Invalid Paprika file format: {ex.Message}");
                } catch (Exception ex) {
                    logger.LogError(ex, "Error importing Paprika file: {FileName}", file.FileName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Import a recipe from a URL
        /// </summary>
        /// <param name="request">The URL to import from</param>
        [HttpPost("url")]
        [ProducesResponseType(typeof(Dto.Output.RecipeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ImportFromUrlAsync([FromBody] Dto.Input.ImportUrlRequestDto request) {
            if (request == null || string.IsNullOrWhiteSpace(request.Url)) {
                return BadRequest("URL is required");
            }

            using (LogContext.PushProperty("ImportUrl", request.Url)) {
                try {
                    var recipeDto = await facade.ImportFromUrlAsync(request.Url).ConfigureAwait(false);
                    return Ok(recipeDto);
                } catch (HttpRequestException ex) {
                    logger.LogError(ex, "Unable to fetch URL for import: {Url}", request.Url);
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, $"Unable to fetch recipe from URL: {ex.Message}");
                } catch (InvalidOperationException ex) {
                    logger.LogError(ex, "No recipe found at URL: {Url}", request.Url);
                    return UnprocessableEntity($"No recipe found at URL: {ex.Message}");
                } catch (Exception ex) {
                    logger.LogError(ex, "Error importing from URL: {Url}", request.Url);
                    throw;
                }
            }
        }

        /// <summary>
        /// Import a recipe from multiple images (e.g., multi-page cookbook photos)
        /// </summary>
        /// <param name="images">List of image files (1-4 images)</param>
        /// <param name="processingMode">Processing mode: "sequential" (default) or "stitch"</param>
        [HttpPost("multi-image")]
        [ProducesResponseType(typeof(Dto.Output.RecipeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ImportFromMultipleImagesAsync([FromForm] List<IFormFile> images, [FromForm] string processingMode = "sequential") {
            if (images == null || images.Count == 0) {
                return BadRequest("At least one image is required");
            }

            if (images.Count > 4) {
                return BadRequest("Maximum 4 images allowed");
            }

            const long maxFileSize = 50 * 1024 * 1024; // 50MB
            foreach (var image in images) {
                if (image.Length > maxFileSize) {
                    return BadRequest($"File '{image.FileName}' exceeds 50MB limit");
                }
            }

            using (LogContext.PushProperty("ImageCount", images.Count))
            using (LogContext.PushProperty("ProcessingMode", processingMode)) {
                try {
                    var recipeDto = await facade.ImportFromMultipleImagesAsync(images, processingMode).ConfigureAwait(false);
                    return Ok(recipeDto);
                } catch (HttpRequestException ex) {
                    logger.LogError(ex, "External image processing API unavailable");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Image processing service temporarily unavailable");
                } catch (InvalidOperationException ex) {
                    logger.LogError(ex, "Unable to parse recipe from images");
                    return UnprocessableEntity($"Unable to parse recipe from images: {ex.Message}");
                } catch (Exception ex) {
                    logger.LogError(ex, "Error importing from multiple images");
                    throw;
                }
            }
        }

        /// <summary>
        /// Import a recipe from structured data (browser extension)
        /// </summary>
        [HttpPost("structured")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportStructuredAsync([FromBody] Models.Requests.ImportStructuredRequestModel model) {
            using (LogContext.PushProperty("Source", model.Source)) {
                var dto = new Dto.Input.ImportStructuredRequestDto {
                    Title = model.Title,
                    Description = model.Description,
                    Yield = model.Yield,
                    PrepTimeMinutes = model.PrepTimeMinutes,
                    CookTimeMinutes = model.CookTimeMinutes,
                    Source = model.Source,
                    OriginalImageUrl = model.OriginalImageUrl,
                    RawIngredients = model.RawIngredients,
                    RawInstructions = model.RawInstructions,
                    Categories = model.Categories
                };
                var recipe = await facade.ImportStructuredAsync(dto).ConfigureAwait(false);
                return Ok(recipeMapper.Map(recipe));
            }
        }

        /// <summary>
        /// Import a recipe from raw HTML content (browser extension server-side extraction)
        /// </summary>
        [HttpPost("html")]
        [ProducesResponseType(typeof(RecipeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportHtmlAsync([FromBody] Models.Requests.ImportHtmlRequestModel model) {
            using (LogContext.PushProperty("Source", model.Source)) {
                var dto = new Dto.Input.ImportHtmlRequestDto {
                    Html = model.Html,
                    Source = model.Source
                };
                var recipe = await facade.ImportHtmlAsync(dto).ConfigureAwait(false);
                return Ok(recipeMapper.Map(recipe));
            }
        }
    }
}
