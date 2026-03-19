using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecipeVault.Data;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;

namespace RecipeVault.WebApi.Services {
    /// <summary>
    /// Seeds the system account and sample recipes on startup if they don't already exist.
    /// </summary>
    public class OnboardingSeedService : IHostedService {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<OnboardingSeedService> logger;

        public OnboardingSeedService(
            IServiceScopeFactory scopeFactory,
            ILogger<OnboardingSeedService> logger) {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            using var scope = scopeFactory.CreateScope();
            var userAccountRepository = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();
            var recipeRepository = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<Cortside.AspNetCore.EntityFramework.IUnitOfWork>();

            var systemSubjectId = SampleRecipeSeedData.SystemSubjectId;

            // Ensure system account exists
            var systemAccount = await userAccountRepository.GetBySubjectIdAsync(systemSubjectId).ConfigureAwait(false);
            if (systemAccount == null) {
                systemAccount = new UserAccount(systemSubjectId);
                await userAccountRepository.AddAsync(systemAccount).ConfigureAwait(false);
                logger.LogInformation("Created system account for onboarding seed data ({SubjectId})", systemSubjectId);
            }

            // Check if sample recipes already exist
            var existingRecipes = await recipeRepository.GetSampleRecipesAsync(systemSubjectId).ConfigureAwait(false);
            if (existingRecipes.Count > 0) {
                logger.LogInformation("Sample recipes already seeded ({Count} found), skipping", existingRecipes.Count);
                return;
            }

            // Seed sample recipes
            var definitions = SampleRecipeSeedData.GetSampleRecipes();
            Recipe bibimbapRecipe = null;

            foreach (var def in definitions) {
                // Skip the forked recipe on first pass — seed it after bibimbap is saved
                if (def.IsForkedFromBibimbap) {
                    continue;
                }

                var recipe = CreateRecipeFromDefinition(def);
                await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

                if (def.Title == "Korean Bibimbap Bowl") {
                    bibimbapRecipe = recipe;
                }
            }

            // Now seed the forked recipe
            var forkDef = definitions.Find(d => d.IsForkedFromBibimbap);
            if (forkDef != null) {
                var forkRecipe = CreateRecipeFromDefinition(forkDef);
                await recipeRepository.AddAsync(forkRecipe).ConfigureAwait(false);
            }

            await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Seeded {Count} sample recipes for onboarding", definitions.Count);
        }

        private static Recipe CreateRecipeFromDefinition(SampleRecipeDefinition def) {
            var recipe = new Recipe(
                def.Title,
                def.Yield,
                def.PrepTimeMinutes,
                def.CookTimeMinutes,
                def.Description,
                null,
                null,
                isPublic: true
            );

            recipe.MarkAsSample();
            recipe.SetShowcaseFeature(def.Showcases);

            recipe.SetIngredients(def.Ingredients.ConvertAll(i =>
                new RecipeIngredient(i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText)
            ));

            recipe.SetInstructions(def.Instructions.ConvertAll(i =>
                new RecipeInstruction(i.StepNumber, i.Instruction, i.Instruction)
            ));

            return recipe;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
