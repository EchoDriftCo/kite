using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.Facade;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.WebApi.Controllers;
using RecipeVault.WebApi.Mappers;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;
using RecipeVault.WebApi.Tests.Base;

namespace RecipeVault.WebApi.Tests.Controllers {
    public class RecipesControllerTests : ControllerTestBase {
        [Fact]
        public async Task GetRecipesAsync_WithValidSearch_ReturnsOkWithPagedResults() {
            // Arrange
            var recipeDtos = new List<RecipeDto>
            {
                new RecipeDtoBuilder().WithTitle("Recipe 1").Build(),
                new RecipeDtoBuilder().WithTitle("Recipe 2").Build()
            };

            var pagedList = new PagedList<RecipeDto>
            {
                Items = recipeDtos,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var search = new RecipeSearchModel { PageNumber = 1, PageSize = 10 };

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.SearchRecipesAsync(It.IsAny<RecipeSearchDto>()))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.GetRecipesAsync(search);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.SearchRecipesAsync(It.IsAny<RecipeSearchDto>()), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .WithTitle("Test Recipe")
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.GetRecipeAsync(recipeId))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.GetRecipeAsync(recipeId);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var returnedModel = okResult.Value.ShouldBeOfType<RecipeModel>();
            returnedModel.RecipeResourceId.ShouldBe(recipeId);
            returnedModel.Title.ShouldBe("Test Recipe");

            mockFacade.Verify(x => x.GetRecipeAsync(recipeId), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithValidInput_ReturnsCreatedAtActionWithRecipe() {
            // Arrange
            var input = new UpdateRecipeModel
            {
                Title = "New Recipe",
                Yield = 4,
                Description = "A test recipe"
            };

            var createdDto = new RecipeDtoBuilder()
                .WithTitle(input.Title)
                .WithYield(input.Yield)
                .WithDescription(input.Description)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.CreateRecipeAsync(It.IsAny<UpdateRecipeDto>()))
                .ReturnsAsync(createdDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.CreateRecipeAsync(input);

            // Assert
            var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
            createdResult.StatusCode.ShouldBe(201);
            createdResult.ActionName.ShouldBe(nameof(RecipesController.GetRecipeAsync));

            var returnedModel = createdResult.Value.ShouldBeOfType<RecipeModel>();
            returnedModel.Title.ShouldBe(input.Title);
            returnedModel.Yield.ShouldBe(input.Yield);

            mockFacade.Verify(x => x.CreateRecipeAsync(It.IsAny<UpdateRecipeDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithValidData_ReturnsOkWithUpdatedRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var input = new UpdateRecipeModel
            {
                Title = "Updated Recipe",
                Yield = 6
            };

            var updatedDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .WithTitle(input.Title)
                .WithYield(input.Yield)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.UpdateRecipeAsync(recipeId, It.IsAny<UpdateRecipeDto>()))
                .ReturnsAsync(updatedDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.UpdateRecipeAsync(recipeId, input);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var returnedModel = okResult.Value.ShouldBeOfType<RecipeModel>();
            returnedModel.Title.ShouldBe(input.Title);
            returnedModel.Yield.ShouldBe(input.Yield);

            mockFacade.Verify(x => x.UpdateRecipeAsync(recipeId, It.IsAny<UpdateRecipeDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidId_ReturnsNoContent() {
            // Arrange
            var recipeId = Guid.NewGuid();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.DeleteRecipeAsync(recipeId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.DeleteRecipeAsync(recipeId);

            // Assert
            var statusResult = result.ShouldBeOfType<StatusCodeResult>();
            statusResult.StatusCode.ShouldBe(204);

            mockFacade.Verify(x => x.DeleteRecipeAsync(recipeId), Times.Once);
        }

        [Fact]
        public async Task SetRecipeRatingAsync_WithValidRating_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var input = new SetRatingModel { Rating = 4 };

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.SetRecipeRatingAsync(recipeId, 4))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.SetRecipeRatingAsync(recipeId, input);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var returnedModel = okResult.Value.ShouldBeOfType<RecipeModel>();
            returnedModel.RecipeResourceId.ShouldBe(recipeId);

            mockFacade.Verify(x => x.SetRecipeRatingAsync(recipeId, 4), Times.Once);
        }

        [Fact]
        public async Task SetRecipeRatingAsync_WithNullRating_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var input = new SetRatingModel { Rating = null };

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.SetRecipeRatingAsync(recipeId, (int?)null))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.SetRecipeRatingAsync(recipeId, input);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.SetRecipeRatingAsync(recipeId, (int?)null), Times.Once);
        }

        [Fact]
        public async Task SetRecipeFavoriteAsync_WithTrue_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var input = new SetFavoriteModel { IsFavorite = true };

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.SetRecipeFavoriteAsync(recipeId, true))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.SetRecipeFavoriteAsync(recipeId, input);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.SetRecipeFavoriteAsync(recipeId, true), Times.Once);
        }

        [Fact]
        public async Task GenerateShareTokenAsync_WithValidId_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.GenerateShareTokenAsync(recipeId))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.GenerateShareTokenAsync(recipeId);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.GenerateShareTokenAsync(recipeId), Times.Once);
        }

        [Fact]
        public async Task RevokeShareTokenAsync_WithValidId_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.RevokeShareTokenAsync(recipeId))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.RevokeShareTokenAsync(recipeId);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.RevokeShareTokenAsync(recipeId), Times.Once);
        }

        [Fact]
        public async Task GetSharedRecipeAsync_WithValidToken_ReturnsOkWithRecipe() {
            // Arrange
            var recipeDto = new RecipeDtoBuilder()
                .WithTitle("Shared Recipe")
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.GetRecipeByShareTokenAsync("abc123"))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.GetSharedRecipeAsync("abc123");

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            var returnedModel = okResult.Value.ShouldBeOfType<RecipeModel>();
            returnedModel.Title.ShouldBe("Shared Recipe");

            mockFacade.Verify(x => x.GetRecipeByShareTokenAsync("abc123"), Times.Once);
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithValidId_ReturnsOkWithRecipe() {
            // Arrange
            var recipeId = Guid.NewGuid();
            var input = new SetVisibilityModel { IsPublic = true };

            var recipeDto = new RecipeDtoBuilder()
                .WithRecipeResourceId(recipeId)
                .Build();

            var mockFacade = MockRepository.Create<IRecipeFacade>();
            var mockSubjectMapper = MockRepository.Create<SubjectModelMapper>();
            var mapper = new RecipeModelMapper(mockSubjectMapper.Object);

            mockFacade
                .Setup(x => x.SetRecipeVisibilityAsync(recipeId, true))
                .ReturnsAsync(recipeDto)
                .Verifiable();

            var controller = new RecipesController(mockFacade.Object, mapper, Mock.Of<IWebHostEnvironment>());

            // Act
            var result = await controller.SetRecipeVisibilityAsync(recipeId, input);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);

            mockFacade.Verify(x => x.SetRecipeVisibilityAsync(recipeId, true), Times.Once);
        }
    }
}
