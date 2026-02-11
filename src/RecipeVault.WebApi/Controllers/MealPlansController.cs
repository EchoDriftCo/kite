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
    /// Represents the shared functionality/resources of the meal plan resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/meal-plans")]
    [Authorize]
    public class MealPlansController : ControllerBase {
        private readonly IMealPlanFacade facade;
        private readonly MealPlanModelMapper mealPlanMapper;

        /// <summary>
        /// Initializes a new instance of the MealPlansController
        /// </summary>
        public MealPlansController(IMealPlanFacade facade, MealPlanModelMapper mealPlanMapper) {
            this.facade = facade;
            this.mealPlanMapper = mealPlanMapper;
        }

        /// <summary>
        /// Gets meal plans
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<MealPlanModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMealPlansAsync([FromQuery] MealPlanSearchModel search) {
            var searchDto = mealPlanMapper.MapToDto(search);
            var results = await facade.SearchMealPlansAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => mealPlanMapper.Map(x));
            return Ok(models);
        }

        /// <summary>
        /// Gets a meal plan by id
        /// </summary>
        /// <param name="id">the resource id of the meal plan to get</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetMealPlanAsync))]
        [ProducesResponseType(typeof(MealPlanModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMealPlanAsync(Guid id) {
            using (LogContext.PushProperty("MealPlanResourceId", id)) {
                var dto = await facade.GetMealPlanAsync(id).ConfigureAwait(false);
                return Ok(mealPlanMapper.Map(dto));
            }
        }

        /// <summary>
        /// Create a new meal plan
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(MealPlanModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateMealPlanAsync([FromBody] UpdateMealPlanModel input) {
            var inputDto = mealPlanMapper.MapToDto(input);
            var dto = await facade.CreateMealPlanAsync(inputDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetMealPlanAsync), new { id = dto.MealPlanResourceId }, mealPlanMapper.Map(dto));
        }

        /// <summary>
        /// Update a meal plan
        /// </summary>
        /// <param name="id">the resource id of the meal plan to update</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MealPlanModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMealPlanAsync(Guid id, [FromBody] UpdateMealPlanModel input) {
            using (LogContext.PushProperty("MealPlanResourceId", id)) {
                var inputDto = mealPlanMapper.MapToDto(input);
                var dto = await facade.UpdateMealPlanAsync(id, inputDto).ConfigureAwait(false);
                return Ok(mealPlanMapper.Map(dto));
            }
        }

        /// <summary>
        /// Delete a meal plan
        /// </summary>
        /// <param name="id">the resource id of the meal plan to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMealPlanAsync(Guid id) {
            using (LogContext.PushProperty("MealPlanResourceId", id)) {
                await facade.DeleteMealPlanAsync(id).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Generate grocery list for a meal plan
        /// </summary>
        /// <param name="id">the resource id of the meal plan</param>
        [HttpGet("{id}/grocery-list")]
        [ProducesResponseType(typeof(GroceryListModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroceryListAsync(Guid id) {
            using (LogContext.PushProperty("MealPlanResourceId", id)) {
                var groceryList = await facade.GetGroceryListAsync(id).ConfigureAwait(false);
                return Ok(mealPlanMapper.Map(groceryList));
            }
        }
    }
}
