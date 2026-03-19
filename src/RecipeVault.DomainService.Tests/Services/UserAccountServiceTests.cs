using System;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class UserAccountServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private UserAccountService CreateService(Mock<IUserAccountRepository> mockRepository) {
            var mockLogger = CreateMockLogger<UserAccountService>();
            return new UserAccountService(mockRepository.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetOrCreateAccountAsync_WhenAccountExists_ReturnsExistingAccount() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.GetOrCreateAccountAsync(TestSubjectId);

            // Assert
            result.ShouldNotBeNull();
            result.SubjectId.ShouldBe(TestSubjectId);
            result.AccountTier.ShouldBe(AccountTier.Free);
            mockRepository.Verify(x => x.AddAsync(It.IsAny<UserAccount>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateAccountAsync_WhenNoAccount_CreatesNewFreeAccount() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync((UserAccount)null)
                .Verifiable();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<UserAccount>()))
                .ReturnsAsync((UserAccount account) => account)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.GetOrCreateAccountAsync(TestSubjectId);

            // Assert
            result.ShouldNotBeNull();
            result.SubjectId.ShouldBe(TestSubjectId);
            result.AccountTier.ShouldBe(AccountTier.Free);
            result.UserAccountResourceId.ShouldNotBe(Guid.Empty);
            mockRepository.Verify(x => x.AddAsync(It.IsAny<UserAccount>()), Times.Once);
        }

        [Fact]
        public async Task SetTierAsync_UpdatesTierAndReturnsAccount() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.SetTierAsync(TestSubjectId, AccountTier.Premium);

            // Assert
            result.ShouldNotBeNull();
            result.AccountTier.ShouldBe(AccountTier.Premium);
            result.TierChangedDate.ShouldNotBeNull();
        }

        [Fact]
        public async Task SetTierAsync_SameTier_DoesNotUpdateTierChangedDate() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.SetTierAsync(TestSubjectId, AccountTier.Free);

            // Assert
            result.ShouldNotBeNull();
            result.AccountTier.ShouldBe(AccountTier.Free);
            result.TierChangedDate.ShouldBeNull();
        }

        [Fact]
        public async Task GetTierAsync_WhenAccountExists_ReturnsTier() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);
            existingAccount.SetTier(AccountTier.Premium);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.GetTierAsync(TestSubjectId);

            // Assert
            result.ShouldBe(AccountTier.Premium);
        }

        [Fact]
        public async Task GetTierAsync_WhenNoAccount_ReturnsFree() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync((UserAccount)null)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.GetTierAsync(TestSubjectId);

            // Assert
            result.ShouldBe(AccountTier.Free);
        }

        [Fact]
        public async Task IsPremiumOrBetaAsync_WithPremium_ReturnsTrue() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);
            existingAccount.SetTier(AccountTier.Premium);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.IsPremiumOrBetaAsync(TestSubjectId);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task IsPremiumOrBetaAsync_WithBeta_ReturnsTrue() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);
            existingAccount.SetTier(AccountTier.Beta);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.IsPremiumOrBetaAsync(TestSubjectId);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task IsPremiumOrBetaAsync_WithFree_ReturnsFalse() {
            // Arrange
            var mockRepository = MockRepository.Create<IUserAccountRepository>();
            var existingAccount = new UserAccount(TestSubjectId);

            mockRepository
                .Setup(x => x.GetBySubjectIdAsync(TestSubjectId))
                .ReturnsAsync(existingAccount)
                .Verifiable();

            var service = CreateService(mockRepository);

            // Act
            var result = await service.IsPremiumOrBetaAsync(TestSubjectId);

            // Assert
            result.ShouldBeFalse();
        }
    }
}
