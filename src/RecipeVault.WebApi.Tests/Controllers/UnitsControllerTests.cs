using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using RecipeVault.WebApi.Controllers;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using RecipeVault.WebApi.Tests.Base;

namespace RecipeVault.WebApi.Tests.Controllers {
    public class UnitsControllerTests : ControllerTestBase {
        [Fact]
        public async Task GetAllAsync_ReturnsOkWithUnits() {
            // Arrange
            var unitDtos = new List<UnitDto>
            {
                new UnitDto {
                    UnitResourceId = Guid.NewGuid(),
                    Name = "tablespoon",
                    Abbreviation = "tbsp",
                    PluralName = "tablespoons",
                    Type = "Volume"
                },
                new UnitDto {
                    UnitResourceId = Guid.NewGuid(),
                    Name = "teaspoon",
                    Abbreviation = "tsp",
                    PluralName = "teaspoons",
                    Type = "Volume"
                }
            };

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.GetAllAsync())
                .ReturnsAsync(unitDtos)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.GetAllAsync();

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var models = okResult.Value.ShouldBeOfType<List<UnitModel>>();
            models.Count.ShouldBe(2);
            models.First().Name.ShouldBe("tablespoon");
            models.Last().Name.ShouldBe("teaspoon");

            mockFacade.Verify(f => f.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsOkWithUnit() {
            // Arrange
            var unitId = Guid.NewGuid();
            var unitDto = new UnitDto {
                UnitResourceId = unitId,
                Name = "tablespoon",
                Abbreviation = "tbsp",
                PluralName = "tablespoons",
                Type = "Volume",
                Aliases = new List<UnitAliasDto> {
                    new UnitAliasDto { Alias = "T" },
                    new UnitAliasDto { Alias = "Tbsp" }
                }
            };

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.GetByIdAsync(unitId))
                .ReturnsAsync(unitDto)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.GetByIdAsync(unitId);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var model = okResult.Value.ShouldBeOfType<UnitModel>();
            model.Name.ShouldBe("tablespoon");
            model.Abbreviation.ShouldBe("tbsp");
            model.Aliases.Count.ShouldBe(2);

            mockFacade.Verify(f => f.GetByIdAsync(unitId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound() {
            // Arrange
            var unitId = Guid.NewGuid();

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.GetByIdAsync(unitId))
                .ReturnsAsync((UnitDto)null)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.GetByIdAsync(unitId);

            // Assert
            var notFoundResult = result.ShouldBeOfType<NotFoundResult>();
            notFoundResult.StatusCode.ShouldBe(404);

            mockFacade.Verify(f => f.GetByIdAsync(unitId), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithValidInput_ReturnsOkWithMatchResult() {
            // Arrange
            var request = new UnitMatchRequestModel { Input = "tbsp" };
            var matchResultDto = new UnitMatchResultDto {
                IsMatch = true,
                Confidence = 1.0m,
                OriginalInput = "tbsp",
                Unit = new UnitDto {
                    UnitResourceId = Guid.NewGuid(),
                    Name = "tablespoon",
                    Abbreviation = "tbsp",
                    PluralName = "tablespoons",
                    Type = "Volume"
                }
            };

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.MatchAsync("tbsp"))
                .ReturnsAsync(matchResultDto)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.MatchAsync(request);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var model = okResult.Value.ShouldBeOfType<UnitMatchModel>();
            model.IsMatch.ShouldBeTrue();
            model.Confidence.ShouldBe(1.0m);
            model.Unit.Name.ShouldBe("tablespoon");

            mockFacade.Verify(f => f.MatchAsync("tbsp"), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithNoMatch_ReturnsOkWithNoMatchResult() {
            // Arrange
            var request = new UnitMatchRequestModel { Input = "xyz" };
            var matchResultDto = new UnitMatchResultDto {
                IsMatch = false,
                Confidence = 0m,
                OriginalInput = "xyz",
                Unit = null
            };

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.MatchAsync("xyz"))
                .ReturnsAsync(matchResultDto)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.MatchAsync(request);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var model = okResult.Value.ShouldBeOfType<UnitMatchModel>();
            model.IsMatch.ShouldBeFalse();
            model.Unit.ShouldBeNull();

            mockFacade.Verify(f => f.MatchAsync("xyz"), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithFuzzyMatch_ReturnsOkWithFuzzyResult() {
            // Arrange
            var request = new UnitMatchRequestModel { Input = "tbs" };
            var matchResultDto = new UnitMatchResultDto {
                IsMatch = true,
                Confidence = 0.9m,
                OriginalInput = "tbs",
                Unit = new UnitDto {
                    UnitResourceId = Guid.NewGuid(),
                    Name = "tablespoon",
                    Abbreviation = "tbsp",
                    Type = "Volume"
                }
            };

            var mockFacade = MockRepository.Create<IUnitFacade>();
            var mapper = new UnitModelMapper();

            mockFacade
                .Setup(f => f.MatchAsync("tbs"))
                .ReturnsAsync(matchResultDto)
                .Verifiable();

            var controller = new UnitsController(mockFacade.Object, mapper);

            // Act
            var result = await controller.MatchAsync(request);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var model = okResult.Value.ShouldBeOfType<UnitMatchModel>();
            model.IsMatch.ShouldBeTrue();
            model.Confidence.ShouldBe(0.9m);
            model.OriginalInput.ShouldBe("tbs");

            mockFacade.Verify(f => f.MatchAsync("tbs"), Times.Once);
        }
    }
}
