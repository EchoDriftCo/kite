using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Facade;
using RecipeVault.WebApi.Controllers;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Responses;
using Xunit;

namespace RecipeVault.WebApi.Tests.Controllers {
    public class RecipesControllerCookingDataTests {
        [Fact]
        public async Task GetCookingDataAsync_WithValidRecipeId_ReturnsOkWithCookingData() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var mockFacade = new Mock<IRecipeFacade>();
            var mockSubjectMapper = new Mock<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            var mockCookingDataDto = new CookingDataDto {
                RecipeResourceId = recipeId,
                Steps = new List<CookingStepDto> {
                    new CookingStepDto {
                        StepNumber = 1,
                        Instruction = "Preheat oven to 350°F for 5 minutes",
                        TimerIndexes = new List<int> { 0 }
                    },
                    new CookingStepDto {
                        StepNumber = 2,
                        Instruction = "Mix ingredients",
                        TimerIndexes = new List<int>()
                    }
                },
                Timers = new List<TimerDto> {
                    new TimerDto {
                        Index = 0,
                        Label = "Preheat (5 min)",
                        Seconds = 300,
                        StepNumber = 1
                    }
                },
                Temperatures = new List<TemperatureDto> {
                    new TemperatureDto {
                        StepNumber = 1,
                        Value = 350,
                        Unit = "F",
                        Context = "oven"
                    }
                }
            };

            mockFacade
                .Setup(f => f.GetCookingDataAsync(recipeId))
                .ReturnsAsync(mockCookingDataDto);

            var controller = new RecipesController(
                mockFacade.Object,
                mapper,
                null, // imageStorage
                null, // cookingLogFacade
                null, // cookingLogMapper
                null, // recipeRepository
                null  // subjectPrincipal
            );

            // Act
            var result = await controller.GetCookingDataAsync(recipeId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsType<CookingDataModel>(okResult.Value);

            var cookingData = okResult.Value as CookingDataModel;
            Assert.Equal(recipeId, cookingData.RecipeResourceId);
            Assert.Equal(2, cookingData.Steps.Count);
            Assert.Equal(1, cookingData.Steps[0].StepNumber);
            Assert.Contains("Preheat", cookingData.Steps[0].Instruction);
            Assert.Single(cookingData.Steps[0].TimerIndexes);
            Assert.Equal(0, cookingData.Steps[0].TimerIndexes[0]);
            Assert.Single(cookingData.Timers);
            Assert.Equal(300, cookingData.Timers[0].Seconds);
            Assert.Single(cookingData.Temperatures);
            Assert.Equal(350, cookingData.Temperatures[0].Value);
        }

        [Fact]
        public async Task GetCookingDataAsync_WithRecipeWithoutInstructions_ReturnsEmptySteps() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var mockFacade = new Mock<IRecipeFacade>();
            var mockSubjectMapper = new Mock<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            var mockCookingDataDto = new CookingDataDto {
                RecipeResourceId = recipeId,
                Steps = new List<CookingStepDto>(),
                Timers = new List<TimerDto>(),
                Temperatures = new List<TemperatureDto>()
            };

            mockFacade
                .Setup(f => f.GetCookingDataAsync(recipeId))
                .ReturnsAsync(mockCookingDataDto);

            var controller = new RecipesController(
                mockFacade.Object,
                mapper,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var result = await controller.GetCookingDataAsync(recipeId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var cookingData = okResult.Value as CookingDataModel;

            Assert.Empty(cookingData.Steps);
            Assert.Empty(cookingData.Timers);
            Assert.Empty(cookingData.Temperatures);
        }
    }
}
