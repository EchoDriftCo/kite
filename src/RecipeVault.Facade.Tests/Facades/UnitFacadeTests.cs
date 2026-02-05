using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;
using RecipeVault.Facade.Tests.Base;

namespace RecipeVault.Facade.Tests.Facades {
    public class UnitFacadeTests : FacadeTestBase {
        [Fact]
        public async Task GetAllAsync_WithValidService_ReturnsMappedUnits() {
            // Arrange
            var units = new List<Unit>
            {
                new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1),
                new Unit("teaspoon", "tsp", "teaspoons", UnitType.Volume, 4.93m, null, 2)
            };

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IUnitService>();
            var mockLogger = CreateMockLogger<UnitFacade>();
            var mapper = new UnitMapper();

            mockService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(units)
                .Verifiable();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(u => u.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            var facade = new UnitFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper);

            // Act
            var result = await facade.GetAllAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("tablespoon");
            result[1].Name.ShouldBe("teaspoon");

            mockService.Verify(s => s.GetAllAsync(), Times.Once);
            mockUow.Verify(u => u.BeginNoTracking(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedUnit() {
            // Arrange
            var unitId = Guid.NewGuid();
            var unit = new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1);

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IUnitService>();
            var mockLogger = CreateMockLogger<UnitFacade>();
            var mapper = new UnitMapper();

            mockService
                .Setup(s => s.GetByIdAsync(unitId))
                .ReturnsAsync(unit)
                .Verifiable();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(u => u.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            var facade = new UnitFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper);

            // Act
            var result = await facade.GetByIdAsync(unitId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("tablespoon");

            mockService.Verify(s => s.GetByIdAsync(unitId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull() {
            // Arrange
            var unitId = Guid.NewGuid();

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IUnitService>();
            var mockLogger = CreateMockLogger<UnitFacade>();
            var mapper = new UnitMapper();

            mockService
                .Setup(s => s.GetByIdAsync(unitId))
                .ReturnsAsync((Unit)null);

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(u => u.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            var facade = new UnitFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper);

            // Act
            var result = await facade.GetByIdAsync(unitId);

            // Assert
            result.ShouldBeNull();

            mockService.Verify(s => s.GetByIdAsync(unitId), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithValidInput_ReturnsMappedResult() {
            // Arrange
            var input = "tbsp";
            var unit = new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1);
            var matchResult = UnitMatchResult.ExactMatch(unit, input);

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IUnitService>();
            var mockLogger = CreateMockLogger<UnitFacade>();
            var mapper = new UnitMapper();

            mockService
                .Setup(s => s.MatchAsync(input))
                .ReturnsAsync(matchResult)
                .Verifiable();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(u => u.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            var facade = new UnitFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper);

            // Act
            var result = await facade.MatchAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeTrue();
            result.Unit.ShouldNotBeNull();
            result.Unit.Name.ShouldBe("tablespoon");
            result.Confidence.ShouldBe(1.0m);

            mockService.Verify(s => s.MatchAsync(input), Times.Once);
        }

        [Fact]
        public async Task MatchAsync_WithNoMatch_ReturnsMappedNoMatchResult() {
            // Arrange
            var input = "xyz";
            var matchResult = UnitMatchResult.NoMatch(input);

            var mockUow = MockRepository.Create<IUnitOfWork>();
            var mockService = MockRepository.Create<IUnitService>();
            var mockLogger = CreateMockLogger<UnitFacade>();
            var mapper = new UnitMapper();

            mockService
                .Setup(s => s.MatchAsync(input))
                .ReturnsAsync(matchResult)
                .Verifiable();

            var mockTransaction = MockRepository.Create<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.DisposeAsync())
                .Returns(new ValueTask())
                .Verifiable();

            mockUow
                .Setup(u => u.BeginNoTracking())
                .Returns(mockTransaction.Object)
                .Verifiable();

            var facade = new UnitFacade(mockLogger.Object, mockUow.Object, mockService.Object, mapper);

            // Act
            var result = await facade.MatchAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.IsMatch.ShouldBeFalse();
            result.Unit.ShouldBeNull();
            result.OriginalInput.ShouldBe(input);

            mockService.Verify(s => s.MatchAsync(input), Times.Once);
        }
    }

}
