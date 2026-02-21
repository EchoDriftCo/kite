using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Exceptions;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService.Tests.Services {
    public class TagServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private TagService CreateService(Mock<ITagRepository> mockRepository, Mock<ISubjectPrincipal> mockSubjectPrincipal) {
            var mockLogger = CreateMockLogger<TagService>();
            return new TagService(mockRepository.Object, mockLogger.Object, mockSubjectPrincipal.Object);
        }

        [Fact]
        public async Task CreateTagAsync_WithNewName_CreatesAndReturnsTag() {
            // Arrange
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetByNameAndCategoryAsync("Italian", TagCategory.Cuisine))
                .ReturnsAsync((Tag)null)
                .Verifiable();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Tag>()))
                .ReturnsAsync((Tag tag) => tag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            var result = await service.CreateTagAsync("Italian", TagCategory.Cuisine);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Italian");
            result.Category.ShouldBe(TagCategory.Cuisine);
            result.IsGlobal.ShouldBeFalse();

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Tag>()), Times.Once);
        }

        [Fact]
        public async Task CreateTagAsync_WhenGlobalTagExists_ReturnsExistingGlobalTag() {
            // Arrange
            var globalTag = new TagBuilder().WithName("Vegan").WithCategory(TagCategory.Dietary).WithIsGlobal(true).Build();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetByNameAndCategoryAsync("Vegan", TagCategory.Dietary))
                .ReturnsAsync(globalTag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            var result = await service.CreateTagAsync("Vegan", TagCategory.Dietary);

            // Assert
            result.ShouldBe(globalTag);
            result.IsGlobal.ShouldBeTrue();

            // Should not create a new one
            mockRepository.Verify(x => x.AddAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateTagAsync_WhenTagExists_ReturnsExisting() {
            // Arrange
            var existingTag = new TagBuilder().WithName("Italian").WithCategory(TagCategory.Cuisine).Build();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetByNameAndCategoryAsync("Italian", TagCategory.Cuisine))
                .ReturnsAsync(existingTag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            var result = await service.GetOrCreateTagAsync("Italian", TagCategory.Cuisine);

            // Assert
            result.ShouldBe(existingTag);
            mockRepository.Verify(x => x.AddAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task GetTagAsync_WithValidId_ReturnsTag() {
            // Arrange
            var tag = new TagBuilder().WithName("Italian").WithCategory(TagCategory.Cuisine).Build();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetAsync(tag.TagResourceId))
                .ReturnsAsync(tag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            var result = await service.GetTagAsync(tag.TagResourceId);

            // Assert
            result.ShouldBe(tag);
            mockRepository.Verify(x => x.GetAsync(tag.TagResourceId), Times.Once);
        }

        [Fact]
        public async Task GetTagAsync_WithInvalidId_ThrowsTagNotFoundException() {
            // Arrange
            var invalidId = Guid.NewGuid();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetAsync(invalidId))
                .ReturnsAsync((Tag)null)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<TagNotFoundException>(
                () => service.GetTagAsync(invalidId)
            );
        }

        [Fact]
        public async Task UpdateTagAsync_WithOwnTag_UpdatesAndReturnsTag() {
            // Arrange
            var tag = new TagBuilder().WithName("My Tag").WithCategory(TagCategory.Custom).Build();
            tag.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");

            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(tag.TagResourceId))
                .ReturnsAsync(tag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            var result = await service.UpdateTagAsync(tag.TagResourceId, "Updated Tag", TagCategory.Source);

            // Assert
            result.Name.ShouldBe("Updated Tag");
            result.Category.ShouldBe(TagCategory.Source);
        }

        [Fact]
        public async Task UpdateTagAsync_WithGlobalTag_ThrowsTagNotFoundException() {
            // Arrange
            var globalTag = new TagBuilder().WithName("Vegan").WithCategory(TagCategory.Dietary).WithIsGlobal(true).Build();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(globalTag.TagResourceId))
                .ReturnsAsync(globalTag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<TagNotFoundException>(
                () => service.UpdateTagAsync(globalTag.TagResourceId, "Renamed", TagCategory.Dietary)
            );
        }

        [Fact]
        public async Task UpdateTagAsync_WithOtherUsersTag_ThrowsTagNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var tag = new TagBuilder().WithName("Their Tag").WithCategory(TagCategory.Custom).Build();
            tag.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(tag.TagResourceId))
                .ReturnsAsync(tag);

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<TagNotFoundException>(
                () => service.UpdateTagAsync(tag.TagResourceId, "Updated", TagCategory.Custom)
            );
        }

        [Fact]
        public async Task DeleteTagAsync_WithOwnTag_DeletesTag() {
            // Arrange
            var tag = new TagBuilder().WithName("My Tag").WithCategory(TagCategory.Custom).Build();
            tag.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");

            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(tag.TagResourceId))
                .ReturnsAsync(tag)
                .Verifiable();

            mockRepository
                .Setup(x => x.RemoveAsync(tag))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act
            await service.DeleteTagAsync(tag.TagResourceId);

            // Assert
            mockRepository.Verify(x => x.RemoveAsync(tag), Times.Once);
        }

        [Fact]
        public async Task DeleteTagAsync_WithGlobalTag_ThrowsTagNotFoundException() {
            // Arrange
            var globalTag = new TagBuilder().WithName("Vegan").WithCategory(TagCategory.Dietary).WithIsGlobal(true).Build();
            var mockRepository = MockRepository.Create<ITagRepository>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(globalTag.TagResourceId))
                .ReturnsAsync(globalTag)
                .Verifiable();

            var service = CreateService(mockRepository, mockSubjectPrincipal);

            // Act & Assert
            await Should.ThrowAsync<TagNotFoundException>(
                () => service.DeleteTagAsync(globalTag.TagResourceId)
            );

            mockRepository.Verify(x => x.RemoveAsync(It.IsAny<Tag>()), Times.Never);
        }
    }
}
