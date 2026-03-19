using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using Serilog.Context;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Manage API tokens for browser extension authentication
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/user/api-tokens")]
    [Authorize]
    public class ApiTokensController : ControllerBase {
        private readonly IApiTokenService apiTokenService;
        private readonly IUnitOfWork uow;
        private readonly ApiTokenModelMapper mapper;
        private readonly ILogger<ApiTokensController> logger;

        /// <summary>
        /// Initializes a new instance of the ApiTokensController
        /// </summary>
        public ApiTokensController(IApiTokenService apiTokenService, IUnitOfWork uow, ApiTokenModelMapper mapper, ILogger<ApiTokensController> logger) {
            this.apiTokenService = apiTokenService;
            this.uow = uow;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Create a new API token
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiTokenCreatedModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTokenAsync([FromBody] CreateApiTokenModel model) {
            var dto = await apiTokenService.CreateTokenAsync(model.Name, model.ExpiresInDays).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return Ok(mapper.Map(dto));
        }

        /// <summary>
        /// List all API tokens for the current user
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiTokenListModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListTokensAsync() {
            var dtos = await apiTokenService.ListTokensAsync().ConfigureAwait(false);
            return Ok(mapper.MapToList(dtos));
        }

        /// <summary>
        /// Revoke an API token
        /// </summary>
        /// <param name="id">the resource id of the token to revoke</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RevokeTokenAsync(Guid id) {
            using (LogContext.PushProperty("ApiTokenResourceId", id)) {
                await apiTokenService.RevokeTokenAsync(id).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }
    }
}
