using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class BetaInviteCodeServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private BetaInviteCodeService CreateService(
            Mock<IBetaInviteCodeRepository> mockRepo = null,
            Mock<IUserAccountService> mockUserAccountService = null) {

            mockRepo ??= MockRepository.Create<IBetaInviteCodeRepository>();
            mockUserAccountService ??= MockRepository.Create<IUserAccountService>();
            var mockLogger = CreateMockLogger<BetaInviteCodeService>();

            return new BetaInviteCodeService(mockRepo.Object, mockUserAccountService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task CreateCodesAsync_WithValidDto_CreatesAndReturnsMultipleCodes() {
            // Arrange
            var dto = new CreateBetaInviteCodeDto {
                Count = 3,
                MaxUses = 5,
                ExpiresDate = DateTime.UtcNow.AddDays(30)
            };

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<BetaInviteCode>()))
                .ReturnsAsync((BetaInviteCode c) => c);
            mockRepo
                .Setup(x => x.GetByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync((BetaInviteCode)null); // No collisions

            var service = CreateService(mockRepo);

            // Act
            var results = await service.CreateCodesAsync(dto);

            // Assert
            results.ShouldNotBeNull();
            results.Count.ShouldBe(3);
            results.ForEach(result => {
                result.MaxUses.ShouldBe(5);
                result.UseCount.ShouldBe(0);
                result.IsActive.ShouldBeTrue();
                result.BetaInviteCodeResourceId.ShouldNotBe(Guid.Empty);
                result.Code.ShouldNotBeNullOrEmpty();
            });

            mockRepo.Verify(x => x.AddAsync(It.IsAny<BetaInviteCode>()), Times.Exactly(3));
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
        public async Task RedeemCodeAsync_WithValidCode_RedeemsSuccessfully() {
            // Arrange
            var code = new BetaInviteCode("REDEEM", 5, DateTime.UtcNow.AddDays(30));
            typeof(BetaInviteCode).GetProperty("BetaInviteCodeId").SetValue(code, 1);

            var mockAccount = new UserAccount(TestSubjectId);
            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            var mockUserAccountService = MockRepository.Create<IUserAccountService>();

            mockRepo.Setup(x => x.GetByCodeAsync("REDEEM")).ReturnsAsync(code);
            mockUserAccountService.Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(mockAccount);

            var service = CreateService(mockRepo, mockUserAccountService);

            // Act
            var result = await service.RedeemCodeAsync("REDEEM", TestSubjectId);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            code.UseCount.ShouldBe(1);
            code.Redemptions.Count.ShouldBe(1);
            code.Redemptions[0].SubjectId.ShouldBe(TestSubjectId);
            mockAccount.AccountTier.ShouldBe(AccountTier.Beta);
        }

        [Fact]
        public async Task RedeemCodeAsync_WithNonExistentCode_ReturnsFailure() {
            // Arrange
            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("NOTFOUND")).ReturnsAsync((BetaInviteCode)null);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("NOTFOUND", TestSubjectId);

            // Assert
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Invalid invite code");
        }

        [Fact]
        public async Task RedeemCodeAsync_WithInactiveCode_ReturnsFailure() {
            // Arrange
            var code = new BetaInviteCode("INACTIVE", 5, null);
            code.Deactivate();

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("INACTIVE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("INACTIVE", TestSubjectId);

            // Assert
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Invite code is no longer active");
        }

        [Fact]
        public async Task RedeemCodeAsync_WithExpiredCode_ReturnsFailure() {
            // Arrange
            var code = new BetaInviteCode("EXPIRED", 5, DateTime.UtcNow.AddDays(-1));

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("EXPIRED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("EXPIRED", TestSubjectId);

            // Assert
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Invite code has expired");
        }

        [Fact]
        public async Task RedeemCodeAsync_WithMaxUsesReached_ReturnsFailure() {
            // Arrange
            var code = new BetaInviteCode("MAXUSED", 1, null);
            code.IncrementUseCount(); // Now at 1/1

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("MAXUSED")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("MAXUSED", TestSubjectId);

            // Assert
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Invite code has reached its maximum number of uses");
        }

        [Fact]
        public async Task RedeemCodeAsync_WithDuplicateRedemption_ReturnsFailure() {
            // Arrange
            var code = new BetaInviteCode("DUPE", 5, null);
            typeof(BetaInviteCode).GetProperty("BetaInviteCodeId").SetValue(code, 1);
            var existingRedemption = new BetaInviteCodeRedemption(1, TestSubjectId, AccountTier.Free, AccountTier.Beta);
            code.AddRedemption(existingRedemption);

            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("DUPE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.RedeemCodeAsync("DUPE", TestSubjectId);

            // Assert
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("You have already redeemed this invite code");
        }

        [Fact]
        public async Task DeactivateCodeAsync_WithValidCode_DeactivatesCode() {
            // Arrange
            var code = new BetaInviteCode("ACTIVE", 5, null);
            var mockRepo = MockRepository.Create<IBetaInviteCodeRepository>();
            mockRepo.Setup(x => x.GetByCodeAsync("ACTIVE")).ReturnsAsync(code);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.DeactivateCodeAsync("ACTIVE");

            // Assert
            result.ShouldNotBeNull();
            result.IsActive.ShouldBeFalse();
        }
    }
}
