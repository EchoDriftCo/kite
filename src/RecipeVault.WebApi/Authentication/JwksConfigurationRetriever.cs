using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace RecipeVault.WebApi.Authentication {
    /// <summary>
    /// Builds the JWT bearer configuration from a bare JWKS document rather than an OpenID
    /// discovery document. Supabase's discovery document advertises URLs that aren't reachable
    /// from inside Docker, so the JWKS is fetched from a known-reachable address instead.
    /// Used with ConfigurationManager, which caches the result and refreshes it.
    /// </summary>
    public sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration> {
        private readonly string issuer;

        /// <summary>
        /// Creates a retriever that stamps the given issuer on the configuration it builds
        /// </summary>
        public JwksConfigurationRetriever(string issuer) {
            this.issuer = issuer;
        }

        /// <summary>
        /// Fetches the JWKS at <paramref name="address"/> and returns its signing keys
        /// </summary>
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel) {
            ArgumentNullException.ThrowIfNull(retriever);
            var json = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);

            var configuration = new OpenIdConnectConfiguration { Issuer = issuer, JwksUri = address };
            foreach (var key in new JsonWebKeySet(json).GetSigningKeys()) {
                configuration.SigningKeys.Add(key);
            }
            return configuration;
        }
    }
}
