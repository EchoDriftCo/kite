using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using RecipeVault.WebApi.Authentication;

namespace RecipeVault.WebApi.RateLimiting {
    /// <summary>
    /// Rate limit policy names for [EnableRateLimiting], and how callers are partitioned.
    /// Use these constants rather than string literals: a name with no registered policy
    /// makes every request to that endpoint fail with a 500.
    /// </summary>
    public static class RateLimitPolicies {
        /// <summary>
        /// General API traffic: 60 requests per minute per caller
        /// </summary>
        public const string Api = "api";

        /// <summary>
        /// Uploads and AI calls (import, image upload, mix, generate): 10 requests per minute per caller
        /// </summary>
        public const string Upload = "upload";

        /// <summary>
        /// A fixed one-minute window with its own counter for each caller
        /// </summary>
        public static RateLimitPartition<string> FixedWindow(HttpContext context, int permitLimit) {
            return RateLimitPartition.GetFixedWindowLimiter(PartitionKey(context), _ => new FixedWindowRateLimiterOptions {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
        }

        /// <summary>
        /// Registered users are limited per user. Guests are limited per client IP instead,
        /// because a guest can mint a fresh user id at will; anonymous requests likewise.
        /// </summary>
        public static string PartitionKey(HttpContext context) {
            ArgumentNullException.ThrowIfNull(context);
            var user = context.User;
            var subject = user?.FindFirst("sub")?.Value;
            if (user?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(subject) && !user.IsGuest()) {
                return "user:" + subject;
            }
            return "ip:" + GuestAccess.GetClientIp(context);
        }
    }
}
