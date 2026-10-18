using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.IntegrationTests.Base;
using RecipeVault.WebApi.IntegrationTests.Helpers;

namespace RecipeVault.WebApi.IntegrationTests.Endpoints {
    /// <summary>
    /// Behaviour of the Startup pipeline itself: rate limiting, token subject validation and
    /// the liveness endpoint. Each test uses its own subject id, because the rate limiter's
    /// counters live for the lifetime of the shared test server.
    /// </summary>
    public sealed class StartupPipelineTests : IAsyncLifetime, IDisposable {
        private const string ImportUrl = "api/v1/import/url";
        private readonly IntegrationFixture _fixture = new IntegrationFixture();

        public Task InitializeAsync() => _fixture.InitializeAsync();

        public Task DisposeAsync() => _fixture.DisposeAsync();

        public void Dispose() => _fixture.Dispose();

        private Task<HttpResponseMessage> PostAsync(string path, string token) {
            var request = new HttpRequestMessage(HttpMethod.Post, path) {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _fixture.HttpClient.SendAsync(request);
        }

        private async Task<List<HttpStatusCode>> PostRepeatedlyAsync(string path, string token, int times) {
            var codes = new List<HttpStatusCode>();
            for (var i = 0; i < times; i++) {
                using var response = await PostAsync(path, token);
                codes.Add(response.StatusCode);
            }
            return codes;
        }

        [Fact]
        public async Task UploadPolicy_IsEnforced_TheEleventhRequestInAMinuteGets429() {
            var token = AuthenticationHelper.GenerateTestJwt(Guid.NewGuid().ToString());

            var codes = await PostRepeatedlyAsync(ImportUrl, token, 11);

            codes.GetRange(0, 10).ShouldNotContain(HttpStatusCode.TooManyRequests);
            codes[10].ShouldBe(HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task UploadPolicy_IsPerUser_OneUserCannotExhaustAnother() {
            var first = AuthenticationHelper.GenerateTestJwt(Guid.NewGuid().ToString());
            var second = AuthenticationHelper.GenerateTestJwt(Guid.NewGuid().ToString());

            (await PostRepeatedlyAsync(ImportUrl, first, 11))[10].ShouldBe(HttpStatusCode.TooManyRequests);

            using var response = await PostAsync(ImportUrl, second);
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task GenerateEndpoint_RateLimitPolicyResolves() {
            // It used a policy name that was never registered; with the limiter enforcing
            // policies, that made every request a 500
            var token = AuthenticationHelper.GenerateTestJwt(Guid.NewGuid().ToString());

            using var response = await PostAsync("api/v1/recipes/generate", token);

            response.StatusCode.ShouldNotBe(HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("not-a-guid")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task Token_WhoseSubIsNotAUserId_IsRejected(string sub) {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/recipes");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationHelper.GenerateTestJwt(sub));

            using var response = await _fixture.HttpClient.SendAsync(request);

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task LivenessEndpoint_AnswersWithoutAuthentication() {
            using var response = await _fixture.HttpClient.GetAsync("health/live");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).ShouldBe("ok");
        }
    }
}
