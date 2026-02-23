using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using Xunit;
using Cortside.Common.Security;

namespace RecipeVault.DomainService.Tests {
    public class CircleServiceTests {
        private readonly Mock<ICircleRepository> mockCircleRepository;
        private readonly Mock<IRecipeRepository> mockRecipeRepository;
        private readonly Mock<ILogger<CircleService>> mockLogger;
        private readonly Mock<ISubjectPrincipal> mockSubjectPrincipal;
        private readonly CircleService service;

        public CircleServiceTests() {
            mockCircleRepository = new Mock<ICircleRepository>();
            mockRecipeRepository = new Mock<IRecipeRepository>();
            mockLogger = new Mock<ILogger<CircleService>>();
            mockSubjectPrincipal = new Mock<ISubjectPrincipal>();
            
            // Default subject ID
            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns("1");
            
            service = new CircleService(
                mockCircleRepository.Object,
                mockRecipeRepository.Object,
                mockLogger.Object,
                mockSubjectPrincipal.Object
            );
        }

        [Fact]
        public async Task CreateCircleAsync_ShouldCreateCircle() {
            // Arrange
            var dto = new UpdateCircleDto {
                Name = "Family Recipes",
                Description = "Our family favorites"
            };

            mockCircleRepository
                .Setup(x => x.AddAsync(It.IsAny<Circle>()))
                .ReturnsAsync((Circle c) => c);

            // Act
            var result = await service.CreateCircleAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Family Recipes", result.Name);
            Assert.Equal("Our family favorites", result.Description);
            Assert.Equal(1, result.OwnerSubjectId);
            mockCircleRepository.Verify(x => x.AddAsync(It.IsAny<Circle>()), Times.Once);
        }

        [Fact]
        public async Task GetCircleAsync_WhenUserIsMember_ShouldReturnCircle() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 1);
            circle.AddMember(1, CircleRole.Owner, MemberStatus.Active);

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            // Act
            var result = await service.GetCircleAsync(circleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Circle", result.Name);
        }

        [Fact]
        public async Task GetCircleAsync_WhenUserIsNotMember_ShouldThrowException() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 2); // Different owner
            circle.AddMember(2, CircleRole.Owner, MemberStatus.Active);

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns("1"); // Different user

            // Act & Assert
            await Assert.ThrowsAsync<CircleNotFoundException>(() => service.GetCircleAsync(circleId));
        }

        [Fact]
        public async Task InviteToCircleAsync_WhenUserIsOwner_ShouldCreateInvite() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 1);
            circle.AddMember(1, CircleRole.Owner, MemberStatus.Active);

            var inviteDto = new InviteToCircleDto {
                InviteeEmail = "test@example.com"
            };

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            // Act
            var result = await service.InviteToCircleAsync(circleId, inviteDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.InviteeEmail);
            Assert.Equal(InviteStatus.Pending, result.Status);
        }

        [Fact]
        public async Task InviteToCircleAsync_WhenUserIsMember_ShouldThrowException() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 2);
            circle.AddMember(2, CircleRole.Owner, MemberStatus.Active);
            circle.AddMember(1, CircleRole.Member, MemberStatus.Active);

            var inviteDto = new InviteToCircleDto {
                InviteeEmail = "test@example.com"
            };

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                service.InviteToCircleAsync(circleId, inviteDto));
        }

        [Fact]
        public async Task DeleteCircleAsync_WhenUserIsOwner_ShouldDeleteCircle() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 1);
            circle.AddMember(1, CircleRole.Owner, MemberStatus.Active);

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            // Act
            await service.DeleteCircleAsync(circleId);

            // Assert
            mockCircleRepository.Verify(x => x.RemoveAsync(circle), Times.Once);
        }

        [Fact]
        public async Task DeleteCircleAsync_WhenUserIsNotOwner_ShouldThrowException() {
            // Arrange
            var circleId = Guid.NewGuid();
            var circle = new Circle("Test Circle", "Test", 2); // Different owner
            circle.AddMember(2, CircleRole.Owner, MemberStatus.Active);
            circle.AddMember(1, CircleRole.Admin, MemberStatus.Active);

            mockCircleRepository
                .Setup(x => x.GetAsync(circleId))
                .ReturnsAsync(circle);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                service.DeleteCircleAsync(circleId));
        }
    }
}
