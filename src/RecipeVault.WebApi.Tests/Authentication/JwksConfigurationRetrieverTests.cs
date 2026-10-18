using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.Authentication;

namespace RecipeVault.WebApi.Tests.Authentication {
    public class JwksConfigurationRetrieverTests {
        private const string JwksUrl = "https://example.supabase.co/auth/v1/.well-known/jwks.json";
        private const string Issuer = "https://example.supabase.co/auth/v1";

        [Fact]
        public async Task GetConfigurationAsync_ReturnsTheJwksSigningKeysAndIssuer() {
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(ecdsa) { KeyId = "test-kid" });
            jwk.Alg = "ES256";
            jwk.Use = "sig";
            var jwksJson = JsonSerializer.Serialize(new {
                keys = new[] { new { kty = jwk.Kty, crv = jwk.Crv, x = jwk.X, y = jwk.Y, kid = jwk.Kid, alg = jwk.Alg, use = jwk.Use } }
            });

            var documents = new Mock<IDocumentRetriever>();
            documents.Setup(d => d.GetDocumentAsync(JwksUrl, It.IsAny<CancellationToken>())).ReturnsAsync(jwksJson);

            var configuration = await new JwksConfigurationRetriever(Issuer)
                .GetConfigurationAsync(JwksUrl, documents.Object, CancellationToken.None);

            configuration.Issuer.ShouldBe(Issuer);
            configuration.JwksUri.ShouldBe(JwksUrl);
            configuration.SigningKeys.Count.ShouldBe(1);
            configuration.SigningKeys.ShouldAllBe(k => k.KeyId == "test-kid");
        }
    }
}
