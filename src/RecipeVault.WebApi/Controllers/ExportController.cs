using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Facade;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Export recipes in various formats
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/export")]
    [Authorize]
    public class ExportController : ControllerBase {
        private readonly IExportFacade facade;

        /// <summary>
        /// Initializes a new instance of the ExportController
        /// </summary>
        public ExportController(IExportFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Export a recipe as JSON
        /// </summary>
        /// <param name="id">Recipe resource ID</param>
        [HttpGet("recipes/{id}/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> ExportRecipeAsJsonAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var json = await facade.ExportRecipeAsJsonAsync(id).ConfigureAwait(false);
                return Content(json, "application/json");
            }
        }

        /// <summary>
        /// Export a recipe as plain text
        /// </summary>
        /// <param name="id">Recipe resource ID</param>
        [HttpGet("recipes/{id}/text")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("text/plain")]
        public async Task<IActionResult> ExportRecipeAsTextAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var text = await facade.ExportRecipeAsTextAsync(id).ConfigureAwait(false);
                return Content(text, "text/plain");
            }
        }

        /// <summary>
        /// Export a recipe in Paprika format (.paprikarecipes)
        /// </summary>
        /// <param name="id">Recipe resource ID</param>
        [HttpGet("recipes/{id}/paprika")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportRecipeAsPaprikaAsync(Guid id) {
            using (LogContext.PushProperty("RecipeResourceId", id)) {
                var data = await facade.ExportRecipeAsPaprikaAsync(id).ConfigureAwait(false);
                return File(data, "application/octet-stream", $"recipe-{id}.paprikarecipes");
            }
        }

        /// <summary>
        /// Export all user recipes in Paprika format (.paprikarecipes)
        /// </summary>
        [HttpPost("recipes/paprika")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportAllRecipesAsPaprikaAsync() {
            var data = await facade.ExportAllAsPaprikaAsync().ConfigureAwait(false);
            var fileName = $"recipes-{DateTime.UtcNow:yyyy-MM-dd}.paprikarecipes";
            return File(data, "application/octet-stream", fileName);
        }
    }
}
