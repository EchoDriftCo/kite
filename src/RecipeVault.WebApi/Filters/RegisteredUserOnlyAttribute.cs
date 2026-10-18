using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using RecipeVault.WebApi.Authentication;

namespace RecipeVault.WebApi.Filters {
    /// <summary>
    /// Rejects guest (anonymous sign-in) users with 403 registration-required.
    /// Use on endpoints that publish content, send email, or manage the account.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RegisteredUserOnlyAttribute : Attribute, IAsyncActionFilter {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            if (context.HttpContext.User.IsGuest()) {
                context.Result = GuestAccess.RegistrationRequired();
                return;
            }

            await next().ConfigureAwait(false);
        }
    }
}
