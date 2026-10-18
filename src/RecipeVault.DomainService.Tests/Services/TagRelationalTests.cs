using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Cortside.AspNetCore.Auditable;
using Cortside.Common.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using Shouldly;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class TagRelationalTests {
        [Fact]
        public async Task SaveTags_WithSameNameAndCategoryForDifferentOwners_Succeeds() {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<RecipeVaultDbContext>()
                .UseSqlite(connection)
                .Options;

            var aliceId = Guid.NewGuid();
            var bobId = Guid.NewGuid();
            var globalOwnerId = Guid.NewGuid();
            await using (var context = CreateContext(options, aliceId)) {
                await context.Database.EnsureCreatedAsync();
                context.Tags.Add(new Tag("Family", TagCategory.Custom, false));
                await context.SaveChangesAsync();
            }

            await using (var context = CreateContext(options, bobId)) {
                context.Tags.Add(new Tag("Family", TagCategory.Custom, false));
                await context.SaveChangesAsync();
            }

            await using (var context = CreateContext(options, globalOwnerId)) {
                context.Tags.Add(new Tag("Family", TagCategory.Custom, true));
                await context.SaveChangesAsync();
                await Should.ThrowAsync<DbUpdateException>(async () => {
                    context.Tags.Add(new Tag("Family", TagCategory.Custom, true));
                    await context.SaveChangesAsync();
                });
            }

            await using var verifyContext = CreateContext(options, Guid.NewGuid());
            (await verifyContext.Tags.CountAsync()).ShouldBe(3);
        }

        private static RecipeVaultDbContext CreateContext(DbContextOptions<RecipeVaultDbContext> options, Guid subjectId) {
            var principal = new SubjectPrincipal(new List<Claim> { new("sub", subjectId.ToString()) });
            return new RecipeVaultDbContext(options, principal, new DefaultSubjectFactory());
        }
    }
}
