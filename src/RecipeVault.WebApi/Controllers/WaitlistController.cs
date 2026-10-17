using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Dto;
using RecipeVault.Facade;
using RecipeVault.WebApi.Filters;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Premium waitlist signup (public, no auth required)
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/waitlist")]
    public class WaitlistController : ControllerBase {
        private readonly IPremiumWaitlistFacade facade;

        public WaitlistController(IPremiumWaitlistFacade facade) {
            this.facade = facade;
        }

        /// <summary>
        /// Join the premium waitlist
        /// </summary>
        [HttpPost("")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(WaitlistResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WaitlistResponse>> JoinWaitlist([FromBody] JoinWaitlistModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var result = await facade.JoinWaitlistAsync(model.Email, model.Source);
            return Ok(new WaitlistResponse {
                Message = "You're on the list! We'll notify you when Premium launches.",
                ResourceId = result.PremiumWaitlistResourceId
            });
        }

        /// <summary>
        /// List all waitlist signups (Admin only)
        /// </summary>
        [HttpGet("")]
        [Authorize]
        [AdminOnly]
        [ProducesResponseType(typeof(List<PremiumWaitlistDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PremiumWaitlistDto>>> GetAll() {
            var results = await facade.GetAllAsync();
            return Ok(results);
        }
    }
}
