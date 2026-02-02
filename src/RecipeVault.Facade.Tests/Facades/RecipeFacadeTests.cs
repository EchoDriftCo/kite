using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Medallion.Threading;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.DomainService;
using RecipeVault.Facade.Mappers;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.Facade.Tests.Base;

namespace RecipeVault.Facade.Tests.Facades {
    public class RecipeFacadeTests : FacadeTestBase {
        [Fact]
        public async Task CreateRecipeAsync_WithValidDto_CreatesAndReturnsMappedDto() {
            // Arrange
            var dto = new UpdateRecipeDtoBuilder()
                .WithTitle("New Recipe")
                .WithYield(4)
                .Build();

            var recipe = new RecipeBuilder()
                .WithTitle(dto.Title)
                .WithYield(dto.Yield)
                .Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var mockLockProvider = MockRepository.Create<IDistributedLockProvider>();

            var mapper = new RecipeMapper();

            mockService
                .Setup(x => x.CreateRecipeAsync(dto))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, mockLockProvider.Object);

            // Act
            var result = await facade.CreateRecipeAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(dto.Title);
            result.Yield.ShouldBe(dto.Yield);

            mockService.Verify(x => x.CreateRecipeAsync(dto), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ReturnsRecipeDto() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var mockLockProvider = MockRepository.Create<IDistributedLockProvider>();
            var mockTransaction = MockRepository.Create<IAsyncDisposable>();

            var mapper = new RecipeMapper();

            mockUow
                .Setup(x => x.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            mockTransaction
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockService
                .Setup(x => x.GetRecipeAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, mockLockProvider.Object);

            // Act
            var result = await facade.GetRecipeAsync(recipe.RecipeResourceId);

            // Assert
            result.ShouldNotBeNull();
            result.RecipeResourceId.ShouldBe(recipe.RecipeResourceId);
            result.Title.ShouldBe(recipe.Title);

            mockService.Verify(x => x.GetRecipeAsync(recipe.RecipeResourceId), Times.Once);
        }

        [Fact]
        public async Task SearchRecipesAsync_WithValidSearch_ReturnsPagedRecipeDtos() {
            // Arrange
            var recipes = new List<Recipe>
            {
                new RecipeBuilder().WithTitle("Recipe 1").Build(),
                new RecipeBuilder().WithTitle("Recipe 2").Build()
            };

            var pagedList = new PagedList<Recipe>
            {
                Items = recipes,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var searchDto = new RecipeSearchDto { PageNumber = 1, PageSize = 10 };
            var recipeSearch = new RecipeSearch { PageNumber = 1, PageSize = 10 };

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var mockLockProvider = MockRepository.Create<IDistributedLockProvider>();
            var mockTransaction = MockRepository.Create<IAsyncDisposable>();

            var mapper = new RecipeMapper();

            mockUow
                .Setup(x => x.BeginReadUncommitedAsync())
                .ReturnsAsync(mockTransaction.Object)
                .Verifiable();

            mockTransaction
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockService
                .Setup(x => x.SearchRecipesAsync(It.IsAny<RecipeSearch>()))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, mockLockProvider.Object);

            // Act
            var result = await facade.SearchRecipesAsync(searchDto);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);

            mockService.Verify(x => x.SearchRecipesAsync(It.IsAny<RecipeSearch>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithValidData_UpdatesAndReturnsMappedDto() {
            // Arrange
            var recipe = new RecipeBuilder()
                .WithTitle("Original Title")
                .Build();

            var updateDto = new UpdateRecipeDtoBuilder()
                .WithTitle("Updated Title")
                .Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var mockLockProvider = MockRepository.Create<IDistributedLockProvider>();

            var mapper = new RecipeMapper();

            var mockLock = MockRepository.Create<IAsyncDisposable>();
            mockLock
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask());

            mockLockProvider
                .Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockLock.Object)
                .Verifiable();

            mockService
                .Setup(x => x.UpdateRecipeAsync(recipe.RecipeResourceId, It.IsAny<UpdateRecipeDto>()))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, mockLockProvider.Object);

            // Act
            var result = await facade.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto);

            // Assert
            result.ShouldNotBeNull();

            mockLockProvider.Verify(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockService.Verify(x => x.UpdateRecipeAsync(recipe.RecipeResourceId, It.IsAny<UpdateRecipeDto>()), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidId_DeletesRecipe() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var mockLockProvider = MockRepository.Create<IDistributedLockProvider>();

            var mapper = new RecipeMapper();

            var mockLock = MockRepository.Create<IAsyncDisposable>();
            mockLock
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask());

            mockLockProvider
                .Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockLock.Object)
                .Verifiable();

            mockService
                .Setup(x => x.DeleteRecipeAsync(recipe.RecipeResourceId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, mockLockProvider.Object);

            // Act
            await facade.DeleteRecipeAsync(recipe.RecipeResourceId);

            // Assert
            mockLockProvider.Verify(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockService.Verify(x => x.DeleteRecipeAsync(recipe.RecipeResourceId), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
