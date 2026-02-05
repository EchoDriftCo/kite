using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Unit normalization endpoints
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/units")]
    [Authorize]
    public class UnitsController : ControllerBase {
        private readonly IUnitFacade facade;
        private readonly UnitModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the UnitsController
        /// </summary>
        public UnitsController(IUnitFacade facade, UnitModelMapper mapper) {
            this.facade = facade;
            this.mapper = mapper;
        }

        /// <summary>
        /// Gets all available units
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(List<UnitModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync() {
            var units = await facade.GetAllAsync().ConfigureAwait(false);
            return Ok(mapper.Map(units));
        }

        /// <summary>
        /// Gets a unit by id
        /// </summary>
        /// <param name="id">the resource id of the unit to get</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UnitModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id) {
            var unit = await facade.GetByIdAsync(id).ConfigureAwait(false);
            if (unit == null) {
                return NotFound();
            }
            return Ok(mapper.Map(unit));
        }

        /// <summary>
        /// Match a unit string to a canonical unit
        /// </summary>
        /// <param name="request">the unit string to match</param>
        [HttpPost("match")]
        [ProducesResponseType(typeof(UnitMatchModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> MatchAsync([FromBody] UnitMatchRequestModel request) {
            var result = await facade.MatchAsync(request.Input).ConfigureAwait(false);
            return Ok(mapper.Map(result));
        }
    }
}
