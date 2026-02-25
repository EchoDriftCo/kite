using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using Shouldly;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class CollectionServiceTests {
        private readonly Mock<ICollectionRepository> collectionRepositoryMock;
        private readonly Mock<IRecipeRepository> recipeRepositoryMock;
        private readonly Mock<ILogger<CollectionService>> loggerMock;
        private readonly Mock<ISubjectPrincipal> subjectPrincipalMock;
        private readonly CollectionService service;
        private readonly Guid currentSubjectId = Guid.NewGuid();

        public CollectionServiceTests() {
            collectionRepositoryMock = new Mock<ICollectionRepository>();
            recipeRepositoryMock = new Mock<IRecipeRepository>();
            loggerMock = new Mock<ILogger<CollectionService>>();
            subjectPrincipalMock = new Mock<ISubjectPrincipal>();

            subjectPrincipalMock.Setup(x => x.SubjectId).Returns(currentSubjectId.ToString());

            service = new CollectionService(
                collectionRepositoryMock.Object,
                recipeRepositoryMock.Object,
                loggerMock.Object,
                subjectPrincipalMock.Object
            );
        }

        [Fact]
        public async Task CreateCollectionAsync_ShouldCreateCollection() {
            // Arrange
            var dto = new UpdateCollectionDto {
                Name = "Test Collection",
                Description = "Test Description"
            };

            collectionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Collection>()))
                .ReturnsAsync((Collection c) => c);

            // Act
            var result = await service.CreateCollectionAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test Collection");
            result.Description.ShouldBe("Test Description");
            result.SubjectId.ShouldBe(currentSubjectId);
            collectionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Collection>()), Times.Once);
        }

        [Fact]
        public async Task GetCollectionAsync_WhenUserOwnsCollection_ShouldReturnCollection() {
            // Arrange
            var collectionId = Guid.NewGuid();
            var collection = new Collection("Test Collection", "Description", currentSubjectId);
            typeof(Collection).GetProperty("CollectionResourceId")!.SetValue(collection, collectionId);

            collectionRepositoryMock.Setup(x => x.GetAsync(collectionId))
                .ReturnsAsync(collection);

            // Act
            var result = await service.GetCollectionAsync(collectionId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test Collection");
        }

        [Fact]
        public async Task GetCollectionAsync_WhenCollectionNotFound_ShouldThrowException() {
            // Arrange
            var collectionId = Guid.NewGuid();
            collectionRepositoryMock.Setup(x => x.GetAsync(collectionId))
                .ReturnsAsync((Collection)null);

            // Act & Assert
            await Should.ThrowAsync<CollectionNotFoundException>(
                async () => await service.GetCollectionAsync(collectionId)
            );
        }

        [Fact]
        public async Task DeleteCollectionAsync_WhenUserOwnsCollection_ShouldDelete() {
            // Arrange
            var collectionId = Guid.NewGuid();
            var collection = new Collection("Test Collection", "Description", currentSubjectId);
            typeof(Collection).GetProperty("CollectionResourceId")!.SetValue(collection, collectionId);

            collectionRepositoryMock.Setup(x => x.GetAsync(collectionId))
                .ReturnsAsync(collection);

            // Act
            await service.DeleteCollectionAsync(collectionId);

            // Assert
            collectionRepositoryMock.Verify(x => x.RemoveAsync(collection), Times.Once);
        }

        [Fact]
        public async Task GetUserCollectionsAsync_ShouldReturnUserCollections() {
            // Arrange
            var collections = new List<Collection> {
                new Collection("Collection 1", "Desc 1", currentSubjectId),
                new Collection("Collection 2", "Desc 2", currentSubjectId)
            };

            collectionRepositoryMock.Setup(x => x.GetUserCollectionsAsync(currentSubjectId))
                .ReturnsAsync(collections);

            // Act
            var result = await service.GetUserCollectionsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
        }
    }
}
