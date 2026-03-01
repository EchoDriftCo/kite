using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;

namespace RecipeVault.WebApi.Filters {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresTierAttribute : Attribute, IAsyncActionFilter {
        private readonly AccountTier requiredTier;

        public RequiresTierAttribute(AccountTier requiredTier) {
            this.requiredTier = requiredTier;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var subjectPrincipal = context.HttpContext.RequestServices.GetRequiredService<ISubjectPrincipal>();
            var userAccountService = context.HttpContext.RequestServices.GetRequiredService<IUserAccountService>();
            var uow = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

            if (string.IsNullOrEmpty(subjectPrincipal.SubjectId)) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Title = "Unauthorized",
                    Detail = "Authentication is required",
                    Status = StatusCodes.Status401Unauthorized
                }) {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var account = await userAccountService.GetOrCreateAccountAsync(subjectId).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);

            if (!MeetsTierRequirement(account.AccountTier)) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Title = "Forbidden",
                    Detail = "This feature requires a premium account",
                    Status = StatusCodes.Status403Forbidden
                }) {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next().ConfigureAwait(false);
        }

        private bool MeetsTierRequirement(AccountTier userTier) {
            // Beta has access to everything
            if (userTier == AccountTier.Beta) {
                return true;
            }

            // Premium has access to Premium-required features
            if (requiredTier == AccountTier.Premium && userTier == AccountTier.Premium) {
                return true;
            }

            // Free has access only to Free-required features
            if (requiredTier == AccountTier.Free) {
                return true;
            }

            return false;
        }
    }
}
