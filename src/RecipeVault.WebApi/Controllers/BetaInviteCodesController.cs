using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Facade;
using RecipeVault.WebApi.Filters;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Manages beta invite codes for closed beta registration
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/beta-invite-codes")]
    [Authorize]
    public class BetaInviteCodesController : ControllerBase {
        private readonly IBetaInviteCodeFacade facade;
        private readonly BetaInviteCodeModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the BetaInviteCodesController
        /// </summary>
        public BetaInviteCodesController(IBetaInviteCodeFacade facade, BetaInviteCodeModelMapper mapper) {
            this.facade = facade;
            this.mapper = mapper;
        }

        /// <summary>
        /// Generate new beta invite codes (Admin only)
        /// </summary>
        [HttpPost("")]
        [AdminOnly]
        [ProducesResponseType(typeof(List<BetaInviteCodeModel>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCodesAsync([FromBody] CreateBetaInviteCodeModel input) {
            var dto = mapper.MapToDto(input);
            var results = await facade.CreateCodesAsync(dto).ConfigureAwait(false);
            var models = results.Select(x => mapper.Map(x)).ToList();
            return StatusCode(StatusCodes.Status201Created, models);
        }

        /// <summary>
        /// List all beta invite codes (Admin only)
        /// </summary>
        [HttpGet("")]
        [AdminOnly]
        [ProducesResponseType(typeof(PagedList<BetaInviteCodeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCodesAsync([FromQuery] BetaInviteCodeSearchModel search) {
            var searchDto = mapper.MapToDto(search);
            var results = await facade.SearchCodesAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => mapper.Map(x));
            return Ok(models);
        }

        /// <summary>
        /// Deactivate a beta invite code (Admin only)
        /// </summary>
        [HttpDelete("{code}")]
        [AdminOnly]
        [ProducesResponseType(typeof(BetaInviteCodeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateCodeAsync(string code) {
            var result = await facade.DeactivateCodeAsync(code).ConfigureAwait(false);
            if (result == null) {
                return NotFound(new ProblemDetails {
                    Title = "Not Found",
                    Detail = $"Invite code '{code}' not found",
                    Status = StatusCodes.Status404NotFound
                });
            }
            return Ok(mapper.Map(result));
        }

        /// <summary>
        /// Validate a beta invite code without redeeming it
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ValidateInviteCodeResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateCodeAsync([FromBody] ValidateInviteCodeModel input) {
            var result = await facade.ValidateCodeAsync(input.Code).ConfigureAwait(false);
            return Ok(mapper.Map(result));
        }

        /// <summary>
        /// Redeem a beta invite code to upgrade account to Beta tier
        /// </summary>
        [HttpPost("redeem")]
        [ProducesResponseType(typeof(RedeemCodeResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RedeemCodeAsync([FromBody] RedeemInviteCodeModel input) {
            var dto = mapper.MapToDto(input);
            var result = await facade.RedeemCodeAsync(dto).ConfigureAwait(false);

            if (!result.Success) {
                return BadRequest(new ProblemDetails {
                    Title = "Redemption Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(mapper.Map(result));
        }
    }
}
