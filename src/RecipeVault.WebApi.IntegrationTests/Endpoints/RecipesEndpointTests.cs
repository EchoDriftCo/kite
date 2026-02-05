using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.WebApi.IntegrationTests.Base;
using RecipeVault.WebApi.IntegrationTests.Helpers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.IntegrationTests.Endpoints {
    public class RecipesEndpointTests : IAsyncLifetime, IDisposable {
        private readonly IntegrationFixture _fixture;
        private string _authToken;
        private bool _disposed;

        public RecipesEndpointTests() {
            _fixture = new IntegrationFixture();
        }

        public async Task InitializeAsync() {
            await _fixture.InitializeAsync();
            _authToken = AuthenticationHelper.GenerateTestJwt();
        }

        public async Task DisposeAsync() {
            await _fixture.DisposeAsync();
        }

        [Fact]
        public async Task GetRecipes_WithAuthentication_ReturnsOkWithEmptyList() {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/recipes");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<object>();
            content.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateRecipe_WithValidInput_ReturnsCreatedWithLocation() {
            // Arrange
            var input = new UpdateRecipeModel
            {
                Title = "Integration Test Recipe",
                Yield = 4,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 30,
                Description = "A recipe created by integration test",
                Source = "Test Kitchen",
                OriginalImageUrl = "https://example.com/recipe.jpg"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/recipes")
            {
                Content = JsonContent.Create(input)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldNotBeNull();

            var content = await response.Content.ReadFromJsonAsync<RecipeModel>();
            content.ShouldNotBeNull();
            content.Title.ShouldBe(input.Title);
            content.Yield.ShouldBe(input.Yield);
            content.RecipeResourceId.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task GetRecipeById_WithExistingRecipe_ReturnsOkWithRecipe() {
            // Arrange
            var recipe = new RecipeBuilder().WithTitle("Test Recipe for Retrieval").Build();
            await _fixture.AddToDbAsync(recipe);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/recipes/{recipe.RecipeResourceId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<RecipeModel>();
            content.ShouldNotBeNull();
            content.RecipeResourceId.ShouldBe(recipe.RecipeResourceId);
            content.Title.ShouldBe(recipe.Title);
        }

        [Fact]
        public async Task UpdateRecipe_WithValidData_ReturnsOkWithUpdatedRecipe() {
            // Arrange
            var recipe = new RecipeBuilder().WithTitle("Original Title").Build();
            await _fixture.AddToDbAsync(recipe);

            var updateInput = new UpdateRecipeModel
            {
                Title = "Updated Title",
                Yield = 8,
                Description = "Updated description"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/recipes/{recipe.RecipeResourceId}")
            {
                Content = JsonContent.Create(updateInput)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<RecipeModel>();
            content.ShouldNotBeNull();
            content.Title.ShouldBe(updateInput.Title);
            content.Yield.ShouldBe(updateInput.Yield);
            content.Description.ShouldBe(updateInput.Description);
        }

        [Fact]
        public async Task DeleteRecipe_WithExistingRecipe_ReturnsNoContent() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            await _fixture.AddToDbAsync(recipe);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/recipes/{recipe.RecipeResourceId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetRecipe_WithoutAuthentication_ReturnsUnauthorized() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            await _fixture.AddToDbAsync(recipe);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/recipes/{recipe.RecipeResourceId}");
            // No authorization header

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetNonexistentRecipe_ReturnsNotFound() {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/recipes/{nonexistentId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _fixture?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
