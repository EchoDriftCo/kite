using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Data.Tests.Repositories {
    public class UnitRepositoryTests : IAsyncLifetime {
        private RecipeVaultDbContext dbContext;

        public async Task InitializeAsync() {
            var options = new DbContextOptionsBuilder<RecipeVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            dbContext = new RecipeVaultDbContext(options, null, null);
            await dbContext.Database.EnsureCreatedAsync();

            // Seed test data
            await SeedTestDataAsync();
        }

        public async Task DisposeAsync() {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.DisposeAsync();
        }

        private async Task SeedTestDataAsync() {
            var systemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var tablespoon = new Unit("tablespoon", "tbsp", "tablespoons", UnitType.Volume, 14.79m, null, 1);
            tablespoon.AddAlias("T", false);
            tablespoon.AddAlias("Tbsp", false);

            var teaspoon = new Unit("teaspoon", "tsp", "teaspoons", UnitType.Volume, 4.93m, null, 2);
            teaspoon.AddAlias("t", false);

            var cup = new Unit("cup", "cup", "cups", UnitType.Volume, 236.59m, null, 3);
            cup.AddAlias("c", false);

            dbContext.Units.AddRange(tablespoon, teaspoon, cup);
            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllWithAliasesAsync_ReturnsAllActiveUnits() {
            // Arrange
            var repository = new UnitRepository(dbContext);

            // Act
            var result = await repository.GetAllWithAliasesAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result.ShouldContain(u => u.Name == "tablespoon");
            result.ShouldContain(u => u.Name == "teaspoon");
            result.ShouldContain(u => u.Name == "cup");
        }

        [Fact]
        public async Task GetAllWithAliasesAsync_IncludesAliases() {
            // Arrange
            var repository = new UnitRepository(dbContext);

            // Act
            var result = await repository.GetAllWithAliasesAsync();

            // Assert
            var tablespoon = result.First(u => u.Name == "tablespoon");
            tablespoon.Aliases.ShouldNotBeNull();
            tablespoon.Aliases.Count.ShouldBe(2);
            tablespoon.Aliases.ShouldContain(a => a.Alias == "T");
            tablespoon.Aliases.ShouldContain(a => a.Alias == "Tbsp");
        }

        [Fact]
        public async Task GetAllWithAliasesAsync_OrdersBySortOrder() {
            // Arrange
            var repository = new UnitRepository(dbContext);

            // Act
            var result = await repository.GetAllWithAliasesAsync();

            // Assert
            result[0].Name.ShouldBe("tablespoon");
            result[1].Name.ShouldBe("teaspoon");
            result[2].Name.ShouldBe("cup");
        }

        [Fact]
        public async Task GetByIdAsync_WithValidGuid_ReturnsUnit() {
            // Arrange
            var repository = new UnitRepository(dbContext);
            var existingUnit = dbContext.Units.First(u => u.Name == "tablespoon");

            // Act
            var result = await repository.GetByIdAsync(existingUnit.UnitResourceId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("tablespoon");
            result.Aliases.ShouldNotBeNull();
            result.Aliases.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidGuid_ReturnsNull() {
            // Arrange
            var repository = new UnitRepository(dbContext);

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithIntId_ReturnsUnit() {
            // Arrange
            var repository = new UnitRepository(dbContext);
            var existingUnit = dbContext.Units.First(u => u.Name == "teaspoon");

            // Act
            var result = await repository.GetByIdAsync(existingUnit.UnitId);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("teaspoon");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidIntId_ReturnsNull() {
            // Arrange
            var repository = new UnitRepository(dbContext);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.ShouldBeNull();
        }
    }
}
