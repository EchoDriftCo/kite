using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class RecipeLinkFacade : IRecipeLinkFacade {
        private readonly ILogger<RecipeLinkFacade> logger;
        private readonly IUnitOfWork uow;
        private readonly IRecipeLinkService recipeLinkService;
        private readonly RecipeLinkMapper recipeLinkMapper;
        private readonly RecipeMapper recipeMapper;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeLinkFacade(
            ILogger<RecipeLinkFacade> logger,
            IUnitOfWork uow,
            IRecipeLinkService recipeLinkService,
            RecipeLinkMapper recipeLinkMapper,
            RecipeMapper recipeMapper,
            ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.uow = uow;
            this.recipeLinkService = recipeLinkService;
            this.recipeLinkMapper = recipeLinkMapper;
            this.recipeMapper = recipeMapper;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<RecipeLinkDto> CreateRecipeLinkAsync(Guid parentRecipeResourceId, CreateRecipeLinkDto dto) {
            var recipeLink = await recipeLinkService.CreateRecipeLinkAsync(parentRecipeResourceId, dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return recipeLinkMapper.MapToDto(recipeLink);
        }

        public async Task<RecipeLinkDto> UpdateRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId, UpdateRecipeLinkDto dto) {
            var recipeLink = await recipeLinkService.UpdateRecipeLinkAsync(parentRecipeResourceId, linkResourceId, dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return recipeLinkMapper.MapToDto(recipeLink);
        }

        public async Task DeleteRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId) {
            await recipeLinkService.DeleteRecipeLinkAsync(parentRecipeResourceId, linkResourceId).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<LinkedRecipeDto>> GetLinkedRecipesAsync(Guid parentRecipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var links = await recipeLinkService.GetLinkedRecipesAsync(parentRecipeResourceId).ConfigureAwait(false);
                return links.Select(l => recipeLinkMapper.MapToLinkedRecipeDto(l)).ToList();
            }
        }

        public async Task<List<UsedInRecipeDto>> GetUsedInRecipesAsync(Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var links = await recipeLinkService.GetUsedInRecipesAsync(recipeResourceId).ConfigureAwait(false);
                return links.Select(l => recipeLinkMapper.MapToUsedInRecipeDto(l)).ToList();
            }
        }

        public async Task<List<RecipeDto>> SearchLinkableRecipesAsync(string query) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipes = await recipeLinkService.SearchLinkableRecipesAsync(query).ConfigureAwait(false);
                return recipes.Select(r => recipeMapper.MapToDto(r, CurrentSubjectId)).ToList();
            }
        }
    }
}
