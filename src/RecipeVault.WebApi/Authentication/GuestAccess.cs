using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RecipeVault.WebApi.Authentication {
    /// <summary>
    /// Helpers for guest (Supabase anonymous sign-in) users.
    /// Anonymous users get a normal "authenticated" JWT with an "is_anonymous": true claim.
    /// </summary>
    public static class GuestAccess {
        /// <summary>
        /// Supabase JWT claim set to true for anonymous users
        /// </summary>
        public const string IsAnonymousClaim = "is_anonymous";

        /// <summary>
        /// ProblemDetails type returned when a guest calls an endpoint that needs a full account
        /// </summary>
        public const string RegistrationRequiredType = "urn:recipevault:registration-required";

        /// <summary>
        /// ProblemDetails type returned when a guest has used up a daily quota
        /// </summary>
        public const string GuestQuotaExceededType = "urn:recipevault:guest-quota-exceeded";

        /// <summary>
        /// True when the principal signed in anonymously (guest)
        /// </summary>
        public static bool IsGuest(this ClaimsPrincipal user) {
            var claim = user?.FindFirst(IsAnonymousClaim);
            return claim != null && string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 403 result telling the client a full account is required
        /// </summary>
        public static ObjectResult RegistrationRequired(string detail = "Create a free account to use this feature") {
            return new ObjectResult(new ProblemDetails {
                Type = RegistrationRequiredType,
                Title = "Registration Required",
                Detail = detail,
                Status = StatusCodes.Status403Forbidden
            }) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        /// <summary>
        /// Best-effort client IP. On Fly.io the proxy sets Fly-Client-IP (and overwrites any client value);
        /// elsewhere fall back to the connection address.
        /// </summary>
        public static string GetClientIp(HttpContext context) {
            var flyClientIp = context.Request.Headers["Fly-Client-IP"].ToString();
            if (!string.IsNullOrWhiteSpace(flyClientIp)) {
                return flyClientIp.Trim();
            }
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
