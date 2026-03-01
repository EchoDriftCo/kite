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
        /// Create a new beta invite code
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(BetaInviteCodeModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCodeAsync([FromBody] CreateBetaInviteCodeModel input) {
            var dto = mapper.MapToDto(input);
            var result = await facade.CreateCodeAsync(dto).ConfigureAwait(false);
            return StatusCode(StatusCodes.Status201Created, mapper.Map(result));
        }

        /// <summary>
        /// List all beta invite codes
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedList<BetaInviteCodeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCodesAsync([FromQuery] BetaInviteCodeSearchModel search) {
            var searchDto = mapper.MapToDto(search);
            var results = await facade.SearchCodesAsync(searchDto).ConfigureAwait(false);
            var models = results.Convert(x => mapper.Map(x));
            return Ok(models);
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
        /// Redeem a beta invite code
        /// </summary>
        [HttpPost("redeem")]
        [ProducesResponseType(typeof(BetaInviteCodeModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RedeemCodeAsync([FromBody] RedeemInviteCodeModel input) {
            var dto = mapper.MapToDto(input);
            var result = await facade.RedeemCodeAsync(dto).ConfigureAwait(false);
            return Ok(mapper.Map(result));
        }
    }
}
