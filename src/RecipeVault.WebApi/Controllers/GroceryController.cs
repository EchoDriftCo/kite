using System;
using System.Linq;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.WebApi.Models.Responses;
using RecipeVault.WebApi.Services;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Grocery delivery helpers
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/grocery")]
    [Authorize]
    public class GroceryController : ControllerBase {
        /// <summary>
        /// Build a grocery service checkout/search URL from ingredient items.
        /// </summary>
        /// <param name="service">Target grocery service: instacart, walmart, amazonfresh, shipt, manual</param>
        /// <param name="items">Ingredient items (supports repeated query params and comma-separated values)</param>
        /// <param name="zipCode">Optional zip code (reserved for future service-specific behavior)</param>
        /// <param name="store">Optional preferred store (reserved for future service-specific behavior)</param>
        [HttpGet("checkout-url")]
        [ProducesResponseType(typeof(GroceryCheckoutUrlModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult GetCheckoutUrl([FromQuery] string service, [FromQuery] string[] items, [FromQuery] string zipCode = null, [FromQuery] string store = null) {
            if (!GroceryCheckoutUrlBuilder.IsSupportedService(service)) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid grocery service",
                    Detail = "Service must be one of: instacart, walmart, amazonfresh, shipt, manual.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var normalizedItems = GroceryCheckoutUrlBuilder.NormalizeItems(items);
            if (normalizedItems.Count == 0) {
                return BadRequest(new ProblemDetails {
                    Title = "No valid grocery items",
                    Detail = "At least one valid ingredient item is required.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var normalizedService = service.Trim().ToLowerInvariant();
            var model = new GroceryCheckoutUrlModel {
                Service = normalizedService,
                Url = GroceryCheckoutUrlBuilder.BuildUrl(normalizedService, normalizedItems, zipCode, store),
                ItemCount = normalizedItems.Count,
                NormalizedItems = normalizedItems
            };

            return Ok(model);
        }

        /// <summary>
        /// Build checkout/search URLs across all supported services for quick comparison.
        /// </summary>
        [HttpGet("checkout-options")]
        [ProducesResponseType(typeof(GroceryCheckoutOptionsModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult GetCheckoutOptions([FromQuery] string[] items, [FromQuery] string zipCode = null, [FromQuery] string store = null) {
            var normalizedItems = GroceryCheckoutUrlBuilder.NormalizeItems(items);
            if (normalizedItems.Count == 0) {
                return BadRequest(new ProblemDetails {
                    Title = "No valid grocery items",
                    Detail = "At least one valid ingredient item is required.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var options = GroceryCheckoutUrlBuilder.GetSupportedServices()
                .Select(s => new GroceryCheckoutUrlModel {
                    Service = s,
                    Url = GroceryCheckoutUrlBuilder.BuildUrl(s, normalizedItems, zipCode, store),
                    ItemCount = normalizedItems.Count,
                    NormalizedItems = normalizedItems
                })
                .ToList();

            return Ok(new GroceryCheckoutOptionsModel {
                ItemCount = normalizedItems.Count,
                NormalizedItems = normalizedItems,
                Options = options
            });
        }
    }
}
