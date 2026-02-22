using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Integrations.Gemini;
using RecipeVault.Integrations.Gemini.Models;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class SubstitutionServiceTests : DomainServiceTestBase {
        private static readonly Guid TestRecipeId = Guid.NewGuid();

        private SubstitutionService CreateService(
            Mock<IGeminiClient> mockGeminiClient,
            Mock<IRecipeService> mockRecipeService,
            Mock<ISubstitutionCacheService> mockCacheService) {
            var mockLogger = CreateMockLogger<SubstitutionService>();
            return new SubstitutionService(
                mockGeminiClient.Object,
                mockRecipeService.Object,
                mockCacheService.Object,
                mockLogger.Object);
        }

        private static Recipe BuildTestRecipe() {
            var recipe = new RecipeBuilder()
                .WithTitle("Test Recipe")
                .WithIngredients(new List<RecipeIngredient> {
                    new RecipeIngredientBuilder()
                        .WithSortOrder(0)
                        .WithQuantity(2m)
                        .WithUnit("cups")
                        .WithItem("all-purpose flour")
                        .Build(),
                    new RecipeIngredientBuilder()
                        .WithSortOrder(1)
                        .WithQuantity(1m)
                        .WithUnit("cup")
                        .WithItem("milk")
                        .Build(),
                    new RecipeIngredientBuilder()
                        .WithSortOrder(2)
                        .WithQuantity(2m)
                        .WithUnit("tbsp")
                        .WithItem("butter")
                        .Build()
                })
                .Build();
            return recipe;
        }

        private static GeminiSubstitutionAnalysis BuildTestGeminiAnalysis() {
            return new GeminiSubstitutionAnalysis {
                Analysis = "Gluten-free substitution analysis",
                Substitutions = new List<GeminiIngredientSubstitution> {
                    new GeminiIngredientSubstitution {
                        OriginalIndex = 0,
                        Original = "2 cups all-purpose flour",
                        Reason = "Contains gluten",
                        Options = new List<GeminiSubstitutionOption> {
                            new GeminiSubstitutionOption {
                                Name = "Gluten-free flour blend",
                                Ingredients = new List<GeminiSubstitutionIngredient> {
                                    new GeminiSubstitutionIngredient {
                                        Quantity = 2m,
                                        Unit = "cups",
                                        Item = "gluten-free flour blend"
                                    }
                                },
                                Notes = "1:1 substitution works well",
                                TechniqueAdjustments = "Add 1 tsp xanthan gum if blend doesn't include it"
                            }
                        }
                    }
                }
            };
        }

        #region GetSubstitutionsAsync Tests

        [Fact]
        public async Task GetSubstitutionsAsync_WithIngredientIndices_ReturnsSuggestions() {
            // Arrange
            var recipe = BuildTestRecipe();
            var geminiAnalysis = BuildTestGeminiAnalysis();
            var ingredientIndices = new List<int> { 0 };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            mockCacheService
                .Setup(x => x.BuildCacheKey(TestRecipeId, ingredientIndices, null))
                .Returns("substitution:test-cache-key");

            mockCacheService
                .Setup(x => x.GetAsync<SubstitutionResponseDto>("substitution:test-cache-key"))
                .ReturnsAsync((SubstitutionResponseDto)null);

            mockGeminiClient
                .Setup(x => x.AnalyzeSubstitutionsAsync(
                    recipe.Title,
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    ingredientIndices,
                    null,
                    default))
                .ReturnsAsync(geminiAnalysis);

            mockCacheService
                .Setup(x => x.SetAsync("substitution:test-cache-key", It.IsAny<SubstitutionResponseDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.GetSubstitutionsAsync(TestRecipeId, ingredientIndices, null);

            // Assert
            result.ShouldNotBeNull();
            result.Analysis.ShouldBe("Gluten-free substitution analysis");
            result.Substitutions.Count.ShouldBe(1);
            result.Substitutions[0].OriginalIndex.ShouldBe(0);
            result.Substitutions[0].OriginalText.ShouldBe("2 cups all-purpose flour");
            result.Substitutions[0].Options.Count.ShouldBe(1);
            result.Substitutions[0].Options[0].Name.ShouldBe("Gluten-free flour blend");
            result.Cached.ShouldBeFalse();

            mockGeminiClient.Verify(x => x.AnalyzeSubstitutionsAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                ingredientIndices,
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithDietaryConstraints_ReturnsSuggestions() {
            // Arrange
            var recipe = BuildTestRecipe();
            var geminiAnalysis = BuildTestGeminiAnalysis();
            var dietaryConstraints = new List<string> { "Gluten-Free" };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            mockCacheService
                .Setup(x => x.BuildCacheKey(TestRecipeId, null, dietaryConstraints))
                .Returns("substitution:test-cache-key-2");

            mockCacheService
                .Setup(x => x.GetAsync<SubstitutionResponseDto>("substitution:test-cache-key-2"))
                .ReturnsAsync((SubstitutionResponseDto)null);

            mockGeminiClient
                .Setup(x => x.AnalyzeSubstitutionsAsync(
                    recipe.Title,
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    null,
                    dietaryConstraints,
                    default))
                .ReturnsAsync(geminiAnalysis);

            mockCacheService
                .Setup(x => x.SetAsync("substitution:test-cache-key-2", It.IsAny<SubstitutionResponseDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.GetSubstitutionsAsync(TestRecipeId, null, dietaryConstraints);

            // Assert
            result.ShouldNotBeNull();
            result.Analysis.ShouldBe("Gluten-free substitution analysis");
            result.Substitutions.Count.ShouldBe(1);
            result.Cached.ShouldBeFalse();

            mockGeminiClient.Verify(x => x.AnalyzeSubstitutionsAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                null,
                dietaryConstraints,
                default), Times.Once);
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithNeitherIngredientNorConstraints_ThrowsArgumentException() {
            // Arrange
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.GetSubstitutionsAsync(TestRecipeId, null, null)
            );

            ex.Message.ShouldContain("Must provide at least one ingredient index or dietary constraint");
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithEmptyLists_ThrowsArgumentException() {
            // Arrange
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.GetSubstitutionsAsync(TestRecipeId, new List<int>(), new List<string>())
            );

            ex.Message.ShouldContain("Must provide at least one ingredient index or dietary constraint");
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithCachedResult_ReturnsCachedResponse() {
            // Arrange
            var ingredientIndices = new List<int> { 0 };
            var cachedResponse = new SubstitutionResponseDto {
                Analysis = "Cached analysis",
                Substitutions = new List<IngredientSubstitutionDto>(),
                Cached = false // Will be set to true by the service
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var recipe = BuildTestRecipe();
            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            mockCacheService
                .Setup(x => x.BuildCacheKey(TestRecipeId, ingredientIndices, null))
                .Returns("substitution:cached-key");

            mockCacheService
                .Setup(x => x.GetAsync<SubstitutionResponseDto>("substitution:cached-key"))
                .ReturnsAsync(cachedResponse);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.GetSubstitutionsAsync(TestRecipeId, ingredientIndices, null);

            // Assert
            result.ShouldNotBeNull();
            result.Cached.ShouldBeTrue(); // Service should set this to true
            result.Analysis.ShouldBe("Cached analysis");

            // Should not call Gemini when result is cached
            mockGeminiClient.Verify(x => x.AnalyzeSubstitutionsAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<string>>(),
                default), Times.Never);
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WhenNotInCache_CallsGemini() {
            // Arrange
            var recipe = BuildTestRecipe();
            var geminiAnalysis = BuildTestGeminiAnalysis();
            var ingredientIndices = new List<int> { 0 };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            mockCacheService
                .Setup(x => x.BuildCacheKey(TestRecipeId, ingredientIndices, null))
                .Returns("substitution:new-key");

            mockCacheService
                .Setup(x => x.GetAsync<SubstitutionResponseDto>("substitution:new-key"))
                .ReturnsAsync((SubstitutionResponseDto)null); // Not in cache

            mockGeminiClient
                .Setup(x => x.AnalyzeSubstitutionsAsync(
                    recipe.Title,
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    ingredientIndices,
                    null,
                    default))
                .ReturnsAsync(geminiAnalysis)
                .Verifiable();

            mockCacheService
                .Setup(x => x.SetAsync("substitution:new-key", It.IsAny<SubstitutionResponseDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.GetSubstitutionsAsync(TestRecipeId, ingredientIndices, null);

            // Assert
            result.ShouldNotBeNull();
            result.Cached.ShouldBeFalse();

            // Should call Gemini when not in cache
            mockGeminiClient.Verify(x => x.AnalyzeSubstitutionsAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                ingredientIndices,
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithInvalidIngredientIndex_ThrowsArgumentException() {
            // Arrange
            var recipe = BuildTestRecipe(); // Has 3 ingredients (0-2)
            var invalidIndices = new List<int> { 5 }; // Out of bounds

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.GetSubstitutionsAsync(TestRecipeId, invalidIndices, null)
            );

            ex.Message.ShouldContain("Invalid ingredient indices");
            ex.Message.ShouldContain("5");
        }

        [Fact]
        public async Task GetSubstitutionsAsync_WithRecipeWithNoIngredients_ThrowsInvalidOperationException() {
            // Arrange
            var recipe = new RecipeBuilder()
                .WithTitle("Empty Recipe")
                .WithIngredients(new List<RecipeIngredient>())
                .Build();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(recipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(
                () => service.GetSubstitutionsAsync(TestRecipeId, null, new List<string> { "Vegan" })
            );

            ex.Message.ShouldContain("Recipe has no ingredients to substitute");
        }

        #endregion

        #region ApplySubstitutionsAsync Tests

        [Fact]
        public async Task ApplySubstitutionsAsync_WithValidSelections_CreatesForkedRecipe() {
            // Arrange
            var originalRecipe = BuildTestRecipe();
            var forkedRecipe = originalRecipe.Fork(); // Use the Fork method to properly create a fork

            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 0,
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "Gluten-free flour blend",
                        Ingredients = new List<SubstitutionIngredientDto> {
                            new SubstitutionIngredientDto {
                                Quantity = 2m,
                                Unit = "cups",
                                Item = "gluten-free flour blend"
                            }
                        },
                        Notes = "Works well as 1:1 substitute"
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(originalRecipe);

            mockRecipeService
                .Setup(x => x.ForkRecipeAsync(TestRecipeId, null))
                .ReturnsAsync(forkedRecipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.ApplySubstitutionsAsync(TestRecipeId, selections);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeNull();
            result.Ingredients.Count.ShouldBe(3); // Original had 3, replaced 1st with 1 ingredient
            result.Ingredients[0].Item.ShouldBe("gluten-free flour blend");
            result.Description.ShouldContain("Substitutions Made:");
            result.Description.ShouldContain("2 cups all-purpose flour");
            result.Description.ShouldContain("gluten-free flour blend");

            mockRecipeService.Verify(x => x.ForkRecipeAsync(TestRecipeId, null), Times.Once);
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_AddsSubstitutionNoteToDescription() {
            // Arrange
            var originalRecipe = BuildTestRecipe();
            var forkedRecipe = new RecipeBuilder()
                .WithTitle("Test Recipe (Copy)")
                .WithDescription("Original description")
                .WithIngredients(originalRecipe.Ingredients.ToList())
                .Build();

            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 1,
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "Almond milk",
                        Ingredients = new List<SubstitutionIngredientDto> {
                            new SubstitutionIngredientDto {
                                Quantity = 1m,
                                Unit = "cup",
                                Item = "almond milk"
                            }
                        },
                        Notes = "Slightly nutty flavor",
                        TechniqueAdjustments = "May need to reduce sugar slightly"
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(originalRecipe);

            mockRecipeService
                .Setup(x => x.ForkRecipeAsync(TestRecipeId, null))
                .ReturnsAsync(forkedRecipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.ApplySubstitutionsAsync(TestRecipeId, selections);

            // Assert
            result.Description.ShouldContain("**Substitutions Made:**");
            result.Description.ShouldContain("1 cup milk");
            result.Description.ShouldContain("1 cup almond milk");
            result.Description.ShouldContain("Slightly nutty flavor");
            result.Description.ShouldContain("⚠️ May need to reduce sugar slightly");
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_WithEmptySelections_ThrowsArgumentException() {
            // Arrange
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.ApplySubstitutionsAsync(TestRecipeId, new List<SubstitutionSelectionDto>())
            );

            ex.Message.ShouldContain("Must provide at least one substitution selection");
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_WithNullSelections_ThrowsArgumentException() {
            // Arrange
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.ApplySubstitutionsAsync(TestRecipeId, null)
            );

            ex.Message.ShouldContain("Must provide at least one substitution selection");
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_WithMissingSubstitutionData_ThrowsArgumentException() {
            // Arrange
            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 0,
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "Missing ingredients",
                        Ingredients = new List<SubstitutionIngredientDto>() // Empty!
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act & Assert
            var ex = await Should.ThrowAsync<ArgumentException>(
                () => service.ApplySubstitutionsAsync(TestRecipeId, selections)
            );

            ex.Message.ShouldContain("Selection for ingredient index 0 is missing substitution data");
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_WithCustomForkTitle_UsesCustomTitle() {
            // Arrange
            var originalRecipe = BuildTestRecipe();
            var customTitle = "My Custom Gluten-Free Version";
            var forkedRecipe = new RecipeBuilder()
                .WithTitle(customTitle)
                .WithIngredients(originalRecipe.Ingredients.ToList())
                .Build();

            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 0,
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "GF flour",
                        Ingredients = new List<SubstitutionIngredientDto> {
                            new SubstitutionIngredientDto {
                                Quantity = 2m,
                                Unit = "cups",
                                Item = "gluten-free flour"
                            }
                        }
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(originalRecipe);

            mockRecipeService
                .Setup(x => x.ForkRecipeAsync(TestRecipeId, customTitle))
                .ReturnsAsync(forkedRecipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.ApplySubstitutionsAsync(TestRecipeId, selections, customTitle);

            // Assert
            result.Title.ShouldBe(customTitle);
            mockRecipeService.Verify(x => x.ForkRecipeAsync(TestRecipeId, customTitle), Times.Once);
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_WithMultipleIngredientSubstitution_CreatesCorrectFork() {
            // Arrange
            var originalRecipe = BuildTestRecipe();
            var forkedRecipe = new RecipeBuilder()
                .WithTitle("Test Recipe (Copy)")
                .WithIngredients(originalRecipe.Ingredients.ToList())
                .Build();

            // Replace butter with both coconut oil and a bit of salt
            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 2, // butter
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "Coconut oil with salt",
                        Ingredients = new List<SubstitutionIngredientDto> {
                            new SubstitutionIngredientDto {
                                Quantity = 2m,
                                Unit = "tbsp",
                                Item = "coconut oil"
                            },
                            new SubstitutionIngredientDto {
                                Quantity = 0.25m,
                                Unit = "tsp",
                                Item = "salt"
                            }
                        },
                        Notes = "Coconut oil replaces butter fat, salt adds flavor"
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(originalRecipe);

            mockRecipeService
                .Setup(x => x.ForkRecipeAsync(TestRecipeId, null))
                .ReturnsAsync(forkedRecipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.ApplySubstitutionsAsync(TestRecipeId, selections);

            // Assert
            result.Ingredients.Count.ShouldBe(4); // flour, milk, coconut oil, salt
            result.Ingredients[2].Item.ShouldBe("coconut oil");
            result.Ingredients[3].Item.ShouldBe("salt");
            result.Description.ShouldContain("2 tbsp coconut oil + 0.25 tsp salt");
        }

        [Fact]
        public async Task ApplySubstitutionsAsync_PreservesUnsubstitutedIngredients() {
            // Arrange
            var originalRecipe = BuildTestRecipe(); // Has flour, milk, butter
            var forkedRecipe = new RecipeBuilder()
                .WithTitle("Test Recipe (Copy)")
                .WithIngredients(originalRecipe.Ingredients.ToList())
                .Build();

            // Only substitute the first ingredient (flour)
            var selections = new List<SubstitutionSelectionDto> {
                new SubstitutionSelectionDto {
                    IngredientIndex = 0,
                    SelectedOption = new SubstitutionOptionDto {
                        Name = "GF flour",
                        Ingredients = new List<SubstitutionIngredientDto> {
                            new SubstitutionIngredientDto {
                                Quantity = 2m,
                                Unit = "cups",
                                Item = "gluten-free flour"
                            }
                        }
                    }
                }
            };

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockRecipeService = MockRepository.Create<IRecipeService>();
            var mockCacheService = MockRepository.Create<ISubstitutionCacheService>();

            mockRecipeService
                .Setup(x => x.GetRecipeAsync(TestRecipeId))
                .ReturnsAsync(originalRecipe);

            mockRecipeService
                .Setup(x => x.ForkRecipeAsync(TestRecipeId, null))
                .ReturnsAsync(forkedRecipe);

            var service = CreateService(mockGeminiClient, mockRecipeService, mockCacheService);

            // Act
            var result = await service.ApplySubstitutionsAsync(TestRecipeId, selections);

            // Assert
            result.Ingredients.Count.ShouldBe(3);
            result.Ingredients[0].Item.ShouldBe("gluten-free flour"); // Substituted
            result.Ingredients[1].Item.ShouldBe("milk"); // Preserved
            result.Ingredients[2].Item.ShouldBe("butter"); // Preserved
        }

        #endregion
    }
}
