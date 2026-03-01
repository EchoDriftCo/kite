using System;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class BetaInviteCodeServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal() {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            return mock;
        }

        private BetaInviteCodeService CreateService(
            Mock<IBetaInviteCodeRepository> mockRepo = null,
            Mock<ISubjectPrincipal> mockSubjectPrincipal = null) {

            mockRepo ??= MockRepository.Create<IBetaInviteCodeRepository>();
            mockSubjectPrincipal ??= CreateMockSubjectPrincipal();
            var mockLogger = CreateMockLogger<BetaInviteCodeService>();

            return new BetaInviteCodeService(mockRepo.Object, mockLogger.Object, mockSubjectPrincipal.Object);
        }

        [Fact]
        public async Task CreateCodeAsync_WithValidDto_CreatesAndReturnsCode() {
            // Arrange
            var dto = new CreateBetaInviteCodeDto {
                Code = "BETA2026",
                MaxUses = 5,
                ExpiresDate = DateTime.UtcNow.AddDays(30)
            };

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<BetaInviteCode>()))
                .ReturnsAsync((BetaInviteCode c) => c)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateCodeAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Code.ShouldBe("BETA2026");
            result.MaxUses.ShouldBe(5);
            result.UseCount.ShouldBe(0);
            result.IsActive.ShouldBeTrue();
            result.BetaInviteCodeResourceId.ShouldNotBe(Guid.Empty);

            mockRepo.Verify(x => x.AddAsync(It.IsAny<BetaInviteCode>()), Times.Once);
        }

        [Fact]
        public async Task ValidateCodeAsync_WithValidCode_ReturnsValid() {
            // Arrange
            var code = new BetaInviteCode("VALIDCODE", 5, DateTime.UtcNow.AddDays(30));

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("VALIDCODE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ValidateCodeAsync("VALIDCODE");

            // Assert
            result.ShouldNotBeNull();
            result.IsValid.ShouldBeTrue();
            result.Message.ShouldBe("Invite code is valid");
        }

        [Fact]
        public async Task ValidateCodeAsync_WithNonExistentCode_ReturnsInvalid() {
            // Arrange
            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("BADCODE")).ReturnsAsync((BetaInviteCode)null);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ValidateCodeAsync("BADCODE");

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Message.ShouldBe("Invalid invite code");
        }

        [Fact]
        public async Task ValidateCodeAsync_WithInactiveCode_ReturnsInvalid() {
            // Arrange
            var code = new BetaInviteCode("INACTIVE", 5, null);
            code.Deactivate();

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("INACTIVE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ValidateCodeAsync("INACTIVE");

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Message.ShouldBe("Invite code is no longer active");
        }

        [Fact]
        public async Task ValidateCodeAsync_WithExpiredCode_ReturnsInvalid() {
            // Arrange
            var code = new BetaInviteCode("EXPIRED", 5, DateTime.UtcNow.AddDays(-1));

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("EXPIRED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ValidateCodeAsync("EXPIRED");

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Message.ShouldBe("Invite code has expired");
        }

        [Fact]
        public async Task ValidateCodeAsync_WithMaxUsesReached_ReturnsInvalid() {
            // Arrange
            var code = new BetaInviteCode("MAXUSED", 1, null);
            code.IncrementUseCount(); // Now at 1/1

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("MAXUSED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ValidateCodeAsync("MAXUSED");

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Message.ShouldBe("Invite code has reached its maximum number of uses");
        }

        [Fact]
        public async Task RedeemCodeAsync_WithValidCode_RedeemsAndReturnsCode() {
            // Arrange
            var code = new BetaInviteCode("REDEEM", 5, DateTime.UtcNow.AddDays(30));
            // Set BetaInviteCodeId via reflection since it's auto-generated
            typeof(BetaInviteCode).GetProperty("BetaInviteCodeId").SetValue(code, 1);

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("REDEEM")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("REDEEM");

            // Assert
            result.ShouldNotBeNull();
            result.UseCount.ShouldBe(1);
            result.Redemptions.Count.ShouldBe(1);
            result.Redemptions[0].SubjectId.ShouldBe(TestSubjectId);
        }

        [Fact]
        public async Task RedeemCodeAsync_WithNonExistentCode_ThrowsInvalidInviteCodeException() {
            // Arrange
            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("NOTFOUND")).ReturnsAsync((BetaInviteCode)null);

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InvalidInviteCodeException>(
                () => service.RedeemCodeAsync("NOTFOUND")
            );
        }

        [Fact]
        public async Task RedeemCodeAsync_WithInactiveCode_ThrowsInvalidInviteCodeException() {
            // Arrange
            var code = new BetaInviteCode("INACTIVE", 5, null);
            code.Deactivate();

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("INACTIVE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InvalidInviteCodeException>(
                () => service.RedeemCodeAsync("INACTIVE")
            );
        }

        [Fact]
        public async Task RedeemCodeAsync_WithExpiredCode_ThrowsInviteCodeExpiredException() {
            // Arrange
            var code = new BetaInviteCode("EXPIRED", 5, DateTime.UtcNow.AddDays(-1));

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("EXPIRED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InviteCodeExpiredException>(
                () => service.RedeemCodeAsync("EXPIRED")
            );
        }

        [Fact]
        public async Task RedeemCodeAsync_WithMaxUsesReached_ThrowsInviteCodeMaxUsesReachedException() {
            // Arrange
            var code = new BetaInviteCode("MAXUSED", 1, null);
            code.IncrementUseCount(); // Now at 1/1

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("MAXUSED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InviteCodeMaxUsesReachedException>(
                () => service.RedeemCodeAsync("MAXUSED")
            );
        }
    }
}
