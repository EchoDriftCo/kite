using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
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
using Microsoft.EntityFrameworkCore.Storage;

namespace RecipeVault.Facade.Tests.Facades {
    public class RecipeFacadeTests : FacadeTestBase {
        private readonly SubjectMapper subjectMapper = new SubjectMapper();
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private RecipeMapper CreateMapper() {
            return new RecipeMapper(subjectMapper, new TagMapper(subjectMapper));
        }

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
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = CreateMapper();

            mockService
                .Setup(x => x.CreateRecipeAsync(dto))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockService
                .Setup(x => x.AnalyzeAndApplyDietaryTagsAsync(recipe))
                .Returns(Task.CompletedTask);

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.CreateRecipeAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(dto.Title);
            result.Yield.ShouldBe(dto.Yield);

            mockService.Verify(x => x.CreateRecipeAsync(dto), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ReturnsRecipeDto() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = CreateMapper();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(x => x.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            mockService
                .Setup(x => x.GetRecipeAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

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

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = CreateMapper();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(x => x.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(x => x.BeginReadUncommitedAsync())
                .ReturnsAsync(mockTransaction.Object)
                .Verifiable();

            mockService
                .Setup(x => x.SearchRecipesAsync(It.IsAny<RecipeSearch>()))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.SearchRecipesAsync(searchDto);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);

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

            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = CreateMapper();

            mockService
                .Setup(x => x.UpdateRecipeAsync(recipe.RecipeResourceId, It.IsAny<UpdateRecipeDto>()))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockService
                .Setup(x => x.AnalyzeAndApplyDietaryTagsAsync(recipe))
                .Returns(Task.CompletedTask);

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto);

            // Assert
            result.ShouldNotBeNull();

            mockService.Verify(x => x.UpdateRecipeAsync(recipe.RecipeResourceId, It.IsAny<UpdateRecipeDto>()), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithValidId_SetsVisibilityAndReturnsMappedDto() {
            // Arrange
            var recipe = new RecipeBuilder().WithIsPublic(false).Build();
            recipe.CreatedSubject = new Cortside.AspNetCore.Auditable.Entities.Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = CreateMapper();

            mockService
                .Setup(x => x.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockService
                .Setup(x => x.GetRecipeAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true);

            // Assert
            result.ShouldNotBeNull();
            result.RecipeResourceId.ShouldBe(recipe.RecipeResourceId);
            result.IsOwner.ShouldBeTrue();

            mockService.Verify(x => x.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidId_DeletesRecipe() {
            // Arrange
            var recipe = new RecipeBuilder().Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IRecipeService>();
            var mockLogger = CreateMockLogger<RecipeFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            var mapper = CreateMapper();

            mockService
                .Setup(x => x.DeleteRecipeAsync(recipe.RecipeResourceId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new RecipeFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            await facade.DeleteRecipeAsync(recipe.RecipeResourceId);

            // Assert
            mockService.Verify(x => x.DeleteRecipeAsync(recipe.RecipeResourceId), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Stub implementation of IDistributedLockProvider for testing
    /// </summary>
#pragma warning disable CA1822
    internal sealed class StubDistributedLockProvider : IDistributedLockProvider {
        public IDistributedSynchronizationHandle Acquire(string name, TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            return new StubSynchronizationHandle();
        }

        public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(string name, TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            return new ValueTask<IDistributedSynchronizationHandle>(new StubSynchronizationHandle());
        }

        public IDistributedLock CreateLock(string name) {
            return new StubDistributedLock();
        }
    }
#pragma warning restore CA1822

    /// <summary>
    /// Stub implementation of IDistributedLock for testing
    /// </summary>
    internal sealed class StubDistributedLock : IDistributedLock {
        public string Name => "stub-lock";

        public IDistributedSynchronizationHandle Acquire(TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            return new StubSynchronizationHandle();
        }

        public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            return new ValueTask<IDistributedSynchronizationHandle>(new StubSynchronizationHandle());
        }

        public IDistributedSynchronizationHandle TryAcquire(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
            return new StubSynchronizationHandle();
        }

        public ValueTask<IDistributedSynchronizationHandle> TryAcquireAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
            return new ValueTask<IDistributedSynchronizationHandle>(new StubSynchronizationHandle());
        }
    }

    /// <summary>
    /// Stub implementation of IDistributedSynchronizationHandle for testing
    /// </summary>
#pragma warning disable CA1822
    internal sealed class StubSynchronizationHandle : IDistributedSynchronizationHandle {
        public string HandleId => "stub-handle";
        public CancellationToken HandleLostToken => CancellationToken.None;
#pragma warning restore CA1822

        public void Dispose() {
            // No-op for testing
        }

        public ValueTask DisposeAsync() {
            return default;
        }
    }
}
