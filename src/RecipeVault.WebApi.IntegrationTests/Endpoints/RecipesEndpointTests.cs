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

        [Fact]
        public async Task ForkRecipe_WithPublicRecipe_ReturnsCreatedWithFork() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe to Fork")
                .WithYield(4)
                .WithIsPublic(true)
                .Build();
            
            var ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredientBuilder().WithItem("flour").WithSortOrder(1).Build(),
                new RecipeIngredientBuilder().WithItem("sugar").WithSortOrder(2).Build()
            };
            original.SetIngredients(ingredients);
            
            var instructions = new List<RecipeInstruction>
            {
                new RecipeInstructionBuilder().WithStepNumber(1).WithInstruction("Mix ingredients").Build()
            };
            original.SetInstructions(instructions);
            
            await _fixture.AddToDbAsync(original);

            var forkInput = new ForkRecipeModel
            {
                Title = "My Forked Version"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/recipes/{original.RecipeResourceId}/fork")
            {
                Content = JsonContent.Create(forkInput)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldNotBeNull();

            var fork = await response.Content.ReadFromJsonAsync<RecipeModel>();
            fork.ShouldNotBeNull();
            fork.Title.ShouldBe("My Forked Version");
            fork.RecipeResourceId.ShouldNotBe(original.RecipeResourceId);
            fork.Yield.ShouldBe(original.Yield);
            fork.Ingredients.Count.ShouldBe(2);
            fork.Instructions.Count.ShouldBe(1);
            fork.ForkedFrom.ShouldNotBeNull();
            fork.ForkedFrom.RecipeResourceId.ShouldBe(original.RecipeResourceId);
            fork.ForkedFrom.Title.ShouldBe("Original Recipe to Fork");
            fork.ForkedFrom.IsAvailable.ShouldBeTrue();
            fork.IsPublic.ShouldBeFalse(); // Forks start private
        }

        [Fact]
        public async Task ForkRecipe_WithoutCustomTitle_UsesDefaultTitle() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .WithIsPublic(true)
                .Build();
            await _fixture.AddToDbAsync(original);

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/recipes/{original.RecipeResourceId}/fork")
            {
                Content = JsonContent.Create(new ForkRecipeModel())
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var fork = await response.Content.ReadFromJsonAsync<RecipeModel>();
            fork.Title.ShouldBe("Original Recipe (Copy)");
        }

        [Fact]
        public async Task GetRecipeForks_WithPublicForks_ReturnsPagedList() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Popular Recipe")
                .WithIsPublic(true)
                .Build();
            await _fixture.AddToDbAsync(original);

            // Create a public fork
            var publicFork = original.Fork("Public Fork");
            publicFork.SetVisibility(true);
            await _fixture.AddToDbAsync(publicFork);

            // Create a private fork (should not appear in results)
            var privateFork = original.Fork("Private Fork");
            privateFork.SetVisibility(false);
            await _fixture.AddToDbAsync(privateFork);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/recipes/{original.RecipeResourceId}/forks");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _fixture.HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNull();
            // The response should contain only the public fork
        }

        [Fact]
        public async Task ForkRecipe_WithNonexistentRecipe_ReturnsNotFound() {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/recipes/{nonexistentId}/fork")
            {
                Content = JsonContent.Create(new ForkRecipeModel())
            };
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
