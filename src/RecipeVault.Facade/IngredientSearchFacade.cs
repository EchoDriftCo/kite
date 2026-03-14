using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public class IngredientSearchFacade : IIngredientSearchFacade {
        private readonly IUnitOfWork uow;
        private readonly IIngredientSearchService ingredientSearchService;
        private readonly IIngredientSuggestService ingredientSuggestService;
        private readonly ILogger<IngredientSearchFacade> logger;

        public IngredientSearchFacade(
            ILogger<IngredientSearchFacade> logger,
            IUnitOfWork uow,
            IIngredientSearchService ingredientSearchService,
            IIngredientSuggestService ingredientSuggestService) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.ingredientSearchService = ingredientSearchService ?? throw new ArgumentNullException(nameof(ingredientSearchService));
            this.ingredientSuggestService = ingredientSuggestService ?? throw new ArgumentNullException(nameof(ingredientSuggestService));
        }

        public async Task<PagedList<IngredientSearchResultDto>> SearchByIngredientsAsync(IngredientSearchRequestDto request) {
            // Use read-uncommitted for search operations (no tracking needed, but UoW needed for seeding)
            var allResults = await ingredientSearchService.SearchByIngredientsAsync(request).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);

            // Paginate
            var totalItems = allResults.Count;
            var pagedItems = allResults
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedList<IngredientSearchResultDto> {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                Items = pagedItems
            };
        }

        public async Task<List<IngredientSuggestionDto>> SuggestAsync(string query, int limit = 10) {
            await using (var tx = uow.BeginNoTracking()) {
                return await ingredientSuggestService.SuggestAsync(query, limit).ConfigureAwait(false);
            }
        }
    }
}
