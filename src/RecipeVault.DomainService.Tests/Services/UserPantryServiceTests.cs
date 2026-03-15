using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using Shouldly;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class UserPantryServiceTests {
        private readonly Mock<IUserPantryRepository> repositoryMock;
        private readonly Mock<ISubjectPrincipal> subjectPrincipalMock;
        private readonly Mock<ILogger<UserPantryService>> loggerMock;
        private readonly UserPantryService service;
        private readonly Guid currentSubjectId = Guid.NewGuid();

        public UserPantryServiceTests() {
            repositoryMock = new Mock<IUserPantryRepository>();
            subjectPrincipalMock = new Mock<ISubjectPrincipal>();
            loggerMock = new Mock<ILogger<UserPantryService>>();

            subjectPrincipalMock.Setup(x => x.SubjectId).Returns(currentSubjectId.ToString());

            service = new UserPantryService(
                repositoryMock.Object,
                subjectPrincipalMock.Object,
                loggerMock.Object
            );
        }

        [Fact]
        public async Task GetUserPantryAsync_ShouldReturnCurrentUserItems() {
            // Arrange
            var items = new List<UserPantryItem> {
                new UserPantryItem(currentSubjectId, "chicken breast"),
                new UserPantryItem(currentSubjectId, "salt", isStaple: true)
            };
            repositoryMock.Setup(x => x.GetBySubjectIdAsync(currentSubjectId))
                .ReturnsAsync(items);

            // Act
            var result = await service.GetUserPantryAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            repositoryMock.Verify(x => x.GetBySubjectIdAsync(currentSubjectId), Times.Once);
        }

        [Fact]
        public async Task AddPantryItemAsync_ShouldCreateNewItem() {
            // Arrange
            var dto = new CreatePantryItemDto {
                IngredientName = "  broccoli  ",
                IsStaple = false,
                ExpirationDate = new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc)
            };

            repositoryMock.Setup(x => x.AddAsync(It.IsAny<UserPantryItem>()))
                .ReturnsAsync((UserPantryItem item) => item);

            // Act
            var result = await service.AddPantryItemAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.IngredientName.ShouldBe("broccoli");
            result.SubjectId.ShouldBe(currentSubjectId);
            result.IsStaple.ShouldBeFalse();
            repositoryMock.Verify(x => x.AddAsync(It.Is<UserPantryItem>(i =>
                i.IngredientName == "broccoli" && i.SubjectId == currentSubjectId)), Times.Once);
        }

        [Fact]
        public async Task UpdatePantryItemAsync_WhenItemBelongsToUser_ShouldUpdate() {
            // Arrange
            var item = new UserPantryItem(currentSubjectId, "broccoli");
            repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);

            var dto = new UpdatePantryItemDto {
                IngredientName = "broccoli",
                IsStaple = true,
                ExpirationDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await service.UpdatePantryItemAsync(1, dto);

            // Assert
            result.ShouldNotBeNull();
            result.IsStaple.ShouldBeTrue();
            result.ExpirationDate.ShouldBe(new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task UpdatePantryItemAsync_WhenItemBelongsToDifferentUser_ShouldThrow() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var item = new UserPantryItem(otherUserId, "broccoli");
            repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);

            var dto = new UpdatePantryItemDto {
                IngredientName = "broccoli",
                IsStaple = false
            };

            // Act & Assert
            await Should.ThrowAsync<PantryItemNotFoundException>(() =>
                service.UpdatePantryItemAsync(1, dto));
        }

        [Fact]
        public async Task UpdatePantryItemAsync_WhenItemNotFound_ShouldThrow() {
            // Arrange
            repositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((UserPantryItem)null);

            var dto = new UpdatePantryItemDto {
                IngredientName = "broccoli",
                IsStaple = false
            };

            // Act & Assert
            await Should.ThrowAsync<PantryItemNotFoundException>(() =>
                service.UpdatePantryItemAsync(999, dto));
        }

        [Fact]
        public async Task DeletePantryItemAsync_WhenItemBelongsToUser_ShouldDelete() {
            // Arrange
            var item = new UserPantryItem(currentSubjectId, "broccoli");
            repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);

            // Act
            await service.DeletePantryItemAsync(1);

            // Assert
            repositoryMock.Verify(x => x.Remove(item), Times.Once);
        }

        [Fact]
        public async Task DeletePantryItemAsync_WhenItemBelongsToDifferentUser_ShouldThrow() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var item = new UserPantryItem(otherUserId, "broccoli");
            repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);

            // Act & Assert
            await Should.ThrowAsync<PantryItemNotFoundException>(() =>
                service.DeletePantryItemAsync(1));
        }

        [Fact]
        public async Task GetDefaultStaplesAsync_ShouldReturnDefaultList() {
            // Act
            var result = await service.GetDefaultStaplesAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(10);
            result.ShouldContain("salt");
            result.ShouldContain("black pepper");
            result.ShouldContain("garlic");
            result.ShouldContain("onion");
        }

        [Fact]
        public async Task EnsureUserPantrySeededAsync_WhenNoExistingItems_ShouldSeedDefaults() {
            // Arrange
            repositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(0);
            repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<List<UserPantryItem>>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.EnsureUserPantrySeededAsync();

            // Assert
            repositoryMock.Verify(x => x.AddRangeAsync(It.Is<List<UserPantryItem>>(items =>
                items.Count == 10 &&
                items.All(i => i.IsStaple) &&
                items.All(i => i.SubjectId == currentSubjectId)
            )), Times.Once);
        }

        [Fact]
        public async Task EnsureUserPantrySeededAsync_WhenExistingItems_ShouldNotSeed() {
            // Arrange
            repositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(5);

            // Act
            await service.EnsureUserPantrySeededAsync();

            // Assert
            repositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<UserPantryItem>>()), Times.Never);
        }
    }
}
