using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class MealPlanServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private static MealPlan BuildMealPlanWithOwner(MealPlanBuilder builder = null) {
            builder ??= new MealPlanBuilder();
            var mealPlan = builder.Build();
            mealPlan.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");
            return mealPlan;
        }

        [Fact]
        public async Task CreateMealPlanAsync_WithValidDto_CreatesAndReturnsMealPlan() {
            // Arrange
            var dto = new UpdateMealPlanDto {
                Name = "Weekly Plan",
                StartDate = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc)
            };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockMealPlanRepo
                .Setup(x => x.AddAsync(It.IsAny<MealPlan>()))
                .ReturnsAsync((MealPlan mp) => mp)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.CreateMealPlanAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(dto.Name);
            result.StartDate.ShouldBe(dto.StartDate);
            result.EndDate.ShouldBe(dto.EndDate);
            result.MealPlanResourceId.ShouldNotBe(Guid.Empty);

            mockMealPlanRepo.Verify(x => x.AddAsync(It.IsAny<MealPlan>()), Times.Once);
        }

        [Fact]
        public async Task GetMealPlanAsync_WithValidId_ReturnsMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner();
            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.GetMealPlanAsync(mealPlan.MealPlanResourceId);

            // Assert
            result.ShouldBe(mealPlan);
        }

        [Fact]
        public async Task GetMealPlanAsync_WithOtherUsersPlan_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var mealPlan = new MealPlanBuilder().Build();
            mealPlan.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act & Assert
            await Should.ThrowAsync<MealPlanNotFoundException>(
                () => service.GetMealPlanAsync(mealPlan.MealPlanResourceId)
            );
        }

        [Fact]
        public async Task DeleteMealPlanAsync_WithValidId_DeletesMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner();
            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            mockMealPlanRepo
                .Setup(x => x.RemoveAsync(mealPlan))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            await service.DeleteMealPlanAsync(mealPlan.MealPlanResourceId);

            // Assert
            mockMealPlanRepo.Verify(x => x.RemoveAsync(mealPlan), Times.Once);
        }

        [Fact]
        public async Task UpdateMealPlanAsync_WithValidData_UpdatesAndReturnsMealPlan() {
            // Arrange
            var mealPlan = BuildMealPlanWithOwner(new MealPlanBuilder().WithName("Original"));
            var updateDto = new UpdateMealPlanDto {
                Name = "Updated Plan",
                StartDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc)
            };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockMealPlanRepo
                .Setup(x => x.GetAsync(mealPlan.MealPlanResourceId))
                .ReturnsAsync(mealPlan);

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.UpdateMealPlanAsync(mealPlan.MealPlanResourceId, updateDto);

            // Assert
            result.Name.ShouldBe("Updated Plan");
            result.StartDate.ShouldBe(updateDto.StartDate);
        }

        [Fact]
        public async Task SearchMealPlansAsync_WithValidSearch_ReturnsPagedResults() {
            // Arrange
            var mealPlans = new List<MealPlan> {
                new MealPlanBuilder().WithName("Plan 1").Build(),
                new MealPlanBuilder().WithName("Plan 2").Build()
            };

            var pagedList = new PagedList<MealPlan> {
                Items = mealPlans,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var search = new MealPlanSearch { PageNumber = 1, PageSize = 10 };

            var mockMealPlanRepo = MockRepository.Create<IMealPlanRepository>();
            var mockRecipeRepo = MockRepository.Create<IRecipeRepository>();
            var mockLogger = CreateMockLogger<MealPlanService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockMealPlanRepo
                .Setup(x => x.SearchAsync(search))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var service = new MealPlanService(mockMealPlanRepo.Object, mockRecipeRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object, mockGeminiClient.Object);

            // Act
            var result = await service.SearchMealPlansAsync(search);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.TotalItems.ShouldBe(2);
        }
    }
}
