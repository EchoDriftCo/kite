using System.Collections.Generic;
using RecipeVault.WebApi.Services;
using Shouldly;
using Xunit;

namespace RecipeVault.WebApi.Tests.Services {
    public class GroceryCheckoutUrlBuilderTests {
        [Fact]
        public void NormalizeItems_DedupesAndCleansAndCapsAt30() {
            // Arrange
            var items = new List<string> {
                "2 lbs chicken breast, diced",
                "chicken breast",
                "1 tbsp olive oil"
            };

            for (var i = 0; i < 40; i++) {
                items.Add($"item {i}");
            }

            // Act
            var normalized = GroceryCheckoutUrlBuilder.NormalizeItems(items);

            // Assert
            normalized.Count.ShouldBe(30);
            normalized[0].ShouldBe("chicken breast diced");
            normalized[1].ShouldBe("chicken breast");
            normalized.ShouldContain("olive oil");
        }

        [Fact]
        public void BuildUrl_Instacart_UsesInstacartPattern() {
            var url = GroceryCheckoutUrlBuilder.BuildUrl("instacart", new List<string> { "chicken breast", "olive oil" });
            url.ShouldStartWith("https://www.instacart.com/store/recipes?ingredients=");
        }

        [Fact]
        public void BuildUrl_Manual_ReturnsNull() {
            var url = GroceryCheckoutUrlBuilder.BuildUrl("manual", new List<string> { "chicken breast" });
            url.ShouldBeNull();
        }
    }
}
