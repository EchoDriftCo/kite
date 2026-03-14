using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class UserPantryFacade : IUserPantryFacade {
        private readonly IUnitOfWork uow;
        private readonly IUserPantryService userPantryService;
        private readonly UserPantryMapper mapper;
        private readonly ILogger<UserPantryFacade> logger;

        public UserPantryFacade(
            ILogger<UserPantryFacade> logger,
            IUnitOfWork uow,
            IUserPantryService userPantryService,
            UserPantryMapper mapper) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.userPantryService = userPantryService ?? throw new ArgumentNullException(nameof(userPantryService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<UserPantryItemDto>> GetUserPantryAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var items = await userPantryService.GetUserPantryAsync().ConfigureAwait(false);
                return items.Select(mapper.MapToDto).ToList();
            }
        }

        public async Task<UserPantryItemDto> AddPantryItemAsync(CreatePantryItemDto dto) {
            var item = await userPantryService.AddPantryItemAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(item);
        }

        public async Task<UserPantryItemDto> UpdatePantryItemAsync(int id, UpdatePantryItemDto dto) {
            var item = await userPantryService.UpdatePantryItemAsync(id, dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(item);
        }

        public async Task DeletePantryItemAsync(int id) {
            await userPantryService.DeletePantryItemAsync(id).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<List<string>> GetDefaultStaplesAsync() {
            return userPantryService.GetDefaultStaplesAsync();
        }
    }
}
