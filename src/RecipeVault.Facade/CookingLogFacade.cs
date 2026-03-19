using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class CookingLogFacade : ICookingLogFacade {
        private readonly ILogger<CookingLogFacade> logger;
        private readonly IUnitOfWork uow;
        private readonly ICookingLogService cookingLogService;
        private readonly CookingLogMapper mapper;

        public CookingLogFacade(
            ILogger<CookingLogFacade> logger,
            IUnitOfWork uow,
            ICookingLogService cookingLogService,
            CookingLogMapper mapper) {
            this.logger = logger;
            this.uow = uow;
            this.cookingLogService = cookingLogService;
            this.mapper = mapper;
        }

        public async Task<CookingLogDto> CreateCookingLogAsync(CreateCookingLogDto dto) {
            var cookingLog = await cookingLogService.CreateCookingLogAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(cookingLog);
        }

        public async Task<CookingLogDto> GetCookingLogAsync(Guid cookingLogResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var cookingLog = await cookingLogService.GetCookingLogAsync(cookingLogResourceId);
                return mapper.MapToDto(cookingLog);
            }
        }

        public async Task<PagedList<CookingLogDto>> GetCookingLogsAsync(int pageNumber, int pageSize) {
            await using (var tx = uow.BeginNoTracking()) {
                var cookingLogs = await cookingLogService.GetCookingLogsAsync(pageNumber, pageSize);
                return cookingLogs.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<CookingLogDto> UpdateCookingLogAsync(Guid cookingLogResourceId, UpdateCookingLogDto dto) {
            var cookingLog = await cookingLogService.UpdateCookingLogAsync(cookingLogResourceId, dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(cookingLog);
        }

        public async Task DeleteCookingLogAsync(Guid cookingLogResourceId) {
            await cookingLogService.DeleteCookingLogAsync(cookingLogResourceId).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<CookingLogDto> AddPhotosAsync(Guid cookingLogResourceId, List<CookingLogPhotoDto> photos) {
            var cookingLog = await cookingLogService.AddPhotosAsync(cookingLogResourceId, photos).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(cookingLog);
        }

        public async Task<CookingStatsDto> GetCookingStatsAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                return await cookingLogService.GetCookingStatsAsync();
            }
        }

        public async Task<List<CookingLogDto>> GetCalendarEntriesAsync(int year, int month) {
            await using (var tx = uow.BeginNoTracking()) {
                var cookingLogs = await cookingLogService.GetCalendarEntriesAsync(year, month);
                return cookingLogs.Select(x => mapper.MapToDto(x)).ToList();
            }
        }

        public async Task<List<CookingLogDto>> GetRecipeCookingHistoryAsync(Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var cookingLogs = await cookingLogService.GetRecipeCookingHistoryAsync(recipeResourceId);
                return cookingLogs.Select(x => mapper.MapToDto(x)).ToList();
            }
        }

        public async Task<RecipePersonalStatsDto> GetRecipePersonalStatsAsync(Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                return await cookingLogService.GetRecipePersonalStatsAsync(recipeResourceId);
            }
        }
    }
}
