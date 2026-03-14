using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeVault.Data.Repositories;

namespace RecipeVault.WebApi.Authentication {
    public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
        private readonly IApiTokenRepository apiTokenRepository;

        public ApiTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IApiTokenRepository apiTokenRepository)
            : base(options, logger, encoder) {
            this.apiTokenRepository = apiTokenRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader)) {
                return AuthenticateResult.NoResult();
            }

            var headerValue = authHeader.ToString();
            if (!headerValue.StartsWith("Bearer rv_", StringComparison.Ordinal)) {
                return AuthenticateResult.NoResult();
            }

            var token = headerValue["Bearer ".Length..];
            var hash = ComputeSha256(token);
            var apiToken = await apiTokenRepository.GetByHashAsync(hash)
                .ConfigureAwait(false);

            if (apiToken == null || !apiToken.IsValid()) {
                return AuthenticateResult.Fail("Invalid or expired API token");
            }

            apiToken.MarkUsed();

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, apiToken.SubjectId.ToString()),
                new Claim("sub", apiToken.SubjectId.ToString()),
                new Claim("token_id", apiToken.ApiTokenResourceId.ToString()),
                new Claim("auth_method", "api_token")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private static string ComputeSha256(string input) {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
