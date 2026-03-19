using System.Collections.Generic;
using Shouldly;
using Xunit;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Mappers;

namespace RecipeVault.WebApi.Tests.Mappers {
    public class MealPlanModelMapperTests {
        private readonly MealPlanModelMapper mapper;

        public MealPlanModelMapperTests() {
            var subjectModelMapper = new SubjectModelMapper();
            mapper = new MealPlanModelMapper(subjectModelMapper);
        }

        [Fact]
        public void MapGroceryList_WithNullDto_ReturnsNull() {
            // Act
            var result = mapper.Map((GroceryListDto)null);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void MapGroceryList_WithValidDto_MapsAllFields() {
            // Arrange
            var dto = new GroceryListDto {
                Items = new List<GroceryItemDto> {
                    new GroceryItemDto {
                        Item = "flour",
                        Quantity = 2.5m,
                        Unit = "cup",
                        Category = "Pantry",
                        Sources = new List<string> { "Banana Bread", "Pizza Dough" }
                    },
                    new GroceryItemDto {
                        Item = "milk",
                        Quantity = 1m,
                        Unit = "cup",
                        Category = "Dairy",
                        Sources = new List<string> { "Smoothie" }
                    }
                }
            };

            // Act
            var result = mapper.Map(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);

            result.Items[0].Item.ShouldBe("flour");
            result.Items[0].Quantity.ShouldBe(2.5m);
            result.Items[0].Unit.ShouldBe("cup");
            result.Items[0].Category.ShouldBe("Pantry");
            result.Items[0].Sources.Count.ShouldBe(2);
            result.Items[0].Sources.ShouldContain("Banana Bread");
            result.Items[0].Sources.ShouldContain("Pizza Dough");

            result.Items[1].Item.ShouldBe("milk");
            result.Items[1].Category.ShouldBe("Dairy");
            result.Items[1].Sources.Count.ShouldBe(1);
            result.Items[1].Sources.ShouldContain("Smoothie");
        }

        [Fact]
        public void MapGroceryList_WithNullItems_MapsNullItems() {
            // Arrange
            var dto = new GroceryListDto {
                Items = null
            };

            // Act
            var result = mapper.Map(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeNull();
        }

        [Fact]
        public void MapGroceryList_WithEmptyItems_MapsEmptyList() {
            // Arrange
            var dto = new GroceryListDto {
                Items = new List<GroceryItemDto>()
            };

            // Act
            var result = mapper.Map(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }

        [Fact]
        public void MapGroceryList_WithNullOptionalFields_MapsNulls() {
            // Arrange
            var dto = new GroceryListDto {
                Items = new List<GroceryItemDto> {
                    new GroceryItemDto {
                        Item = "salt",
                        Quantity = null,
                        Unit = null,
                        Category = null,
                        Sources = null
                    }
                }
            };

            // Act
            var result = mapper.Map(dto);

            // Assert
            result.Items[0].Item.ShouldBe("salt");
            result.Items[0].Quantity.ShouldBeNull();
            result.Items[0].Unit.ShouldBeNull();
            result.Items[0].Category.ShouldBeNull();
            result.Items[0].Sources.ShouldBeNull();
        }
    }
}
