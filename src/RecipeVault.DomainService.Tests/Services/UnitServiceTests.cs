using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class UnitServiceTests : DomainServiceTestBase, IDisposable {
        private readonly MemoryCache memoryCache;

        public UnitServiceTests() {
            memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose() {
            memoryCache?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static void SetupLoggerMock(Mock<ILogger<UnitService>> mockLogger) {
            mockLogger
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();
        }

        [Fact]
        public async Task MatchAsync_WithNullInput_ReturnsNoMatch() {
            // Arrange
            var mockRepository = MockRepository.Create<IUnitRepository>();
            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync(null);

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public async Task MatchAsync_WithEmptyInput_ReturnsNoMatch() {
            // Arrange
            var mockRepository = MockRepository.Create<IUnitRepository>();
            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("   ");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public async Task MatchAsync_WithExactNameMatch_ReturnsExactMatch() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("tablespoon");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(1.0m);
            result.MatchedUnit.Name.ShouldBe("tablespoon");

            mockRepository.Verify(r => r.GetAllWithAliasesAsync(), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithAbbreviationMatch_ReturnsExactMatch() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("tbsp");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(1.0m);
            result.MatchedUnit.Name.ShouldBe("tablespoon");
        }

        [Fact]
        public async Task MatchAsync_WithAliasMatch_ReturnsExactMatch() {
            // Arrange
            var unit = new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1);
            unit.AddAlias("T", false);
            unit.AddAlias("Tbsp", false);

            var units = new List<Unit> { unit };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("T");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(1.0m);
            result.MatchedUnit.Name.ShouldBe("tablespoon");
        }

        [Fact]
        public async Task MatchAsync_WithCaseInsensitiveInput_ReturnsExactMatch() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("TABLESPOON");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(1.0m);
        }

        [Fact]
        public async Task MatchAsync_WithPluralMatch_ReturnsExactMatch() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("tablespoons");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(1.0m);
        }

        [Fact]
        public async Task MatchAsync_WithFuzzyMatch_ReturnsFuzzyMatchWithConfidence() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act - typo: "tbs" instead of "tbsp" (distance 1)
            var result = await service.MatchAsync("tbs");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Confidence.ShouldBe(0.9m); // distance 1 = 1.0 - 0.1
            result.MatchedUnit.Name.ShouldBe("tablespoon");
        }

        [Fact]
        public async Task MatchAsync_WithNoMatch_ReturnsNoMatch() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync("xyz");

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeFalse();
            result.Confidence.ShouldBe(0m);
        }

        [Fact]
        public async Task MatchAsync_CachesResults() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act - call twice with same input
            var result1 = await service.MatchAsync("tbsp");
            var result2 = await service.MatchAsync("tbsp");

            // Assert - repository should only be called once due to caching
            result1.IsMatch.ShouldBeTrue();
            result2.IsMatch.ShouldBeTrue();
            mockRepository.Verify(r => r.GetAllWithAliasesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllActiveUnits() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1),
                new Unit("teaspoon", "tsp", "teaspoons", UnitType.Volume, 4.93m, null, 2)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("tablespoon");
            result[1].Name.ShouldBe("teaspoon");

            mockRepository.Verify(r => r.GetAllWithAliasesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsUnit() {
            // Arrange
            var unitId = Guid.NewGuid();
            var unit = new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1);

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetByIdAsync(unitId))
                .ReturnsAsync(unit)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.GetByIdAsync(unitId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("tablespoon");

            mockRepository.Verify(r => r.GetByIdAsync(unitId), Times.Once);
        }

        [Theory]
        [InlineData("cup", 0)]
        [InlineData("cups", 1)]
        [InlineData("cp", 1)]
        [InlineData("xyz", 3)]
        public async Task LevenshteinDistance_CalculatesCorrectly(string input, int expectedDistance) {
            // This tests the algorithm indirectly through matching
            // Distance of 3 should not match (> 2), so we test it returns no match
            var units = new List<Unit>
            {
                new Unit("cup", "cup", "cups", UnitType.Volume, 236.59m, null, 1)
            };

            var mockRepository = MockRepository.Create<IUnitRepository>();
            mockRepository
                .Setup(r => r.GetAllWithAliasesAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockLogger = CreateMockLogger<UnitService>();
            SetupLoggerMock(mockLogger);
            var service = new UnitService(mockRepository.Object, memoryCache, mockLogger.Object);

            // Act
            var result = await service.MatchAsync(input);

            // Assert
            if (expectedDistance <= 2) {
                result.IsMatch.ShouldBeTrue();
            } else {
                result.IsMatch.ShouldBeFalse();
            }
        }
    }
}
