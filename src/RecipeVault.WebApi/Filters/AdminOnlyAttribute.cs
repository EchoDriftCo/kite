using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Cortside.Common.Security;

namespace RecipeVault.WebApi.Filters {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminOnlyAttribute : Attribute, IAsyncActionFilter {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var subjectPrincipal = context.HttpContext.RequestServices.GetService(typeof(ISubjectPrincipal)) as ISubjectPrincipal;
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;

            if (string.IsNullOrEmpty(subjectPrincipal?.SubjectId)) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Title = "Unauthorized",
                    Detail = "Authentication is required",
                    Status = StatusCodes.Status401Unauthorized
                }) {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var adminSubjectIds = configuration.GetSection("AdminSubjectIds").Get<string[]>() ?? Array.Empty<string>();
            var isAdmin = adminSubjectIds.Contains(subjectPrincipal.SubjectId, StringComparer.OrdinalIgnoreCase);

            if (!isAdmin) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Title = "Forbidden",
                    Detail = "This endpoint requires administrator privileges",
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
