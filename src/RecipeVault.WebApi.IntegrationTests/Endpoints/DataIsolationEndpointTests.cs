using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using RecipeVault.Data;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.WebApi.IntegrationTests.Base;
using RecipeVault.WebApi.IntegrationTests.Helpers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.IntegrationTests.Endpoints {
    /// <summary>
    /// Section 1 of the audit: one user must not be able to read or change another user's data
    /// by passing its id. Alice owns the data; Bob tries to reach it.
    /// </summary>
    public class DataIsolationEndpointTests : IAsyncLifetime, IDisposable {
        private const string BobSubjectId = "3f2b8c1e-9d4a-4e6b-8f1c-2a7d5e9b0c13";
        private readonly IntegrationFixture _fixture;
        private string _aliceToken;
        private string _bobToken;
        private bool _disposed;

        public DataIsolationEndpointTests() {
            _fixture = new IntegrationFixture();
        }

        public async Task InitializeAsync() {
            await _fixture.InitializeAsync();
            _aliceToken = AuthenticationHelper.GenerateTestJwt();
            _bobToken = AuthenticationHelper.GenerateTestJwt(BobSubjectId);
        }

        public async Task DisposeAsync() {
            await _fixture.DisposeAsync();
        }

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token, object body = null) {
            var request = new HttpRequestMessage(method, url);
            if (token != null) {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            if (body != null) {
                request.Content = JsonContent.Create(body);
            }
            return _fixture.HttpClient.SendAsync(request);
        }

        private async Task<RecipeModel> CreateRecipeAsync(string token, string title, bool isPublic) {
            var response = await SendAsync(HttpMethod.Post, "api/v1/recipes", token, new UpdateRecipeModel {
                Title = title,
                Yield = 2,
                IsPublic = isPublic,
                Ingredients = new List<UpdateRecipeIngredientModel> {
                    new UpdateRecipeIngredientModel { SortOrder = 0, Quantity = 2, Unit = "cup", Item = "secret flour", RawText = "2 cup secret flour" }
                }
            });
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            return await response.Content.ReadFromJsonAsync<RecipeModel>();
        }

        private static async Task<string> BodyAsync(HttpResponseMessage response) {
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Denied means no 2xx and none of Alice's data in the body. Most endpoints answer 404;
        /// a few catch every exception and answer 500 (an error-handling issue, not a leak).
        /// </summary>
        private static async Task ShouldBeDeniedAsync(Task<HttpResponseMessage> send) {
            var response = await send;
            response.IsSuccessStatusCode.ShouldBeFalse($"{response.RequestMessage.Method} {response.RequestMessage.RequestUri} returned {(int)response.StatusCode}");
            (await BodyAsync(response)).ShouldNotContain("secret flour");
        }

        // --- A1: nutrition endpoints check neither ownership nor visibility ---

        [Fact]
        public async Task Nutrition_CannotOverrideIngredientNutritionOnAnotherUsersPublicRecipe() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Public Bread", isPublic: true);

            var response = await SendAsync(HttpMethod.Put, $"api/v1/recipes/{recipe.RecipeResourceId}/ingredients/0/nutrition", _bobToken,
                new UpdateIngredientNutritionDto { MatchedFoodName = "vandalised", MatchConfidence = 1m });

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Nutrition_CannotOverrideIngredientNutritionOnAnotherUsersPrivateRecipe() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Private Bread", isPublic: false);

            var response = await SendAsync(HttpMethod.Put, $"api/v1/recipes/{recipe.RecipeResourceId}/ingredients/0/nutrition", _bobToken,
                new UpdateIngredientNutritionDto { MatchedFoodName = "vandalised", MatchConfidence = 1m });

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Nutrition_OwnerCanStillOverrideIngredientNutrition() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Own Bread", isPublic: false);

            var response = await SendAsync(HttpMethod.Put, $"api/v1/recipes/{recipe.RecipeResourceId}/ingredients/0/nutrition", _aliceToken,
                new UpdateIngredientNutritionDto { MatchedFoodName = "flour", MatchConfidence = 1m });

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Nutrition_CannotReadAnotherUsersPrivateRecipeNutrition() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Private Stew", isPublic: false);
            using (var scope = _fixture.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<RecipeVaultDbContext>();
                var recipeId = db.Recipes.Single(r => r.RecipeResourceId == recipe.RecipeResourceId).RecipeId;
                var nutrition = new RecipeNutrition(recipeId);
                nutrition.SetPerServingValues(640m, 30m, 50m, 20m, 5m, 3m, 900m);
                db.RecipeNutritions.Add(nutrition);
                await db.SaveChangesAsync();
            }

            var asAlice = await SendAsync(HttpMethod.Get, $"api/v1/recipes/{recipe.RecipeResourceId}/nutrition", _aliceToken);
            asAlice.StatusCode.ShouldBe(HttpStatusCode.OK);

            var asBob = await SendAsync(HttpMethod.Get, $"api/v1/recipes/{recipe.RecipeResourceId}/nutrition", _bobToken);
            asBob.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // --- A2: meal plan entries accept any recipe id ---

        [Fact]
        public async Task MealPlan_CannotAddAnotherUsersPrivateRecipe() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Secret Sauce", isPublic: false);

            var response = await SendAsync(HttpMethod.Post, "api/v1/meal-plans", _bobToken, new UpdateMealPlanModel {
                Name = "Bob's week",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                Entries = new List<UpdateMealPlanEntryModel> {
                    new UpdateMealPlanEntryModel { Date = DateTime.UtcNow.Date, MealSlot = 2, RecipeResourceId = recipe.RecipeResourceId, Servings = 2 }
                }
            });

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await BodyAsync(response)).ShouldNotContain("Alice Secret Sauce");
        }

        [Fact]
        public async Task MealPlan_CanStillAddAnotherUsersPublicRecipe() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Public Soup", isPublic: true);

            var response = await SendAsync(HttpMethod.Post, "api/v1/meal-plans", _bobToken, new UpdateMealPlanModel {
                Name = "Bob's week",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                Entries = new List<UpdateMealPlanEntryModel> {
                    new UpdateMealPlanEntryModel { Date = DateTime.UtcNow.Date, MealSlot = 2, RecipeResourceId = recipe.RecipeResourceId, Servings = 2 }
                }
            });

            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // --- A3: the dietary check reads any recipe ---

        [Fact]
        public async Task DietaryCheck_CannotCheckAnotherUsersPrivateRecipe() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Private Curry", isPublic: false);
            var profile = await SendAsync(HttpMethod.Post, "api/v1/dietary-profiles", _bobToken,
                new UpdateDietaryProfileModel { ProfileName = "Bob", IsDefault = true });
            profile.StatusCode.ShouldBe(HttpStatusCode.Created);

            var response = await SendAsync(HttpMethod.Get, $"api/v1/recipes/{recipe.RecipeResourceId}/dietary-check", _bobToken);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // --- A4: public collections list private recipes ---

        [Fact]
        public async Task PublicCollection_DoesNotListPrivateRecipes() {
            var privateRecipe = await CreateRecipeAsync(_aliceToken, "Alice Hidden Pie", isPublic: false);
            var publicRecipe = await CreateRecipeAsync(_aliceToken, "Alice Shown Pie", isPublic: true);
            var create = await SendAsync(HttpMethod.Post, "api/v1/collections", _aliceToken,
                new UpdateCollectionModel { Name = "Alice's pies", IsPublic = true });
            create.StatusCode.ShouldBe(HttpStatusCode.Created);
            var collection = await create.Content.ReadFromJsonAsync<CollectionModel>();
            foreach (var recipe in new[] { privateRecipe, publicRecipe }) {
                var add = await SendAsync(HttpMethod.Post, $"api/v1/collections/{collection.CollectionResourceId}/recipes", _aliceToken,
                    new AddRecipeToCollectionModel { RecipeResourceId = recipe.RecipeResourceId });
                add.StatusCode.ShouldBe(HttpStatusCode.OK);
            }

            var anonymous = await SendAsync(HttpMethod.Get, "api/v1/collections/public", null);
            anonymous.StatusCode.ShouldBe(HttpStatusCode.OK);
            var anonymousBody = await BodyAsync(anonymous);
            anonymousBody.ShouldContain("Alice Shown Pie");
            anonymousBody.ShouldNotContain("Alice Hidden Pie");
            anonymousBody.ShouldNotContain(privateRecipe.RecipeResourceId.ToString());

            var asBob = await SendAsync(HttpMethod.Get, $"api/v1/collections/{collection.CollectionResourceId}", _bobToken);
            asBob.StatusCode.ShouldBe(HttpStatusCode.OK);
            var bobView = await asBob.Content.ReadFromJsonAsync<CollectionModel>();
            bobView.Recipes.ShouldHaveSingleItem().Title.ShouldBe("Alice Shown Pie");
            bobView.RecipeCount.ShouldBe(1);

            var asAlice = await SendAsync(HttpMethod.Get, $"api/v1/collections/{collection.CollectionResourceId}", _aliceToken);
            var aliceView = await asAlice.Content.ReadFromJsonAsync<CollectionModel>();
            aliceView.Recipes.Count.ShouldBe(2);
        }

        // --- A5: tag names resolve to other users' private tags ---

        [Fact]
        public async Task Tags_CreatingATagWithAnotherUsersTagName_GivesYouYourOwnTag() {
            var aliceCreate = await SendAsync(HttpMethod.Post, "api/v1/tags", _aliceToken, new UpdateTagModel { Name = "Weeknight", Category = 5 });
            aliceCreate.StatusCode.ShouldBe(HttpStatusCode.Created);
            var aliceTag = await aliceCreate.Content.ReadFromJsonAsync<TagModel>();

            var bobCreate = await SendAsync(HttpMethod.Post, "api/v1/tags", _bobToken, new UpdateTagModel { Name = "Weeknight", Category = 5 });
            bobCreate.StatusCode.ShouldBe(HttpStatusCode.Created);
            var bobTag = await bobCreate.Content.ReadFromJsonAsync<TagModel>();

            bobTag.TagResourceId.ShouldNotBe(aliceTag.TagResourceId);
        }

        [Fact]
        public async Task Tags_AssigningByName_DoesNotAttachAnotherUsersPrivateTag() {
            var aliceCreate = await SendAsync(HttpMethod.Post, "api/v1/tags", _aliceToken, new UpdateTagModel { Name = "Grandma", Category = 5 });
            var aliceTag = await aliceCreate.Content.ReadFromJsonAsync<TagModel>();
            var bobRecipe = await CreateRecipeAsync(_bobToken, "Bob Cookies", isPublic: false);

            var assign = await SendAsync(HttpMethod.Post, $"api/v1/recipes/{bobRecipe.RecipeResourceId}/tags", _bobToken,
                new AssignTagsModel { Tags = new List<AssignTagItemModel> { new AssignTagItemModel { Name = "Grandma", Category = 5 } } });

            assign.StatusCode.ShouldBe(HttpStatusCode.OK);
            var tagged = await assign.Content.ReadFromJsonAsync<RecipeModel>();
            tagged.Tags.ShouldHaveSingleItem().TagResourceId.ShouldNotBe(aliceTag.TagResourceId);
        }

        [Fact]
        public async Task Tags_CannotAssignAnotherUsersPrivateTagById() {
            var aliceCreate = await SendAsync(HttpMethod.Post, "api/v1/tags", _aliceToken, new UpdateTagModel { Name = "Alice Only", Category = 5 });
            var aliceTag = await aliceCreate.Content.ReadFromJsonAsync<TagModel>();
            var bobRecipe = await CreateRecipeAsync(_bobToken, "Bob Brownies", isPublic: false);

            var assign = await SendAsync(HttpMethod.Post, $"api/v1/recipes/{bobRecipe.RecipeResourceId}/tags", _bobToken,
                new AssignTagsModel { Tags = new List<AssignTagItemModel> { new AssignTagItemModel { TagResourceId = aliceTag.TagResourceId } } });

            assign.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Tags_CannotReadAnotherUsersPrivateTagById() {
            var aliceCreate = await SendAsync(HttpMethod.Post, "api/v1/tags", _aliceToken, new UpdateTagModel { Name = "Alice Diary", Category = 5 });
            var aliceTag = await aliceCreate.Content.ReadFromJsonAsync<TagModel>();

            var response = await SendAsync(HttpMethod.Get, $"api/v1/tags/{aliceTag.TagResourceId}", _bobToken);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // --- A6: share links don't open without signing in ---

        [Fact]
        public async Task ShareLink_OpensWithoutSigningIn() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Shared Tart", isPublic: false);
            var share = await SendAsync(HttpMethod.Post, $"api/v1/recipes/{recipe.RecipeResourceId}/share-token", _aliceToken);
            share.StatusCode.ShouldBe(HttpStatusCode.OK);
            var shared = await share.Content.ReadFromJsonAsync<RecipeModel>();
            shared.ShareToken.ShouldNotBeNullOrEmpty();

            var anonymous = await SendAsync(HttpMethod.Get, $"api/v1/recipes/shared/{shared.ShareToken}", null);

            anonymous.StatusCode.ShouldBe(HttpStatusCode.OK);
            var viewed = await anonymous.Content.ReadFromJsonAsync<RecipeModel>();
            viewed.Title.ShouldBe("Alice Shared Tart");
            viewed.ShareToken.ShouldBeNull();
            viewed.IsOwner.ShouldBeFalse();
        }

        [Fact]
        public async Task ShareLink_StopsWorkingWhenRevoked() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Revoked Tart", isPublic: false);
            var share = await SendAsync(HttpMethod.Post, $"api/v1/recipes/{recipe.RecipeResourceId}/share-token", _aliceToken);
            var token = (await share.Content.ReadFromJsonAsync<RecipeModel>()).ShareToken;
            var revoke = await SendAsync(HttpMethod.Delete, $"api/v1/recipes/{recipe.RecipeResourceId}/share-token", _aliceToken);
            revoke.IsSuccessStatusCode.ShouldBeTrue();

            var response = await SendAsync(HttpMethod.Get, $"api/v1/recipes/shared/{token}", _bobToken);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // --- A7: an API token can mint more API tokens ---

        [Fact]
        public async Task ApiToken_CannotBeUsedToManageApiTokens() {
            var create = await SendAsync(HttpMethod.Post, "api/v1/user/api-tokens", _aliceToken, new CreateApiTokenModel { Name = "extension" });
            create.StatusCode.ShouldBe(HttpStatusCode.OK);
            var apiToken = (await create.Content.ReadFromJsonAsync<ApiTokenCreatedModel>()).Token;

            // The token still works for normal API calls
            var recipes = await SendAsync(HttpMethod.Get, "api/v1/recipes", apiToken);
            recipes.StatusCode.ShouldBe(HttpStatusCode.OK);

            // ...but can't mint a replacement that would outlive its revocation
            var mint = await SendAsync(HttpMethod.Post, "api/v1/user/api-tokens", apiToken, new CreateApiTokenModel { Name = "persistence" });
            mint.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await BodyAsync(await SendAsync(HttpMethod.Get, "api/v1/user/api-tokens", _aliceToken))).ShouldNotContain("persistence");

            var list = await SendAsync(HttpMethod.Get, "api/v1/user/api-tokens", apiToken);
            list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        // --- Regression: the owner checks that already hold ---

        [Theory]
        [InlineData("GET", "api/v1/recipes/{0}")]
        [InlineData("PUT", "api/v1/recipes/{0}/favorite")]
        [InlineData("PUT", "api/v1/recipes/{0}/rating")]
        [InlineData("DELETE", "api/v1/recipes/{0}")]
        [InlineData("POST", "api/v1/recipes/{0}/share-token")]
        [InlineData("POST", "api/v1/recipes/{0}/fork")]
        [InlineData("GET", "api/v1/recipes/{0}/links")]
        [InlineData("GET", "api/v1/export/recipes/{0}/json")]
        [InlineData("GET", "api/v1/equipment/recipes/{0}")]
        [InlineData("POST", "api/v1/recipes/{0}/cooking-data")]
        public async Task OtherUsersPrivateRecipe_IsDenied(string method, string urlFormat) {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Private Recipe", isPublic: false);
            object body = method == "PUT" ? new { rating = 5, isFavorite = true } : method == "POST" ? new { } : null;

            await ShouldBeDeniedAsync(SendAsync(new HttpMethod(method), string.Format(System.Globalization.CultureInfo.InvariantCulture, urlFormat, recipe.RecipeResourceId), _bobToken, body));
        }

        [Fact]
        public async Task OtherUsersMealPlanCollectionAndCookingLog_AreDenied() {
            var recipe = await CreateRecipeAsync(_aliceToken, "Alice Lunch", isPublic: false);
            var plan = await SendAsync(HttpMethod.Post, "api/v1/meal-plans", _aliceToken, new UpdateMealPlanModel {
                Name = "Alice's week", StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(6)
            });
            plan.StatusCode.ShouldBe(HttpStatusCode.Created);
            var planId = JsonDocument.Parse(await BodyAsync(plan)).RootElement.GetProperty("mealPlanResourceId").GetString();
            var collection = await SendAsync(HttpMethod.Post, "api/v1/collections", _aliceToken, new UpdateCollectionModel { Name = "Alice private" });
            var collectionId = (await collection.Content.ReadFromJsonAsync<CollectionModel>()).CollectionResourceId;
            var log = await SendAsync(HttpMethod.Post, "api/v1/cooking-log", _aliceToken, new { recipeResourceId = recipe.RecipeResourceId, cookedDate = DateTime.UtcNow });
            log.StatusCode.ShouldBe(HttpStatusCode.Created);
            var logId = JsonDocument.Parse(await BodyAsync(log)).RootElement.GetProperty("cookingLogResourceId").GetString();

            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/meal-plans/{planId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Delete, $"api/v1/meal-plans/{planId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/collections/{collectionId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Delete, $"api/v1/collections/{collectionId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/cooking-log/{logId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Delete, $"api/v1/cooking-log/{logId}", _bobToken));
        }

        [Fact]
        public async Task OtherUsersApiTokenAndCircle_AreNotReachable() {
            var token = await SendAsync(HttpMethod.Post, "api/v1/user/api-tokens", _aliceToken, new CreateApiTokenModel { Name = "alice" });
            var tokenId = (await token.Content.ReadFromJsonAsync<ApiTokenCreatedModel>()).ApiTokenResourceId;
            var circle = await SendAsync(HttpMethod.Post, "api/v1/circles", _aliceToken, new { name = "Alice family" });
            circle.StatusCode.ShouldBe(HttpStatusCode.Created);
            var circleId = JsonDocument.Parse(await BodyAsync(circle)).RootElement.GetProperty("circleResourceId").GetString();

            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Delete, $"api/v1/user/api-tokens/{tokenId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/circles/{circleId}", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/circles/{circleId}/recipes", _bobToken));
            await ShouldBeDeniedAsync(SendAsync(HttpMethod.Get, $"api/v1/circles/{circleId}/members", _bobToken));
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
