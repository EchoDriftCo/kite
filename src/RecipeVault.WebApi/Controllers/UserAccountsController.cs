using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeVault.Facade;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// User account and tier management
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/account")]
    [Authorize]
    public class UserAccountsController : ControllerBase {
        private readonly IUserAccountFacade facade;
        private readonly UserAccountModelMapper mapper;
        private readonly ILogger<UserAccountsController> logger;

        /// <summary>
        /// Initializes a new instance of the UserAccountsController
        /// </summary>
        public UserAccountsController(IUserAccountFacade facade, UserAccountModelMapper mapper, ILogger<UserAccountsController> logger) {
            this.facade = facade;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Get current user's account and tier information
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(UserAccountModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountAsync() {
            try {
                var dto = await facade.GetCurrentAccountAsync().ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            } catch (Exception ex) {
                logger.LogError(ex, "Error getting user account");
                throw;
            }
        }

        /// <summary>
        /// Set a user's account tier (admin only)
        /// </summary>
        /// <param name="input">The tier to set</param>
        [HttpPut("tier")]
        [Filters.AdminOnly]
        [ProducesResponseType(typeof(UserAccountModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetTierAsync([FromBody] SetTierModel input) {
            try {
                var dto = await facade.SetTierAsync(input.Tier).ConfigureAwait(false);
                return Ok(mapper.Map(dto));
            } catch (ArgumentException ex) {
                return BadRequest(new ProblemDetails {
                    Title = "Invalid tier",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            } catch (Exception ex) {
                logger.LogError(ex, "Error setting account tier");
                throw;
            }
        }
    }
}
