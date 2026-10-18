using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.IntegrationTests.Base;
using RecipeVault.WebApi.IntegrationTests.Helpers;
using RecipeVault.WebApi.Models.Requests;

namespace RecipeVault.WebApi.IntegrationTests.Endpoints {
    /// <summary>
    /// Guests are Supabase anonymous sign-ins: a normal JWT with "is_anonymous": true.
    /// </summary>
    public class GuestAccessEndpointTests : IAsyncLifetime, IDisposable {
        private const string GuestSubjectId = "7c9e6679-7425-40de-944b-e07fc1f90ae7";
        private readonly IntegrationFixture _fixture;
        private string _guestToken;
        private string _userToken;
        private bool _disposed;

        public GuestAccessEndpointTests() {
            _fixture = new IntegrationFixture();
        }

        public async Task InitializeAsync() {
            await _fixture.InitializeAsync();
            _guestToken = AuthenticationHelper.GenerateTestJwt(GuestSubjectId, new Dictionary<string, object> { { "is_anonymous", "true" } });
            _userToken = AuthenticationHelper.GenerateTestJwt();
        }

        public async Task DisposeAsync() {
            await _fixture.DisposeAsync();
        }

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token, object body = null) {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body != null) {
                request.Content = JsonContent.Create(body);
            }
            return _fixture.HttpClient.SendAsync(request);
        }

        [Fact]
        public async Task Guest_CanCreateAndListPrivateRecipes() {
            var create = await SendAsync(HttpMethod.Post, "api/v1/recipes", _guestToken, new UpdateRecipeModel { Title = "Guest Recipe", Yield = 2 });
            create.StatusCode.ShouldBe(HttpStatusCode.Created);

            var list = await SendAsync(HttpMethod.Get, "api/v1/recipes", _guestToken);
            list.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Guest_CannotCreatePublicRecipe() {
            var response = await SendAsync(HttpMethod.Post, "api/v1/recipes", _guestToken, new UpdateRecipeModel { Title = "Public Guest Recipe", Yield = 2, IsPublic = true });

            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            // The UI keys off the camelCase "type" property
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            body.RootElement.GetProperty("type").GetString().ShouldBe("urn:recipevault:registration-required");
        }

        [Fact]
        public async Task Guest_CannotGenerateShareToken() {
            var create = await SendAsync(HttpMethod.Post, "api/v1/recipes", _guestToken, new UpdateRecipeModel { Title = "Guest Recipe", Yield = 2 });
            create.StatusCode.ShouldBe(HttpStatusCode.Created);
            var location = create.Headers.Location.ToString();
            var id = location.Substring(location.LastIndexOf('/') + 1);

            var response = await SendAsync(HttpMethod.Post, $"api/v1/recipes/{id}/share-token", _guestToken);

            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Guest_CannotManageApiTokens() {
            var response = await SendAsync(HttpMethod.Get, "api/v1/user/api-tokens", _guestToken);

            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RegisteredUser_CanManageApiTokens() {
            var response = await SendAsync(HttpMethod.Get, "api/v1/user/api-tokens", _userToken);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _fixture?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
