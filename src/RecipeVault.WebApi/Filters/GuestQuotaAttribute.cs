using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.WebApi.Authentication;

namespace RecipeVault.WebApi.Filters {
    /// <summary>
    /// Daily per-IP quota for guest (anonymous sign-in) users. Registered users are not affected.
    /// Guests can mint new user ids at will, so per-user limits don't hold for them; the quota is keyed by client IP.
    /// Limits come from config "GuestQuota:{bucket}" (requests per UTC day), falling back to the default given here.
    /// Counts are kept in memory, so they are per app instance and reset on restart.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class GuestQuotaAttribute : Attribute, IAsyncActionFilter {
        /// <summary>
        /// Bucket for endpoints that call Gemini
        /// </summary>
        public const string Ai = "Ai";

        /// <summary>
        /// Bucket for saves and uploads (recipe saves also run AI dietary tagging)
        /// </summary>
        public const string Save = "Save";

        // Serializes check-and-increment so concurrent requests can't both pass at limit - 1
        private static readonly object QuotaLock = new object();

        private readonly string bucket;
        private readonly int defaultLimit;

        public GuestQuotaAttribute(string bucket, int defaultLimit) {
            this.bucket = bucket;
            this.defaultLimit = defaultLimit;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var httpContext = context.HttpContext;
            if (!httpContext.User.IsGuest()) {
                await next().ConfigureAwait(false);
                return;
            }

            var cache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var configuration = httpContext.RequestServices.GetService<IConfiguration>();
            var limit = configuration?.GetValue<int?>($"GuestQuota:{bucket}") ?? defaultLimit;

            var cacheKey = $"guest_quota_{bucket}_{GuestAccess.GetClientIp(httpContext)}_{DateTime.UtcNow:yyyy-MM-dd}";
            bool allowed;
            lock (QuotaLock) {
                var count = cache.Get<int>(cacheKey);
                allowed = count < limit;
                if (allowed) {
                    cache.Set(cacheKey, count + 1, TimeSpan.FromDays(1));
                }
            }

            if (!allowed) {
                context.Result = new ObjectResult(new ProblemDetails {
                    Type = GuestAccess.GuestQuotaExceededType,
                    Title = "Guest Limit Reached",
                    Detail = "You've reached today's guest limit for this feature. Create a free account to keep going.",
                    Status = StatusCodes.Status429TooManyRequests
                }) {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };
                return;
            }

            await next().ConfigureAwait(false);
        }
    }
}
