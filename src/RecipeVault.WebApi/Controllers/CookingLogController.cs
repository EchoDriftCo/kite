using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Cooking log and cooking history
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/cooking-log")]
    [Authorize]
    public class CookingLogController : ControllerBase {
        private readonly ICookingLogFacade facade;
        private readonly CookingLogModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the CookingLogController
        /// </summary>
        public CookingLogController(ICookingLogFacade facade, CookingLogModelMapper mapper) {
            this.facade = facade;
            this.mapper = mapper;
        }

        /// <summary>
        /// Log a cook (I Made This)
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("")]
        [ProducesResponseType(typeof(CookingLogModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCookingLogAsync([FromBody] CreateCookingLogModel input) {
            try {
                var inputDto = mapper.MapToDto(input);
                var dto = await facade.CreateCookingLogAsync(inputDto).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetCookingLogAsync), new { id = dto.CookingLogResourceId }, mapper.Map(dto));
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get cooking history (paginated list)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<CookingLogModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCookingLogsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) {
            try {
                var results = await facade.GetCookingLogsAsync(pageNumber, pageSize).ConfigureAwait(false);
                var models = results.Convert(x => mapper.Map(x));
                return Ok(models);
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a cooking log entry by id
        /// </summary>
        /// <param name="id">The resource id of the cooking log</param>
        [HttpGet("{id}")]
        [ActionName(nameof(GetCookingLogAsync))]
        [ProducesResponseType(typeof(CookingLogModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCookingLogAsync(Guid id) {
            try {
                using (LogContext.PushProperty("CookingLogResourceId", id)) {
                    var dto = await facade.GetCookingLogAsync(id).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                }
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a cooking log entry
        /// </summary>
        /// <param name="id">The resource id of the cooking log</param>
        /// <param name="input"></param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CookingLogModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCookingLogAsync(Guid id, [FromBody] UpdateCookingLogModel input) {
            try {
                using (LogContext.PushProperty("CookingLogResourceId", id)) {
                    var inputDto = mapper.MapToDto(input);
                    var dto = await facade.UpdateCookingLogAsync(id, inputDto).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                }
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a cooking log entry
        /// </summary>
        /// <param name="id">The resource id of the cooking log</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCookingLogAsync(Guid id) {
            try {
                using (LogContext.PushProperty("CookingLogResourceId", id)) {
                    await facade.DeleteCookingLogAsync(id).ConfigureAwait(false);
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add photos to a cooking log entry
        /// </summary>
        /// <param name="id">The resource id of the cooking log</param>
        /// <param name="input"></param>
        [HttpPost("{id}/photos")]
        [ProducesResponseType(typeof(CookingLogModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddPhotosAsync(Guid id, [FromBody] AddCookingLogPhotosModel input) {
            try {
                using (LogContext.PushProperty("CookingLogResourceId", id)) {
                    var photos = input.Photos.Select(p => new CookingLogPhotoDto {
                        ImageUrl = p.ImageUrl,
                        Caption = p.Caption,
                        SortOrder = p.SortOrder
                    }).ToList();
                    
                    var dto = await facade.AddPhotosAsync(id, photos).ConfigureAwait(false);
                    return Ok(mapper.Map(dto));
                }
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get cooking statistics for the current user
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(CookingStatsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCookingStatsAsync() {
            try {
                var dto = await facade.GetCookingStatsAsync().ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get cooking log entries for a specific month (calendar view)
        /// </summary>
        /// <param name="year">Year (e.g., 2026)</param>
        /// <param name="month">Month (1-12)</param>
        [HttpGet("calendar")]
        [ProducesResponseType(typeof(List<CookingLogModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCalendarEntriesAsync([FromQuery] int year, [FromQuery] int month) {
            try {
                var results = await facade.GetCalendarEntriesAsync(year, month).ConfigureAwait(false);
                var models = results.Select(x => mapper.Map(x)).ToList();
                return Ok(models);
            } catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }
    }
}
