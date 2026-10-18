using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecipeVault.WebApi.Authentication;

namespace RecipeVault.WebApi.Filters {
    /// <summary>
    /// Rejects requests authenticated with an API token (rv_...) with 403; only a Supabase sign-in passes.
    /// API tokens have no scopes, so without this a leaked token could mint replacement tokens
    /// that outlive its own revocation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SessionOnlyAttribute : Attribute, IAsyncActionFilter {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            if (context.HttpContext.User.FindFirst(ApiTokenAuthenticationHandler.AuthMethodClaim)?.Value == ApiTokenAuthenticationHandler.AuthMethodApiToken) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Title = "Forbidden",
                    Detail = "API tokens can't manage API tokens. Sign in to the app instead.",
                    Status = StatusCodes.Status403Forbidden
                }) {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next().ConfigureAwait(false);
        }
    }
}
