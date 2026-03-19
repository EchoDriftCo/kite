using System;
using System.Collections.Generic;
using System.Linq;
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
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class MealPlanServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private static MealPlan BuildMealPlanWithOwner(MealPlanBuilder builder = null) {
            builder ??= new MealPlanBuilder();
            var mealPlan = builder.Build();
            mealPlan.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");
            return mealPlan;
        }

        [Fact]
        public async Task CreateMealPlanAsync_WithValidDto_CreatesAndReturnsMealPlan() {
            // Arrange
            var dto = new UpdateMealPlanDto {
                Name = "Weekly Plan",
                StartDate = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc)
            };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockMealPlanRepo
                .Setup(x => x.AddAsync(It.IsAny<MealPlan>()))
                .ReturnsAsync((MealPlan mp) => mp)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.CreateMealPlanAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(dto.Name);
            result.StartDate.ShouldBe(dto.StartDate);
            result.EndDate.ShouldBe(dto.EndDate);
            result.MealPlanResourceId.ShouldNotBe(Guid.Empty);

            mockMealPlanRepo.Verify(x => x.AddAsync(It.IsAny<MealPlan>()), Times.Once);
        }

        [Fact]
        public async Task GetMealPlanAsync_WithValidId_ReturnsMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner();
            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.GetMealPlanAsync(mealPlan.MealPlanResourceId);

            // Assert
            result.ShouldBe(mealPlan);
        }

        [Fact]
        public async Task GetMealPlanAsync_WithOtherUsersPlan_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var mealPlan = new MealPlanBuilder().Build();
            mealPlan.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act & Assert
            await Should.ThrowAsync<MealPlanNotFoundException>(
                () => service.GetMealPlanAsync(mealPlan.MealPlanResourceId)
            );
        }

        [Fact]
        public async Task DeleteMealPlanAsync_WithValidId_DeletesMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner();
            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            mockMealPlanRepo
                .Setup(x => x.RemoveAsync(mealPlan))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            await service.DeleteMealPlanAsync(mealPlan.MealPlanResourceId);

            // Assert
            mockMealPlanRepo.Verify(x => x.RemoveAsync(mealPlan), Times.Once);
        }

        [Fact]
        public async Task UpdateMealPlanAsync_WithValidData_UpdatesAndReturnsMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner(new MealPlanBuilder().WithName("Original"));
            var updateDto = new UpdateMealPlanDto {
                Name = "Updated Plan",
                StartDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc)
            };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.UpdateMealPlanAsync(mealPlan.MealPlanResourceId, updateDto);

            // Assert
            result.Name.ShouldBe("Updated Plan");
            result.StartDate.ShouldBe(updateDto.StartDate);
        }

        [Fact]
        public async Task SearchMealPlansAsync_WithValidSearch_ReturnsPagedResults() {
            // Arrange
            var mealPlans = new List<MealPlan> {
                new MealPlanBuilder().WithName("Plan 1").Build(),
                new MealPlanBuilder().WithName("Plan 2").Build()
            };

            var pagedList = new PagedList<MealPlan> {
                Items = mealPlans,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var search = new MealPlanSearch { PageNumber = 1, PageSize = 10 };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockMealPlanRepo
                .Setup(x => x.SearchAsync(search))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.SearchMealPlansAsync(search);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.TotalItems.ShouldBe(2);
        }

        // Helper to set the private Recipe navigation property on MealPlanEntry via reflection
        private static void SetEntryRecipe(MealPlanEntry entry, Recipe recipe) {
            typeof(MealPlanEntry).GetProperty("Recipe").SetValue(entry, recipe);
        }

        private MealPlanService CreateServiceForGroceryTests(
            MealPlan mealPlan,
            out Mock<IGeminiClient> mockGeminiClient) {

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);
            mockGeminiClient = MockRepository.Create<IGeminiClient>();

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            return new MealPlanService(
                mockMealPlanRepo.Object,
                mockRecipeRepo.Object,
                mockLogger.Object,
                mockSubjectPrincipal.Object,
                mockGeminiClient.Object);
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithMultipleRecipes_AggregatesIngredients() {
            // Arrange — two recipes both use "flour" with the same unit, quantities should sum
            var recipe1 = new RecipeBuilder()
                .WithTitle("Banana Bread")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("flour").WithQuantity(2m).WithUnit("cup").Build(),
                    new RecipeIngredientBuilder().WithItem("sugar").WithQuantity(0.5m).WithUnit("cup").Build()
                })
                .Build();

            var recipe2 = new RecipeBuilder()
                .WithTitle("Pizza Dough")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("flour").WithQuantity(3m).WithUnit("cup").Build(),
                    new RecipeIngredientBuilder().WithItem("yeast").WithQuantity(1m).WithUnit("tsp").Build()
                })
                .Build();

            var entry1 = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 0, null, false);
            SetEntryRecipe(entry1, recipe1);
            var entry2 = new MealPlanEntry(DateTime.UtcNow, MealSlot.Lunch, 0, null, false);
            SetEntryRecipe(entry2, recipe2);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry1, entry2 }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            // Gemini returns consolidated items
            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GeminiGroceryItem> items, CancellationToken _) => new GeminiGroceryConsolidationResponse {
                    Items = items.Select(i => new GeminiConsolidatedItem {
                        Item = i.Item,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Category = "Pantry",
                        Sources = i.Sources
                    }).ToList()
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldNotBeEmpty();

            // Flour should have been aggregated to 5 cups before being sent to Gemini
            mockGeminiClient.Verify(x => x.ConsolidateGroceryListAsync(
                It.Is<List<GeminiGroceryItem>>(items =>
                    items.Any(i => i.Item == "flour" && i.Quantity == 5m && i.Unit == "cup")),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithMultipleRecipes_TracksRecipeSources() {
            // Arrange — flour appears in both recipes, sources should contain both titles
            var recipe1 = new RecipeBuilder()
                .WithTitle("Banana Bread")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("flour").WithQuantity(2m).WithUnit("cup").Build()
                })
                .Build();

            var recipe2 = new RecipeBuilder()
                .WithTitle("Pizza Dough")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("flour").WithQuantity(3m).WithUnit("cup").Build()
                })
                .Build();

            var entry1 = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 0, null, false);
            SetEntryRecipe(entry1, recipe1);
            var entry2 = new MealPlanEntry(DateTime.UtcNow, MealSlot.Lunch, 0, null, false);
            SetEntryRecipe(entry2, recipe2);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry1, entry2 }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GeminiGroceryItem> items, CancellationToken _) => new GeminiGroceryConsolidationResponse {
                    Items = items.Select(i => new GeminiConsolidatedItem {
                        Item = i.Item,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Category = "Pantry",
                        Sources = i.Sources
                    }).ToList()
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — flour's sources should include both recipe titles
            mockGeminiClient.Verify(x => x.ConsolidateGroceryListAsync(
                It.Is<List<GeminiGroceryItem>>(items =>
                    items.Any(i => i.Item == "flour" &&
                                   i.Sources.Contains("Banana Bread") &&
                                   i.Sources.Contains("Pizza Dough"))),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithLeftoverEntries_ExcludesLeftovers() {
            // Arrange — one regular entry and one leftover, leftover should be excluded
            var recipe1 = new RecipeBuilder()
                .WithTitle("Pasta")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("pasta").WithQuantity(1m).WithUnit("lb").Build()
                })
                .Build();

            var recipe2 = new RecipeBuilder()
                .WithTitle("Pasta Leftovers")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("extra sauce").WithQuantity(1m).WithUnit("cup").Build()
                })
                .Build();

            var entry1 = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 0, null, false);
            SetEntryRecipe(entry1, recipe1);
            var leftoverEntry = new MealPlanEntry(DateTime.UtcNow.AddDays(1), MealSlot.Lunch, 0, null, true);
            SetEntryRecipe(leftoverEntry, recipe2);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry1, leftoverEntry }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GeminiGroceryItem> items, CancellationToken _) => new GeminiGroceryConsolidationResponse {
                    Items = items.Select(i => new GeminiConsolidatedItem {
                        Item = i.Item, Quantity = i.Quantity, Unit = i.Unit,
                        Category = "Pantry", Sources = i.Sources
                    }).ToList()
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — only "pasta" should be in the list, not "extra sauce"
            mockGeminiClient.Verify(x => x.ConsolidateGroceryListAsync(
                It.Is<List<GeminiGroceryItem>>(items =>
                    items.Count == 1 && items[0].Item == "pasta"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithServingsScaling_ScalesQuantities() {
            // Arrange — recipe yields 4 but meal plan entry requests 8 servings (2x scale)
            var recipe = new RecipeBuilder()
                .WithTitle("Stew")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("potatoes").WithQuantity(2m).WithUnit("lb").Build()
                })
                .Build();

            var entry = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 0, 8, false);
            SetEntryRecipe(entry, recipe);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GeminiGroceryItem> items, CancellationToken _) => new GeminiGroceryConsolidationResponse {
                    Items = items.Select(i => new GeminiConsolidatedItem {
                        Item = i.Item, Quantity = i.Quantity, Unit = i.Unit,
                        Category = "Produce", Sources = i.Sources
                    }).ToList()
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — potatoes should be scaled from 2 lb to 4 lb (8 servings / 4 yield = 2x)
            mockGeminiClient.Verify(x => x.ConsolidateGroceryListAsync(
                It.Is<List<GeminiGroceryItem>>(items =>
                    items.Any(i => i.Item == "potatoes" && i.Quantity == 4m)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WhenGeminiFails_ReturnsFallbackWithOtherCategory() {
            // Arrange
            var recipe = new RecipeBuilder()
                .WithTitle("Salad")
                .WithYield(2)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("lettuce").WithQuantity(1m).WithUnit("head").Build(),
                    new RecipeIngredientBuilder().WithItem("tomato").WithQuantity(2m).WithUnit("piece").Build()
                })
                .Build();

            var entry = new MealPlanEntry(DateTime.UtcNow, MealSlot.Lunch, 0, null, false);
            SetEntryRecipe(entry, recipe);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            // Gemini fails
            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Gemini unavailable"));

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — fallback returns raw items with "Other" category and sources preserved
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.Items.ShouldAllBe(i => i.Category == "Other");
            result.Items.ShouldAllBe(i => i.Sources.Contains("Salad"));
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithConsolidation_MapsCategoryAndSources() {
            // Arrange
            var recipe = new RecipeBuilder()
                .WithTitle("Smoothie")
                .WithYield(1)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("banana").WithQuantity(2m).WithUnit("piece").Build(),
                    new RecipeIngredientBuilder().WithItem("milk").WithQuantity(1m).WithUnit("cup").Build()
                })
                .Build();

            var entry = new MealPlanEntry(DateTime.UtcNow, MealSlot.Breakfast, 0, null, false);
            SetEntryRecipe(entry, recipe);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            // Gemini returns items with categories
            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeminiGroceryConsolidationResponse {
                    Items = new List<GeminiConsolidatedItem> {
                        new GeminiConsolidatedItem { Item = "banana", Quantity = 2m, Unit = "piece", Category = "Produce", Sources = new List<string> { "Smoothie" } },
                        new GeminiConsolidatedItem { Item = "milk", Quantity = 1m, Unit = "cup", Category = "Dairy", Sources = new List<string> { "Smoothie" } }
                    }
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — categories and sources from Gemini should be mapped to the DTO
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);

            var banana = result.Items.First(i => i.Item == "banana");
            banana.Category.ShouldBe("Produce");
            banana.Sources.ShouldContain("Smoothie");

            var milk = result.Items.First(i => i.Item == "milk");
            milk.Category.ShouldBe("Dairy");
            milk.Sources.ShouldContain("Smoothie");
        }

        [Fact]
        public async Task GenerateGroceryListAsync_WithNullCategory_DefaultsToOther() {
            // Arrange
            var recipe = new RecipeBuilder()
                .WithTitle("Mystery Recipe")
                .WithYield(4)
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder().WithItem("mystery ingredient").WithQuantity(1m).WithUnit("cup").Build()
                })
                .Build();

            var entry = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 0, null, false);
            SetEntryRecipe(entry, recipe);

            var mealPlan = BuildMealPlanWithOwner(
                new MealPlanBuilder().WithEntries(new List<MealPlanEntry> { entry }));

            var service = CreateServiceForGroceryTests(mealPlan, out var mockGeminiClient);

            // Gemini returns null category
            mockGeminiClient
                .Setup(x => x.ConsolidateGroceryListAsync(It.IsAny<List<GeminiGroceryItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeminiGroceryConsolidationResponse {
                    Items = new List<GeminiConsolidatedItem> {
                        new GeminiConsolidatedItem { Item = "mystery ingredient", Quantity = 1m, Unit = "cup", Category = null, Sources = null }
                    }
                });

            // Act
            var result = await service.GenerateGroceryListAsync(mealPlan.MealPlanResourceId);

            // Assert — null category defaults to "Other", null sources defaults to empty list
            result.Items[0].Category.ShouldBe("Other");
            result.Items[0].Sources.ShouldNotBeNull();
            result.Items[0].Sources.ShouldBeEmpty();
        }
    }
}
