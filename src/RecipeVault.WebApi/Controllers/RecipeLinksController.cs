using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Recipe linking (component recipes) functionality
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/recipes")]
    [Authorize]
    public class RecipeLinksController : ControllerBase {
        private readonly IRecipeLinkFacade facade;

        /// <summary>
        /// Initializes a new instance of the RecipeLinksController
        /// </summary>
        public RecipeLinksController(IRecipeLinkFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Create a link to another recipe (component recipe)
        /// </summary>
        /// <param name="id">Parent recipe resource id</param>
        /// <param name="input">Link details</param>
        [HttpPost("{id}/links")]
        [ProducesResponseType(typeof(RecipeLinkModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRecipeLinkAsync(Guid id, [FromBody] CreateRecipeLinkModel input) {
            try {
                using (LogContext.PushProperty("ParentRecipeResourceId", id)) {
                    var dto = new CreateRecipeLinkDto {
                        LinkedRecipeResourceId = input.LinkedRecipeResourceId,
                        IngredientIndex = input.IngredientIndex,
                        DisplayText = input.DisplayText,
                        IncludeInTotalTime = input.IncludeInTotalTime,
                        PortionUsed = input.PortionUsed
                    };

                    var result = await facade.CreateRecipeLinkAsync(id, dto).ConfigureAwait(false);

                    var model = new RecipeLinkModel {
                        RecipeLinkResourceId = result.RecipeLinkResourceId,
                        ParentRecipeId = result.ParentRecipeId,
                        LinkedRecipeId = result.LinkedRecipeId,
                        IngredientIndex = result.IngredientIndex,
                        DisplayText = result.DisplayText,
                        IncludeInTotalTime = result.IncludeInTotalTime,
                        PortionUsed = result.PortionUsed
                    };

                    return StatusCode((int)HttpStatusCode.Created, model);
                }
            } catch (InvalidOperationException ex) {
                return BadRequest(new { message = ex.Message });
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while creating the recipe link" });
            }
        }

        /// <summary>
        /// Update a recipe link
        /// </summary>
        /// <param name="id">Parent recipe resource id</param>
        /// <param name="linkId">Link resource id</param>
        /// <param name="input">Updated link details</param>
        [HttpPut("{id}/links/{linkId}")]
        [ProducesResponseType(typeof(RecipeLinkModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRecipeLinkAsync(Guid id, Guid linkId, [FromBody] UpdateRecipeLinkModel input) {
            try {
                using (LogContext.PushProperty("ParentRecipeResourceId", id))
                using (LogContext.PushProperty("RecipeLinkResourceId", linkId)) {
                    var dto = new UpdateRecipeLinkDto {
                        IngredientIndex = input.IngredientIndex,
                        DisplayText = input.DisplayText,
                        IncludeInTotalTime = input.IncludeInTotalTime,
                        PortionUsed = input.PortionUsed
                    };

                    var result = await facade.UpdateRecipeLinkAsync(id, linkId, dto).ConfigureAwait(false);

                    var model = new RecipeLinkModel {
                        RecipeLinkResourceId = result.RecipeLinkResourceId,
                        ParentRecipeId = result.ParentRecipeId,
                        LinkedRecipeId = result.LinkedRecipeId,
                        IngredientIndex = result.IngredientIndex,
                        DisplayText = result.DisplayText,
                        IncludeInTotalTime = result.IncludeInTotalTime,
                        PortionUsed = result.PortionUsed
                    };

                    return Ok(model);
                }
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while updating the recipe link" });
            }
        }

        /// <summary>
        /// Delete a recipe link
        /// </summary>
        /// <param name="id">Parent recipe resource id</param>
        /// <param name="linkId">Link resource id</param>
        [HttpDelete("{id}/links/{linkId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRecipeLinkAsync(Guid id, Guid linkId) {
            try {
                using (LogContext.PushProperty("ParentRecipeResourceId", id))
                using (LogContext.PushProperty("RecipeLinkResourceId", linkId)) {
                    await facade.DeleteRecipeLinkAsync(id, linkId).ConfigureAwait(false);
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while deleting the recipe link" });
            }
        }

        /// <summary>
        /// Get linked recipes (components) for a recipe
        /// </summary>
        /// <param name="id">Parent recipe resource id</param>
        [HttpGet("{id}/links")]
        [ProducesResponseType(typeof(List<LinkedRecipeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLinkedRecipesAsync(Guid id) {
            try {
                using (LogContext.PushProperty("ParentRecipeResourceId", id)) {
                    var results = await facade.GetLinkedRecipesAsync(id).ConfigureAwait(false);

                    var models = results.Select(r => new LinkedRecipeModel {
                        RecipeLinkResourceId = r.RecipeLinkResourceId,
                        RecipeResourceId = r.RecipeResourceId,
                        Title = r.Title,
                        Yield = r.Yield,
                        PrepTimeMinutes = r.PrepTimeMinutes,
                        CookTimeMinutes = r.CookTimeMinutes,
                        TotalTimeMinutes = r.TotalTimeMinutes,
                        OriginalImageUrl = r.OriginalImageUrl,
                        IngredientIndex = r.IngredientIndex,
                        DisplayText = r.DisplayText,
                        IncludeInTotalTime = r.IncludeInTotalTime,
                        PortionUsed = r.PortionUsed,
                        Ingredients = r.Ingredients?.Select(i => new RecipeIngredientModel {
                            RecipeIngredientId = i.RecipeIngredientId,
                            SortOrder = i.SortOrder,
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Item = i.Item,
                            Preparation = i.Preparation,
                            RawText = i.RawText
                        }).ToList(),
                        Instructions = r.Instructions?.Select(i => new RecipeInstructionModel {
                            RecipeInstructionId = i.RecipeInstructionId,
                            StepNumber = i.StepNumber,
                            Instruction = i.Instruction,
                            RawText = i.RawText
                        }).ToList(),
                        Tags = r.Tags?.Select(t => new RecipeTagModel {
                            TagResourceId = t.TagResourceId,
                            GlobalName = t.GlobalName,
                            DisplayName = t.DisplayName,
                            Category = t.Category,
                            CategoryName = t.CategoryName,
                            SourceType = t.SourceType,
                            SourceTypeName = t.SourceTypeName,
                            IsAiAssigned = t.IsAiAssigned,
                            IsOverridden = t.IsOverridden,
                            Confidence = t.Confidence,
                            Detail = t.Detail,
                            NormalizedEntityId = t.NormalizedEntityId,
                            NormalizedEntityType = t.NormalizedEntityType
                        }).ToList()
                    }).ToList();

                    return Ok(models);
                }
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving linked recipes" });
            }
        }

        /// <summary>
        /// Get recipes that use this recipe as a component
        /// </summary>
        /// <param name="id">Recipe resource id</param>
        [HttpGet("{id}/used-in")]
        [ProducesResponseType(typeof(List<UsedInRecipeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsedInRecipesAsync(Guid id) {
            try {
                using (LogContext.PushProperty("RecipeResourceId", id)) {
                    var results = await facade.GetUsedInRecipesAsync(id).ConfigureAwait(false);

                    var models = results.Select(r => new UsedInRecipeModel {
                        RecipeLinkResourceId = r.RecipeLinkResourceId,
                        RecipeResourceId = r.RecipeResourceId,
                        Title = r.Title,
                        OriginalImageUrl = r.OriginalImageUrl,
                        OwnerName = r.OwnerName
                    }).ToList();

                    return Ok(models);
                }
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving used-in recipes" });
            }
        }

        /// <summary>
        /// Search user's recipes for linking candidates
        /// </summary>
        /// <param name="query">Search query</param>
        [HttpGet("linkable")]
        [ProducesResponseType(typeof(List<RecipeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchLinkableRecipesAsync([FromQuery] string query) {
            try {
                var results = await facade.SearchLinkableRecipesAsync(query ?? string.Empty).ConfigureAwait(false);

                // Simplified mapping - only include essential fields for recipe selection
                var models = results.Select(r => new RecipeModel {
                    RecipeResourceId = r.RecipeResourceId,
                    Title = r.Title,
                    Description = r.Description,
                    Yield = r.Yield,
                    PrepTimeMinutes = r.PrepTimeMinutes,
                    CookTimeMinutes = r.CookTimeMinutes,
                    TotalTimeMinutes = r.TotalTimeMinutes,
                    OriginalImageUrl = r.OriginalImageUrl,
                    Ingredients = r.Ingredients?.Select(i => new RecipeIngredientModel {
                        RecipeIngredientId = i.RecipeIngredientId,
                        SortOrder = i.SortOrder,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Item = i.Item,
                        Preparation = i.Preparation,
                        RawText = i.RawText
                    }).ToList(),
                    Tags = r.Tags?.Select(t => new RecipeTagModel {
                        TagResourceId = t.TagResourceId,
                        GlobalName = t.GlobalName,
                        DisplayName = t.DisplayName,
                        Category = t.Category,
                        CategoryName = t.CategoryName,
                        SourceType = t.SourceType,
                        SourceTypeName = t.SourceTypeName,
                        IsAiAssigned = t.IsAiAssigned,
                        IsOverridden = t.IsOverridden,
                        Confidence = t.Confidence,
                        Detail = t.Detail,
                        NormalizedEntityId = t.NormalizedEntityId,
                        NormalizedEntityType = t.NormalizedEntityType
                    }).ToList()
                }).ToList();

                return Ok(models);
            } catch (Exception) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while searching linkable recipes" });
            }
        }
    }
}
