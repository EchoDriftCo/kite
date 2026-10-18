using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Auditable.Entities;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Facade.Mappers;
using RecipeVault.TestUtilities.Builders;
using Shouldly;
using Xunit;

namespace RecipeVault.Facade.Tests.Mappers {
    public class MealPlanMapperTests {
        [Fact]
        public void MapToDto_WhenRecipeBecomesPrivate_HidesRecipeFromPlanResponse() {
            var planOwner = new Subject(Guid.NewGuid(), "Plan Owner", "Plan", "Owner", "owner@example.com");
            var recipeOwner = new Subject(Guid.NewGuid(), "Recipe Owner", "Recipe", "Owner", "recipe@example.com");
            var recipe = new RecipeBuilder().WithTitle("Private Recipe").WithIsPublic(true).Build();
            recipe.CreatedSubject = recipeOwner;
            recipe.SetVisibility(false);

            var entry = new MealPlanEntry(DateTime.UtcNow, MealSlot.Dinner, 1, null, false);
            typeof(MealPlanEntry).GetProperty("Recipe").SetValue(entry, recipe);
            var mealPlan = new MealPlanBuilder()
                .WithEntries(new List<MealPlanEntry> { entry })
                .Build();
            mealPlan.CreatedSubject = planOwner;

            var result = new MealPlanMapper(new SubjectMapper()).MapToDto(mealPlan);

            result.Entries[0].RecipeResourceId.ShouldBe(Guid.Empty);
            result.Entries[0].RecipeTitle.ShouldBeNull();
            result.Entries[0].RecipeYield.ShouldBe(0);
        }
    }
}
