using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class ApiTokenServiceTests : DomainServiceTestBase {
        private static readonly Guid SubjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private ApiTokenService CreateService(
            Mock<IApiTokenRepository> mockApiTokenRepository,
            Mock<ISubjectPrincipal> mockSubjectPrincipal = null) {
            mockSubjectPrincipal ??= CreateDefaultSubjectPrincipal();
            var mockLogger = CreateMockLogger<ApiTokenService>();
            return new ApiTokenService(
                mockApiTokenRepository.Object,
                mockSubjectPrincipal.Object,
                mockLogger.Object);
        }

        private Mock<ISubjectPrincipal> CreateDefaultSubjectPrincipal() {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            mock.Setup(x => x.SubjectId).Returns(SubjectId.ToString());
            return mock;
        }

        [Fact]
        public async Task CreateTokenAsync_GeneratesTokenWithRvPrefix() {
            // Arrange
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateTokenAsync("Chrome Extension", 365);

            // Assert
            result.ShouldNotBeNull();
            result.Token.ShouldStartWith("rv_");
            result.Name.ShouldBe("Chrome Extension");
            result.TokenPrefix.ShouldStartWith("rv_");
            result.TokenPrefix.Length.ShouldBe(10);
            result.ApiTokenResourceId.ShouldNotBe(Guid.Empty);
            result.ExpiresDate.ShouldNotBeNull();

            mockRepo.Verify(x => x.AddAsync(It.IsAny<ApiToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTokenAsync_StoresHashNotPlaintext() {
            // Arrange
            ApiToken savedToken = null;
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .Callback<ApiToken>(t => savedToken = t)
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateTokenAsync("Test Token", null);

            // Assert
            savedToken.ShouldNotBeNull();
            savedToken.TokenHash.ShouldNotBe(result.Token); // Hash, not plaintext
            savedToken.TokenHash.Length.ShouldBe(64); // SHA-256 hex string
        }

        [Fact]
        public async Task CreateTokenAsync_WithNoExpiry_SetsNullExpiresDate() {
            // Arrange
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateTokenAsync("Test Token", null);

            // Assert
            result.ExpiresDate.ShouldBeNull();
        }

        [Fact]
        public async Task CreateTokenAsync_SetsCorrectExpiryDate() {
            // Arrange
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateTokenAsync("Test Token", 30);

            // Assert
            result.ExpiresDate.ShouldNotBeNull();
            result.ExpiresDate.Value.ShouldBeGreaterThan(DateTime.UtcNow.AddDays(29));
            result.ExpiresDate.Value.ShouldBeLessThan(DateTime.UtcNow.AddDays(31));
        }

        [Fact]
        public async Task CreateTokenAsync_CreatesUniqueResourceId() {
            // Arrange
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result1 = await service.CreateTokenAsync("Token 1", null);
            var result2 = await service.CreateTokenAsync("Token 2", null);

            // Assert
            result1.ApiTokenResourceId.ShouldNotBe(result2.ApiTokenResourceId);
        }

        [Fact]
        public async Task ListTokensAsync_ReturnsTokensForCurrentUser() {
            // Arrange
            var tokens = new List<ApiToken> {
                new ApiToken(Guid.NewGuid(), SubjectId, "Token 1", "hash1", "rv_abc123", null),
                new ApiToken(Guid.NewGuid(), SubjectId, "Token 2", "hash2", "rv_def456", DateTime.UtcNow.AddDays(30))
            };

            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.GetBySubjectIdAsync(SubjectId))
                .ReturnsAsync(tokens)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.ListTokensAsync();

            // Assert
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("Token 1");
            result[1].Name.ShouldBe("Token 2");
        }

        [Fact]
        public async Task RevokeTokenAsync_RevokesOwnedToken() {
            // Arrange
            var tokenResourceId = Guid.NewGuid();
            var token = new ApiToken(tokenResourceId, SubjectId, "Test", "hash", "rv_test12", null);

            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.GetAsync(tokenResourceId))
                .ReturnsAsync(token)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            await service.RevokeTokenAsync(tokenResourceId);

            // Assert
            token.IsRevoked.ShouldBeTrue();
        }

        [Fact]
        public async Task RevokeTokenAsync_WhenTokenNotFound_ThrowsException() {
            // Arrange
            var tokenResourceId = Guid.NewGuid();
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.GetAsync(tokenResourceId))
                .ReturnsAsync((ApiToken)null)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(
                () => service.RevokeTokenAsync(tokenResourceId));
        }

        [Fact]
        public async Task RevokeTokenAsync_WhenTokenBelongsToDifferentUser_ThrowsException() {
            // Arrange
            var tokenResourceId = Guid.NewGuid();
            var otherSubjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var token = new ApiToken(tokenResourceId, otherSubjectId, "Test", "hash", "rv_test12", null);

            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.GetAsync(tokenResourceId))
                .ReturnsAsync(token)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(
                () => service.RevokeTokenAsync(tokenResourceId));
        }

        [Fact]
        public async Task CreateTokenAsync_TokenAndHashAreConsistent() {
            // Arrange
            ApiToken savedToken = null;
            var mockRepo = MockRepository.Create<IApiTokenRepository>();
            mockRepo
                .Setup(x => x.AddAsync(It.IsAny<ApiToken>()))
                .Callback<ApiToken>(t => savedToken = t)
                .ReturnsAsync((ApiToken t) => t)
                .Verifiable();

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateTokenAsync("Test", null);

            // Assert
            // The returned token should start with rv_
            result.Token.ShouldStartWith("rv_");
            // The stored hash should be 64 chars (SHA-256 hex)
            savedToken.TokenHash.Length.ShouldBe(64);
            // Token prefix should match start of token
            result.Token[..10].ShouldBe(savedToken.TokenPrefix);
        }
    }
}
