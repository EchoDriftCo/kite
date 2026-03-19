using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService.Tests.Services {
    public class RecipeServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private static Recipe BuildRecipeWithOwner(RecipeBuilder builder = null) {
            builder ??= new RecipeBuilder();
            var recipe = builder.Build();
            recipe.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");
            return recipe;
        }

        private RecipeService CreateService(Mock<IRecipeRepository> mockRepository, Mock<ITagRepository> mockTagRepository, Mock<IGeminiClient> mockGeminiClient, Mock<ISubjectPrincipal> mockSubjectPrincipal) {
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockHttpClientFactory = MockRepository.Create<IHttpClientFactory>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockCookingModeService = MockRepository.Create<ICookingModeService>();
            return new RecipeService(mockRepository.Object, mockTagRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockHttpClientFactory.Object, mockImageStorage.Object, mockCookingModeService.Object);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithValidDto_CreatesAndReturnsRecipe() {
            // Arrange
            var dto = new UpdateRecipeDtoBuilder().Build();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(dto.Title);
            result.Yield.ShouldBe(dto.Yield);
            result.Description.ShouldBe(dto.Description);
            result.RecipeResourceId.ShouldNotBe(Guid.Empty);

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithIngredients_AddsRecipeWithIngredients() {
            // Arrange
            var ingredients = new List<UpdateRecipeIngredientDto>
            {
                new UpdateRecipeIngredientDto { SortOrder = 1, Quantity = 2, Unit = "cups", Item = "flour", RawText = "2 cups flour" },
                new UpdateRecipeIngredientDto { SortOrder = 2, Quantity = 1, Unit = "cup", Item = "sugar", RawText = "1 cup sugar" }
            };

            var dto = new UpdateRecipeDtoBuilder()
                .WithIngredients(ingredients)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.Ingredients.Count.ShouldBe(2);
            result.Ingredients[0].Item.ShouldBe("flour");
            result.Ingredients[1].Item.ShouldBe("sugar");

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithInstructions_AddsRecipeWithInstructions() {
            // Arrange
            var instructions = new List<UpdateRecipeInstructionDto>
            {
                new UpdateRecipeInstructionDto { StepNumber = 1, Instruction = "Preheat oven", RawText = "Preheat oven" },
                new UpdateRecipeInstructionDto { StepNumber = 2, Instruction = "Mix ingredients", RawText = "Mix ingredients" }
            };

            var dto = new UpdateRecipeDtoBuilder()
                .WithInstructions(instructions)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.Instructions.Count.ShouldBe(2);
            result.Instructions[0].Instruction.ShouldBe("Preheat oven");
            result.Instructions[1].Instruction.ShouldBe("Mix ingredients");

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ReturnsRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.GetRecipeAsync(recipe.RecipeResourceId);

            // Assert
            result.ShouldBe(recipe);
            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithInvalidId_ThrowsNotFoundException() {
            // Arrange
            var invalidId = Guid.NewGuid();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetAsync(invalidId))
                .ReturnsAsync((Recipe)null)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            var ex = await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GetRecipeAsync(invalidId)
            );

            ex.Message.ShouldContain(invalidId.ToString());
            mockRepository.Verify(x => x.GetAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(false).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GetRecipeAsync(recipe.RecipeResourceId)
            );
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithValidData_UpdatesAndReturnsRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner(new RecipeBuilder()
                .WithTitle("Original Title")
                .WithYield(4));

            var updateDto = new UpdateRecipeDtoBuilder()
                .WithTitle("Updated Title")
                .WithYield(8)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto);

            // Assert
            result.Title.ShouldBe("Updated Title");
            result.Yield.ShouldBe(8);

            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidId_DeletesRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockRepository
                .Setup(x => x.RemoveAsync(recipe))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.DeleteRecipeAsync(recipe.RecipeResourceId);

            // Assert
            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
            mockRepository.Verify(x => x.RemoveAsync(recipe), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithPublicRecipeFromOtherUser_ReturnsRecipe() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.GetRecipeAsync(recipe.RecipeResourceId);

            // Assert
            result.ShouldBe(recipe);
            result.IsPublic.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithOtherUsersPublicRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var updateDto = new UpdateRecipeDtoBuilder().Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto)
            );
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithOtherUsersPublicRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.DeleteRecipeAsync(recipe.RecipeResourceId)
            );
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithOwnRecipe_SetsVisibility() {
            // Arrange
            var recipe = BuildRecipeWithOwner(new RecipeBuilder().WithIsPublic(false));
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true);

            // Assert
            recipe.IsPublic.ShouldBeTrue();
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true)
            );
        }

        [Fact]
        public async Task SearchRecipesAsync_WithValidSearch_ReturnsPagedResults() {
            // Arrange
            var recipes = new List<Recipe>
            {
                new RecipeBuilder().WithTitle("Recipe 1").Build(),
                new RecipeBuilder().WithTitle("Recipe 2").Build()
            };

            var pagedList = new PagedList<Recipe>
            {
                Items = recipes,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var search = new RecipeSearch { PageNumber = 1, PageSize = 10 };

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.SearchAsync(search))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.SearchRecipesAsync(search);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.TotalItems.ShouldBe(2);

            mockRepository.Verify(x => x.SearchAsync(search), Times.Once);
        }

        [Fact]
        public async Task SetRecipeRatingAsync_WithOwnRecipe_SetsRating() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.SetRecipeRatingAsync(recipe.RecipeResourceId, 4);

            // Assert
            recipe.Rating.ShouldBe(4);
        }

        [Fact]
        public async Task SetRecipeRatingAsync_WithNull_ClearsRating() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            recipe.SetRating(3);

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.SetRecipeRatingAsync(recipe.RecipeResourceId, null);

            // Assert
            recipe.Rating.ShouldBeNull();
        }

        [Fact]
        public async Task SetRecipeRatingAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.SetRecipeRatingAsync(recipe.RecipeResourceId, 4)
            );
        }

        [Fact]
        public async Task SetRecipeFavoriteAsync_WithOwnRecipe_SetsFavorite() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.SetRecipeFavoriteAsync(recipe.RecipeResourceId, true);

            // Assert
            recipe.IsFavorite.ShouldBeTrue();
        }

        [Fact]
        public async Task SetRecipeFavoriteAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.SetRecipeFavoriteAsync(recipe.RecipeResourceId, true)
            );
        }

        [Fact]
        public async Task GenerateShareTokenAsync_WithOwnRecipe_GeneratesToken() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.GenerateShareTokenAsync(recipe.RecipeResourceId);

            // Assert
            recipe.ShareToken.ShouldNotBeNullOrWhiteSpace();
            recipe.ShareToken.Length.ShouldBe(10);
        }

        [Fact]
        public async Task GenerateShareTokenAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GenerateShareTokenAsync(recipe.RecipeResourceId)
            );
        }

        [Fact]
        public async Task RevokeShareTokenAsync_WithOwnRecipe_RevokesToken() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            recipe.GenerateShareToken();
            recipe.ShareToken.ShouldNotBeNull();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.RevokeShareTokenAsync(recipe.RecipeResourceId);

            // Assert
            recipe.ShareToken.ShouldBeNull();
        }

        [Fact]
        public async Task GetRecipeByShareTokenAsync_WithValidToken_ReturnsRecipe() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            recipe.GenerateShareToken();
            var token = recipe.ShareToken;

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetByShareTokenAsync(token))
                .ReturnsAsync(recipe);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.GetRecipeByShareTokenAsync(token);

            // Assert
            result.ShouldBe(recipe);
        }

        [Fact]
        public async Task GetRecipeByShareTokenAsync_WithInvalidToken_ThrowsNotFoundException() {
            // Arrange
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetByShareTokenAsync("invalidtoken"))
                .ReturnsAsync((Recipe)null);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GetRecipeByShareTokenAsync("invalidtoken")
            );
        }

        [Fact]
        public async Task AssignTagsToRecipeAsync_WithNewTagByName_CreatesTagAndAssignsWithEntityReference() {
            // Arrange
            // This test covers the bug where creating a new tag and assigning it in the same
            // operation would fail because TagId=0 (identity not yet assigned).
            // The fix uses the Tag entity reference so EF Core handles insert ordering.
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            // Tag doesn't exist yet
            mockTagRepository
                .Setup(x => x.GetByNameAndCategoryAsync("New Tag", Domain.Enums.TagCategory.Custom))
                .ReturnsAsync((Tag)null);

            Tag capturedTag = null;
            mockTagRepository
                .Setup(x => x.AddAsync(It.IsAny<Tag>()))
                .Callback<Tag>(t => capturedTag = t)
                .ReturnsAsync((Tag t) => t);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            var tags = new List<AssignTagDto> {
                new AssignTagDto { Name = "New Tag", Category = (int)Domain.Enums.TagCategory.Custom }
            };

            // Act
            var result = await service.AssignTagsToRecipeAsync(recipe.RecipeResourceId, tags);

            // Assert
            result.RecipeTags.Count.ShouldBe(1);
            var recipeTag = result.RecipeTags[0];

            // The critical assertion: RecipeTag should have the Tag entity reference,
            // not just TagId (which would be 0 for a new unsaved tag)
            recipeTag.Tag.ShouldNotBeNull();
            recipeTag.Tag.ShouldBe(capturedTag);
            recipeTag.Tag.Name.ShouldBe("New Tag");

            mockTagRepository.Verify(x => x.AddAsync(It.IsAny<Tag>()), Times.Once);
        }

        [Fact]
        public async Task AssignTagsToRecipeAsync_WithExistingTag_AssignsWithTagId() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var existingTag = new TagBuilder().WithName("Existing Tag").Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            mockTagRepository
                .Setup(x => x.GetAsync(existingTag.TagResourceId))
                .ReturnsAsync(existingTag);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            var tags = new List<AssignTagDto> {
                new AssignTagDto { TagResourceId = existingTag.TagResourceId }
            };

            // Act
            var result = await service.AssignTagsToRecipeAsync(recipe.RecipeResourceId, tags);

            // Assert
            result.RecipeTags.Count.ShouldBe(1);
            var recipeTag = result.RecipeTags[0];
            recipeTag.TagId.ShouldBe(existingTag.TagId);

            // Existing tags don't need a new tag created
            mockTagRepository.Verify(x => x.AddAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task AssignTagsToRecipeAsync_WithNonExistentTagResourceId_ThrowsNotFoundException() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var nonExistentTagId = Guid.NewGuid();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            mockTagRepository
                .Setup(x => x.GetAsync(nonExistentTagId))
                .ReturnsAsync((Tag)null);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            var tags = new List<AssignTagDto> {
                new AssignTagDto { TagResourceId = nonExistentTagId }
            };

            // Act & Assert
            await Should.ThrowAsync<TagNotFoundException>(
                () => service.AssignTagsToRecipeAsync(recipe.RecipeResourceId, tags)
            );
        }

        [Fact]
        public async Task CreateRecipeAsync_WithOnlySourceImageUrl_DoesNotSetDisplayImage() {
            // Arrange
            // When importing from a recipe card/document photo, ONLY SourceImageUrl should be set,
            // NOT the display image (OriginalImageUrl). The source image is for preserving the original document.
            var dto = new UpdateRecipeDtoBuilder()
                .WithOriginalImageUrl(null)
                .Build();
            dto.SourceImageUrl = "https://storage.example.com/recipe-card-photo.jpg";

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.OriginalImageUrl.ShouldBeNull(); // Display image should NOT be set
            result.SourceImageUrl.ShouldBe("https://storage.example.com/recipe-card-photo.jpg"); // Source image should be set

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithOnlyDisplayImage_SetsOnlyDisplayImage() {
            // Arrange
            var dto = new UpdateRecipeDtoBuilder()
                .WithOriginalImageUrl("https://example.com/hero-image.jpg")
                .Build();
            dto.SourceImageUrl = null;

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.OriginalImageUrl.ShouldBe("https://example.com/hero-image.jpg");
            result.SourceImageUrl.ShouldBeNull();

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithBothDisplayAndSourceImages_SetsBothCorrectly() {
            // Arrange
            var dto = new UpdateRecipeDtoBuilder()
                .WithOriginalImageUrl("https://example.com/hero-image.jpg")
                .Build();
            dto.SourceImageUrl = "https://storage.example.com/recipe-card-photo.jpg";

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.OriginalImageUrl.ShouldBe("https://example.com/hero-image.jpg");
            result.SourceImageUrl.ShouldBe("https://storage.example.com/recipe-card-photo.jpg");

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task ForkRecipeAsync_WithPublicRecipe_CreatesForkSuccessfully() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .WithIsPublic(true)
                .Build();
            original.CreatedSubject = new Subject(Guid.NewGuid(), "Original Owner", "Original", "Owner", "owner@example.com");

            var ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredientBuilder().WithItem("flour").WithSortOrder(1).Build()
            };
            original.SetIngredients(ingredients);

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(true);

            mockRepository
                .Setup(x => x.GetAsync(original.RecipeResourceId))
                .ReturnsAsync(original)
                .Verifiable();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var fork = await service.ForkRecipeAsync(original.RecipeResourceId);

            // Assert
            fork.ShouldNotBeNull();
            fork.RecipeResourceId.ShouldNotBe(original.RecipeResourceId);
            fork.Title.ShouldBe("Original Recipe (Copy)");
            fork.ForkedFromRecipeId.ShouldBe(original.RecipeId);
            fork.Ingredients.Count.ShouldBe(1);
            fork.Ingredients[0].Item.ShouldBe("flour");

            mockRepository.Verify(x => x.GetAsync(original.RecipeResourceId), Times.Once);
            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task ForkRecipeAsync_WithOwnRecipe_CreatesForkSuccessfully() {
            // Arrange
            var original = BuildRecipeWithOwner(new RecipeBuilder()
                .WithTitle("My Recipe")
                .WithIsPublic(false));

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(true);

            mockRepository
                .Setup(x => x.GetAsync(original.RecipeResourceId))
                .ReturnsAsync(original)
                .Verifiable();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var fork = await service.ForkRecipeAsync(original.RecipeResourceId);

            // Assert
            fork.ShouldNotBeNull();
            fork.ForkedFromRecipeId.ShouldBe(original.RecipeId);
            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task ForkRecipeAsync_WithCustomTitle_UsesTitleProvided() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .WithIsPublic(true)
                .Build();
            original.CreatedSubject = new Subject(Guid.NewGuid(), "Owner", "Owner", "Name", "owner@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(true);

            mockRepository
                .Setup(x => x.GetAsync(original.RecipeResourceId))
                .ReturnsAsync(original)
                .Verifiable();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var fork = await service.ForkRecipeAsync(original.RecipeResourceId, "My Custom Fork");

            // Assert
            fork.Title.ShouldBe("My Custom Fork");
        }

        [Fact]
        public async Task ForkRecipeAsync_WithNonexistentRecipe_ThrowsRecipeNotFoundException() {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(true);

            mockRepository
                .Setup(x => x.GetAsync(nonExistentId))
                .ReturnsAsync((Recipe)null)
                .Verifiable();

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(async () =>
                await service.ForkRecipeAsync(nonExistentId)
            );
        }

        [Fact]
        public async Task ParseRecipeImageAsync_WithHtmlPayload_ParsesUsingGeminiTextAndExtractsOgImage() {
            // Arrange
            const string html = "<html><head><meta property=\"og:image\" content=\"https://example.com/recipe.jpg\" /></head><body><h1>Best Pancakes</h1><p>2 cups flour</p></body></html>";

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockGeminiClient
                .Setup(x => x.ParseRecipeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeminiParseResponse {
                    Confidence = 0.91,
                    Title = "Best Pancakes",
                    Ingredients = new List<GeminiIngredient> {
                        new GeminiIngredient { Item = "flour", RawText = "2 cups flour" }
                    },
                    Instructions = new List<GeminiInstruction> {
                        new GeminiInstruction { StepNumber = 1, Instruction = "Mix and cook", RawText = "Mix and cook" }
                    }
                });

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            var result = await service.ParseRecipeImageAsync(new ParseRecipeRequestDto {
                Url = "https://example.com/pancakes",
                Html = html
            });

            // Assert
            result.ShouldNotBeNull();
            result.Parsed.ShouldNotBeNull();
            result.Parsed.Title.ShouldBe("Best Pancakes");
            result.Parsed.ImageUrl.ShouldBe("https://example.com/recipe.jpg");
            mockGeminiClient.Verify(x => x.ParseRecipeTextAsync(It.Is<string>(s => s.Contains("Best Pancakes")), It.IsAny<CancellationToken>()), Times.Once);
            mockGeminiClient.Verify(x => x.ParseRecipeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ForkRecipeAsync_WithPublicRecipe_IncrementsForkCount() {
            // Arrange
            var original = BuildRecipeWithOwner();
            var forkerSubjectId = Guid.NewGuid();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagRepository = MockRepository.Create<ITagRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockSubjectPrincipal = MockRepository.Create<ISubjectPrincipal>();
            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns(forkerSubjectId.ToString());

            mockRepository
                .Setup(x => x.GetAsync(original.RecipeResourceId))
                .ReturnsAsync(original);
            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRepository, mockTagRepository, mockGeminiClient, mockSubjectPrincipal);

            // Act
            await service.ForkRecipeAsync(original.RecipeResourceId);

            // Assert
            original.ForkCount.ShouldBe(1);
        }

        [Fact]
        public void IncrementForkCount_CalledMultipleTimes_IncrementsCorrectly() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.IncrementForkCount();
            recipe.IncrementForkCount();
            recipe.IncrementForkCount();

            // Assert
            recipe.ForkCount.ShouldBe(3);
        }
    }
}
