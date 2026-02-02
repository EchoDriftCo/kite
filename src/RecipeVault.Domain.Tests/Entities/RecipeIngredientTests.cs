using Shouldly;
using Xunit;
using RecipeVault.TestUtilities.Builders;

namespace RecipeVault.Domain.Tests.Entities {
    public class RecipeIngredientTests {
        [Fact]
        public void RecipeIngredient_CreatedWithValidValues_HasValidProperties() {
            // Arrange & Act
            var ingredient = new RecipeIngredientBuilder()
                .WithSortOrder(1)
                .WithQuantity(2m)
                .WithUnit("cups")
                .WithItem("flour")
                .WithPreparation("sifted")
                .WithRawText("2 cups flour, sifted")
                .Build();

            // Assert
            ingredient.SortOrder.ShouldBe(1);
            ingredient.Quantity.ShouldBe(2m);
            ingredient.Unit.ShouldBe("cups");
            ingredient.Item.ShouldBe("flour");
            ingredient.Preparation.ShouldBe("sifted");
            ingredient.RawText.ShouldBe("2 cups flour, sifted");
        }

        [Fact]
        public void RecipeIngredient_CreatedWithNullQuantity_HasNullQuantity() {
            // Arrange & Act
            var ingredient = new RecipeIngredientBuilder()
                .WithQuantity(null)
                .Build();

            // Assert
            ingredient.Quantity.ShouldBeNull();
        }

        [Fact]
        public void RecipeIngredient_CreatedWithEmptyPreparation_HasEmptyPreparation() {
            // Arrange & Act
            var ingredient = new RecipeIngredientBuilder()
                .WithPreparation("")
                .Build();

            // Assert
            ingredient.Preparation.ShouldBe("");
        }
    }
}
