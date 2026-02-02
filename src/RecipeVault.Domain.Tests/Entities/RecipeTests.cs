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
            recipe.Ingredients.ShouldHaveCount(2);
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
            recipe.Instructions.ShouldHaveCount(3);
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
    }
}
