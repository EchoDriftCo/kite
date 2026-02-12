using System;
using System.Threading;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.DomainService;
using RecipeVault.Facade.Mappers;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.Facade.Tests.Base;

namespace RecipeVault.Facade.Tests.Facades {
    public class TagFacadeTests : FacadeTestBase {
        private readonly SubjectMapper subjectMapper = new SubjectMapper();
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        [Fact]
        public async Task CreateTagAsync_WithValidDto_CreatesAndReturnsMappedDto() {
            // Arrange
            var dto = new UpdateTagDto {
                Name = "Italian",
                Category = (int)TagCategory.Cuisine
            };

            var tag = new TagBuilder()
                .WithName(dto.Name)
                .WithCategory(TagCategory.Cuisine)
                .Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<ITagService>();
            var mockLogger = CreateMockLogger<TagFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = new TagMapper(subjectMapper);

            mockService
                .Setup(x => x.CreateTagAsync(dto.Name, (TagCategory)dto.Category))
                .ReturnsAsync(tag)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new TagFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.CreateTagAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(dto.Name);
            result.Category.ShouldBe(dto.Category);

            mockService.Verify(x => x.CreateTagAsync(dto.Name, (TagCategory)dto.Category), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTagAsync_WithValidId_ReturnsTagDto() {
            // Arrange
            var tag = new TagBuilder().WithName("Vegan").WithCategory(TagCategory.Dietary).Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<ITagService>();
            var mockLogger = CreateMockLogger<TagFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            var mapper = new TagMapper(subjectMapper);

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
                .Setup(x => x.GetTagAsync(tag.TagResourceId))
                .ReturnsAsync(tag)
                .Verifiable();

            var facade = new TagFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.GetTagAsync(tag.TagResourceId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(tag.Name);
            result.Category.ShouldBe((int)tag.Category);

            mockService.Verify(x => x.GetTagAsync(tag.TagResourceId), Times.Once);
        }

        [Fact]
        public async Task SearchTagsAsync_WithValidSearch_ReturnsPagedList() {
            // Arrange
            var search = new TagSearchDto {
                Name = "Vegan",
                Category = (int)TagCategory.Dietary
            };

            var tags = new PagedList<Tag>
            {
                Items = new[] {
                    new TagBuilder().WithName("Vegan").WithCategory(TagCategory.Dietary).Build()
                },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1
            };

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<ITagService>();
            var mockLogger = CreateMockLogger<TagFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            var mapper = new TagMapper(subjectMapper);

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
                .Setup(x => x.SearchTagsAsync(It.IsAny<TagSearch>()))
                .ReturnsAsync(tags)
                .Verifiable();

            var facade = new TagFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.SearchTagsAsync(search);

            // Assert
            result.ShouldNotBeNull();
            result.TotalItems.ShouldBe(1);
            result.Items[0].Name.ShouldBe("Vegan");

            mockService.Verify(x => x.SearchTagsAsync(It.IsAny<TagSearch>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTagAsync_WithValidDto_UpdatesAndReturnsDto() {
            // Arrange
            var tagId = Guid.NewGuid();
            var dto = new UpdateTagDto {
                Name = "Updated Name",
                Category = (int)TagCategory.Custom
            };

            var tag = new TagBuilder()
                .WithName(dto.Name)
                .WithCategory(TagCategory.Custom)
                .Build();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<ITagService>();
            var mockLogger = CreateMockLogger<TagFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            var mapper = new TagMapper(subjectMapper);

            mockService
                .Setup(x => x.UpdateTagAsync(tagId, dto.Name, (TagCategory)dto.Category))
                .ReturnsAsync(tag)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new TagFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            var result = await facade.UpdateTagAsync(tagId, dto);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(dto.Name);

            mockService.Verify(x => x.UpdateTagAsync(tagId, dto.Name, (TagCategory)dto.Category), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTagAsync_WithValidId_DeletesTag() {
            // Arrange
            var tagId = Guid.NewGuid();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<ITagService>();
            var mockLogger = CreateMockLogger<TagFacade>();
            var stubLockProvider = new StubDistributedLockProvider();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            var mapper = new TagMapper(subjectMapper);

            mockService
                .Setup(x => x.DeleteTagAsync(tagId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockUow
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var facade = new TagFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper, stubLockProvider, mockSubjectPrincipal.Object);

            // Act
            await facade.DeleteTagAsync(tagId);

            // Assert
            mockService.Verify(x => x.DeleteTagAsync(tagId), Times.Once);
            mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
