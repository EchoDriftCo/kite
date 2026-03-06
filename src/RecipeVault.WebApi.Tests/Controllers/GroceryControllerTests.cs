using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.WebApi.Controllers;
using RecipeVault.WebApi.Models.Responses;
using Shouldly;
using Xunit;

namespace RecipeVault.WebApi.Tests.Controllers {
    public class GroceryControllerTests {
        private static readonly string[] ValidItems = { "2 lbs chicken breast", "olive oil" };
        private static readonly string[] SingleChickenItem = { "chicken" };
        private static readonly string[] SingleGarlicItem = { "garlic" };
        private static readonly string[] InvalidItems = { " ", "1 tbsp" };

        [Fact]
        public void GetCheckoutUrl_WithValidServiceAndItems_ReturnsOk() {
            // Arrange
            var controller = new GroceryController();

            // Act
            var result = controller.GetCheckoutUrl("instacart", ValidItems);

            // Assert
            var ok = result.ShouldBeOfType<OkObjectResult>();
            var model = ok.Value.ShouldBeOfType<GroceryCheckoutUrlModel>();
            model.Service.ShouldBe("instacart");
            model.ItemCount.ShouldBe(2);
            model.Url.ShouldNotBeNull();
            model.Url.ShouldContain("instacart.com");
        }

        [Fact]
        public void GetCheckoutUrl_WithInvalidService_ReturnsBadRequest() {
            // Arrange
            var controller = new GroceryController();

            // Act
            var result = controller.GetCheckoutUrl("doordash", SingleChickenItem);

            // Assert
            var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
            var details = badRequest.Value.ShouldBeOfType<ProblemDetails>();
            details.Title.ShouldBe("Invalid grocery service");
        }

        [Fact]
        public void GetCheckoutUrl_WithManualService_ReturnsNullUrl() {
            // Arrange
            var controller = new GroceryController();

            // Act
            var result = controller.GetCheckoutUrl("manual", SingleGarlicItem);

            // Assert
            var ok = result.ShouldBeOfType<OkObjectResult>();
            var model = ok.Value.ShouldBeOfType<GroceryCheckoutUrlModel>();
            model.Url.ShouldBeNull();
            model.ItemCount.ShouldBe(1);
        }

        [Fact]
        public void GetCheckoutOptions_WithValidItems_ReturnsAllSupportedServiceOptions() {
            // Arrange
            var controller = new GroceryController();

            // Act
            var result = controller.GetCheckoutOptions(ValidItems);

            // Assert
            var ok = result.ShouldBeOfType<OkObjectResult>();
            var model = ok.Value.ShouldBeOfType<GroceryCheckoutOptionsModel>();
            model.ItemCount.ShouldBe(2);
            model.Options.Count.ShouldBe(5);
            model.Options.Select(x => x.Service).ShouldContain("instacart");
            model.Options.Select(x => x.Service).ShouldContain("walmart");
            model.Options.Select(x => x.Service).ShouldContain("amazonfresh");
            model.Options.Select(x => x.Service).ShouldContain("shipt");
            model.Options.Select(x => x.Service).ShouldContain("manual");
            model.Options.Single(x => x.Service == "manual").Url.ShouldBeNull();
        }

        [Fact]
        public void GetCheckoutOptions_WithoutValidItems_ReturnsBadRequest() {
            // Arrange
            var controller = new GroceryController();

            // Act
            var result = controller.GetCheckoutOptions(InvalidItems);

            // Assert
            var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
            var details = badRequest.Value.ShouldBeOfType<ProblemDetails>();
            details.Title.ShouldBe("No valid grocery items");
        }
    }
}
