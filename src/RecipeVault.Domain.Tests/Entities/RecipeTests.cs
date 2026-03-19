using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.TestUtilities.Builders;

namespace RecipeVault.Domain.Tests.Entities {
    public class RecipeTests {
        [Fact]
        public void Recipe_CreatedWithValidValues_HasValidProperties() {
            // Arrange & Act
            var recipe = new RecipeBuilder()
                .WithTitle("Chocolate Chip Cookies")
                .WithYield(24)
                .WithPrepTimeMinutes(15)
                .WithCookTimeMinutes(10)
                .WithDescription("Delicious cookies")
                .WithSource("Grandma")
                .WithOriginalImageUrl("https://example.com/cookies.jpg")
                .Build();

            // Assert
            recipe.Title.ShouldBe("Chocolate Chip Cookies");
            recipe.Yield.ShouldBe(24);
            recipe.PrepTimeMinutes.ShouldBe(15);
            recipe.CookTimeMinutes.ShouldBe(10);
            recipe.TotalTimeMinutes.ShouldBe(25);
            recipe.Description.ShouldBe("Delicious cookies");
            recipe.Source.ShouldBe("Grandma");
            recipe.OriginalImageUrl.ShouldBe("https://example.com/cookies.jpg");
            recipe.RecipeResourceId.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public void Recipe_UpdateWithValidValues_ChangesProperties() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.Update(
                "Updated Recipe",
                6,
                20,
                40,
                "Updated description",
                "Updated source",
                "https://example.com/updated.jpg"
            );

            // Assert
            recipe.Title.ShouldBe("Updated Recipe");
            recipe.Yield.ShouldBe(6);
            recipe.PrepTimeMinutes.ShouldBe(20);
            recipe.CookTimeMinutes.ShouldBe(40);
            recipe.TotalTimeMinutes.ShouldBe(60);
            recipe.Description.ShouldBe("Updated description");
            recipe.Source.ShouldBe("Updated source");
            recipe.OriginalImageUrl.ShouldBe("https://example.com/updated.jpg");
        }

        [Fact]
        public void Recipe_TotalTimeMinutes_ReturnsNullWhenTimesMissing() {
            // Arrange & Act
            var recipe = new RecipeBuilder()
                .WithPrepTimeMinutes(null)
                .WithCookTimeMinutes(null)
                .Build();

            // Assert
            recipe.TotalTimeMinutes.ShouldBeNull();
        }

        [Fact]
        public void Recipe_TotalTimeMinutes_ReturnsNullWhenOnlyPrepTimePresent() {
            // Arrange & Act
            var recipe = new RecipeBuilder()
                .WithPrepTimeMinutes(30)
                .WithCookTimeMinutes(null)
                .Build();

            // Assert
            recipe.TotalTimeMinutes.ShouldBeNull();
        }

        [Fact]
        public void Recipe_SetIngredients_ReplacesExistingIngredients() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            var ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredientBuilder().WithItem("flour").WithSortOrder(1).Build(),
                new RecipeIngredientBuilder().WithItem("sugar").WithSortOrder(2).Build()
            };

            // Act
            recipe.SetIngredients(ingredients);

            // Assert
            recipe.Ingredients.Count.ShouldBe(2);
            recipe.Ingredients[0].Item.ShouldBe("flour");
            recipe.Ingredients[1].Item.ShouldBe("sugar");
        }

        [Fact]
        public void Recipe_SetInstructions_ReplacesExistingInstructions() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            var instructions = new List<RecipeInstruction>
            {
                new RecipeInstructionBuilder().WithStepNumber(1).WithInstruction("Step 1").Build(),
                new RecipeInstructionBuilder().WithStepNumber(2).WithInstruction("Step 2").Build(),
                new RecipeInstructionBuilder().WithStepNumber(3).WithInstruction("Step 3").Build()
            };

            // Act
            recipe.SetInstructions(instructions);

            // Assert
            recipe.Instructions.Count.ShouldBe(3);
            recipe.Instructions[0].Instruction.ShouldBe("Step 1");
            recipe.Instructions[1].Instruction.ShouldBe("Step 2");
            recipe.Instructions[2].Instruction.ShouldBe("Step 3");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Recipe_UpdateWithEmptyTitle_ThrowsException(string title) {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act & Assert
            Should.Throw<Exception>(() =>
                recipe.Update(title, 4, 10, 20, "desc", "source", "url")
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Recipe_UpdateWithInvalidYield_ThrowsException(int yield) {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act & Assert
            Should.Throw<Exception>(() =>
                recipe.Update("Title", yield, 10, 20, "desc", "source", "url")
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void Recipe_SetRating_WithValidValue_SetsRating(int rating) {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.SetRating(rating);

            // Assert
            recipe.Rating.ShouldBe(rating);
        }

        [Fact]
        public void Recipe_SetRating_WithNull_ClearsRating() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            recipe.SetRating(3);

            // Act
            recipe.SetRating(null);

            // Assert
            recipe.Rating.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void Recipe_SetRating_WithInvalidValue_ThrowsException(int rating) {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act & Assert
            Should.Throw<Exception>(() => recipe.SetRating(rating));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Recipe_SetFavorite_SetsFavoriteStatus(bool isFavorite) {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.SetFavorite(isFavorite);

            // Assert
            recipe.IsFavorite.ShouldBe(isFavorite);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Recipe_SetVisibility_SetsIsPublic(bool isPublic) {
            // Arrange
            var recipe = new RecipeBuilder().WithIsPublic(!isPublic).Build();

            // Act
            recipe.SetVisibility(isPublic);

            // Assert
            recipe.IsPublic.ShouldBe(isPublic);
        }

        [Fact]
        public void Recipe_GenerateShareToken_CreatesNonNullToken() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.GenerateShareToken();

            // Assert
            recipe.ShareToken.ShouldNotBeNullOrWhiteSpace();
            recipe.ShareToken.Length.ShouldBe(10);
        }

        [Fact]
        public void Recipe_GenerateShareToken_CreatesAlphanumericToken() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.GenerateShareToken();

            // Assert
            recipe.ShareToken.ShouldAllBe(c => char.IsLetterOrDigit(c));
        }

        [Fact]
        public void Recipe_GenerateShareToken_CalledTwice_GeneratesDifferentTokens() {
            // Arrange
            var recipe1 = new RecipeBuilder().Build();
            var recipe2 = new RecipeBuilder().Build();

            // Act
            recipe1.GenerateShareToken();
            recipe2.GenerateShareToken();

            // Assert
            recipe1.ShareToken.ShouldNotBe(recipe2.ShareToken);
        }

        [Fact]
        public void Recipe_RevokeShareToken_ClearsToken() {
            // Arrange
            var recipe = new RecipeBuilder().Build();
            recipe.GenerateShareToken();
            recipe.ShareToken.ShouldNotBeNull();

            // Act
            recipe.RevokeShareToken();

            // Assert
            recipe.ShareToken.ShouldBeNull();
        }

        [Fact]
        public void Recipe_SetSourceImageUrl_SetsUrl() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            // Act
            recipe.SetSourceImageUrl("https://example.com/source.jpg");

            // Assert
            recipe.SourceImageUrl.ShouldBe("https://example.com/source.jpg");
        }

        [Fact]
        public void Recipe_DefaultState_HasNoRatingOrFavorite() {
            // Arrange & Act
            var recipe = new RecipeBuilder().Build();

            // Assert
            recipe.Rating.ShouldBeNull();
            recipe.IsFavorite.ShouldBeFalse();
            recipe.ShareToken.ShouldBeNull();
        }

        [Fact]
        public void Recipe_Fork_CreatesNewRecipeWithSameContent() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .WithYield(4)
                .WithPrepTimeMinutes(15)
                .WithCookTimeMinutes(30)
                .WithDescription("Original description")
                .WithOriginalImageUrl("https://example.com/image.jpg")
                .Build();

            var ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredientBuilder().WithItem("flour").WithSortOrder(1).WithQuantity(2).WithUnit("cups").Build(),
                new RecipeIngredientBuilder().WithItem("sugar").WithSortOrder(2).WithQuantity(1).WithUnit("cup").Build()
            };
            original.SetIngredients(ingredients);

            var instructions = new List<RecipeInstruction>
            {
                new RecipeInstructionBuilder().WithStepNumber(1).WithInstruction("Mix ingredients").Build(),
                new RecipeInstructionBuilder().WithStepNumber(2).WithInstruction("Bake").Build()
            };
            original.SetInstructions(instructions);

            original.SetSourceImageUrl("https://example.com/source.jpg");

            // Act
            var fork = original.Fork();

            // Assert
            fork.ShouldNotBeNull();
            fork.RecipeResourceId.ShouldNotBe(original.RecipeResourceId);
            fork.RecipeResourceId.ShouldNotBe(Guid.Empty);
            fork.Title.ShouldBe("Original Recipe (Copy)");
            fork.Yield.ShouldBe(4);
            fork.PrepTimeMinutes.ShouldBe(15);
            fork.CookTimeMinutes.ShouldBe(30);
            fork.Description.ShouldBe("Original description");
            fork.OriginalImageUrl.ShouldBe("https://example.com/image.jpg");
            fork.SourceImageUrl.ShouldBe("https://example.com/source.jpg");
            fork.Source.ShouldBeNull(); // Source is cleared on fork
            fork.IsPublic.ShouldBeFalse(); // Forks start private
            fork.ForkedFromRecipeId.ShouldBe(original.RecipeId);

            // Verify ingredients copied
            fork.Ingredients.Count.ShouldBe(2);
            fork.Ingredients[0].Item.ShouldBe("flour");
            fork.Ingredients[1].Item.ShouldBe("sugar");

            // Verify instructions copied
            fork.Instructions.Count.ShouldBe(2);
            fork.Instructions[0].Instruction.ShouldBe("Mix ingredients");
            fork.Instructions[1].Instruction.ShouldBe("Bake");
        }

        [Fact]
        public void Recipe_Fork_WithCustomTitle_UsesProvidedTitle() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .Build();

            // Act
            var fork = original.Fork("My Custom Version");

            // Assert
            fork.Title.ShouldBe("My Custom Version");
        }

        [Fact]
        public void Recipe_Fork_WithoutSourceImage_CopiesCorrectly() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .Build();

            // Act
            var fork = original.Fork();

            // Assert
            fork.SourceImageUrl.ShouldBeNull();
        }

        [Fact]
        public void Recipe_Fork_ClearsPersonalData() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .WithSource("Personal source")
                .WithIsPublic(true)
                .Build();

            original.SetRating(5);
            original.SetFavorite(true);
            original.GenerateShareToken();

            // Act
            var fork = original.Fork();

            // Assert
            fork.Source.ShouldBeNull();
            fork.IsPublic.ShouldBeFalse();
            fork.Rating.ShouldBeNull();
            fork.IsFavorite.ShouldBeFalse();
            fork.ShareToken.ShouldBeNull();
        }

        [Fact]
        public void Recipe_Fork_CopiesTags() {
            // Arrange
            var original = new RecipeBuilder()
                .WithTitle("Original Recipe")
                .Build();

            // Add tags to original
            var tag1 = new RecipeTag(
                recipeId: 1,
                tagId: 10,
                assignedBySubjectId: Guid.NewGuid(),
                isAiAssigned: true,
                confidence: 0.95m
            );

            var tag2 = new RecipeTag(
                recipeId: 1,
                tagId: 20,
                assignedBySubjectId: Guid.NewGuid(),
                isAiAssigned: false,
                confidence: null
            );

            original.AddTag(tag1);
            original.AddTag(tag2);

            // Act
            var fork = original.Fork();

            // Assert
            fork.RecipeTags.Count.ShouldBe(2);
            
            var forkedTag1 = fork.RecipeTags[0];
            forkedTag1.TagId.ShouldBe(10);
            forkedTag1.IsAiAssigned.ShouldBe(true);
            forkedTag1.Confidence.ShouldBe(0.95m);
            forkedTag1.RecipeId.ShouldBe(0);  // Will be set when saved
            forkedTag1.AssignedBySubjectId.ShouldBe(Guid.Empty);  // Will be set by service

            var forkedTag2 = fork.RecipeTags[1];
            forkedTag2.TagId.ShouldBe(20);
            forkedTag2.IsAiAssigned.ShouldBe(false);
            forkedTag2.Confidence.ShouldBeNull();
            forkedTag2.RecipeId.ShouldBe(0);  // Will be set when saved
            forkedTag2.AssignedBySubjectId.ShouldBe(Guid.Empty);  // Will be set by service
        }
    }
}
