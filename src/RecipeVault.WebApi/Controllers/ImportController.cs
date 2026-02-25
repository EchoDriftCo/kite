using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class ImportController : ControllerBase {
        private readonly IImportFacade facade;
        private readonly ImportModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the ImportController
        /// </summary>
        public ImportController(IImportFacade facade, ImportModelMapper mapper) {
            this.facade = facade;
            this.mapper = mapper;
        }

        /// <summary>
        /// Import recipes from Paprika format (.paprikarecipes file)
        /// </summary>
        /// <param name="file">The .paprikarecipes file to import</param>
        [HttpPost("paprika")]
        [ProducesResponseType(typeof(ImportResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportFromPaprikaAsync(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".paprikarecipes", StringComparison.OrdinalIgnoreCase)) {
                return BadRequest("File must be a .paprikarecipes file");
            }

            using (LogContext.PushProperty("FileName", file.FileName))
            using (LogContext.PushProperty("FileSize", file.Length)) {
                await using var stream = file.OpenReadStream();
                var resultDto = await facade.ImportFromPaprikaAsync(stream).ConfigureAwait(false);
                var resultModel = mapper.Map(resultDto);
                
                return Ok(resultModel);
            }
        }

        /// <summary>
        /// Import a recipe from a URL
        /// </summary>
        /// <param name="request">The URL to import from</param>
        [HttpPost("url")]
        [ProducesResponseType(typeof(Dto.Output.RecipeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportFromUrlAsync([FromBody] Dto.Input.ImportUrlRequestDto request) {
            if (request == null || string.IsNullOrWhiteSpace(request.Url)) {
                return BadRequest("URL is required");
            }

            using (LogContext.PushProperty("ImportUrl", request.Url)) {
                var recipeDto = await facade.ImportFromUrlAsync(request.Url).ConfigureAwait(false);
                return Ok(recipeDto);
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
        public async Task<IActionResult> ImportFromMultipleImagesAsync([FromForm] List<IFormFile> images, [FromForm] string processingMode = "sequential") {
            if (images == null || images.Count == 0) {
                return BadRequest("At least one image is required");
            }

            if (images.Count > 4) {
                return BadRequest("Maximum 4 images allowed");
            }

            using (LogContext.PushProperty("ImageCount", images.Count))
            using (LogContext.PushProperty("ProcessingMode", processingMode)) {
                var recipeDto = await facade.ImportFromMultipleImagesAsync(images, processingMode).ConfigureAwait(false);
                return Ok(recipeDto);
            }
        }
    }
}
