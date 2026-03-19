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
    public class EquipmentFacade : IEquipmentFacade {
        private readonly IUnitOfWork uow;
        private readonly IEquipmentService equipmentService;
        private readonly IRecipeService recipeService;
        private readonly EquipmentMapper mapper;
        private readonly ILogger<EquipmentFacade> logger;

        public EquipmentFacade(ILogger<EquipmentFacade> logger, IUnitOfWork uow, IEquipmentService equipmentService, IRecipeService recipeService, EquipmentMapper mapper) {
            this.uow = uow;
            this.equipmentService = equipmentService;
            this.recipeService = recipeService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<List<EquipmentDto>> GetAllEquipmentAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var equipment = await equipmentService.GetAllEquipmentAsync().ConfigureAwait(false);
                return equipment.Select(mapper.MapToDto).ToList();
            }
        }

        public async Task<List<UserEquipmentDto>> GetUserEquipmentAsync(Guid subjectId) {
            await using (var tx = uow.BeginNoTracking()) {
                var userEquipment = await equipmentService.GetUserEquipmentAsync(subjectId).ConfigureAwait(false);
                return userEquipment.Select(mapper.MapToDto).ToList();
            }
        }

        public async Task<UserEquipmentDto> AddUserEquipmentAsync(Guid subjectId, AddUserEquipmentDto dto) {
            var userEquipment = await equipmentService.AddUserEquipmentAsync(subjectId, dto.EquipmentCode).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);

            // Reload to get Equipment navigation property
            var reloaded = await equipmentService.GetUserEquipmentAsync(subjectId).ConfigureAwait(false);
            var added = reloaded.FirstOrDefault(ue => ue.UserEquipmentId == userEquipment.UserEquipmentId);
            return mapper.MapToDto(added);
        }

        public async Task RemoveUserEquipmentAsync(Guid subjectId, string equipmentCode) {
            await equipmentService.RemoveUserEquipmentAsync(subjectId, equipmentCode).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<RecipeEquipmentDto>> GetRecipeEquipmentAsync(Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);
                var recipeEquipment = await equipmentService.GetRecipeEquipmentAsync(recipe.RecipeId).ConfigureAwait(false);
                return recipeEquipment.Select(mapper.MapToDto).ToList();
            }
        }

        public async Task<List<string>> DetectEquipmentFromInstructionsAsync(Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);
                return await equipmentService.DetectEquipmentFromInstructionsAsync(recipe.RecipeId).ConfigureAwait(false);
            }
        }

        public async Task<EquipmentCheckDto> CheckRecipeEquipmentAsync(Guid subjectId, Guid recipeResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);
                var recipeEquipment = await equipmentService.GetRecipeEquipmentAsync(recipe.RecipeId).ConfigureAwait(false);
                var userEquipment = await equipmentService.GetUserEquipmentAsync(subjectId).ConfigureAwait(false);
                var userEquipmentIds = userEquipment.Select(ue => ue.EquipmentId).ToHashSet();

                var requiredEquipment = recipeEquipment.Where(re => re.IsRequired).ToList();
                var optionalEquipment = recipeEquipment.Where(re => !re.IsRequired).ToList();
                var missingEquipment = requiredEquipment.Where(re => !userEquipmentIds.Contains(re.EquipmentId)).ToList();

                return new EquipmentCheckDto {
                    HasAllRequired = missingEquipment.Count == 0,
                    RequiredEquipment = requiredEquipment.Select(re => mapper.MapToDto(re.Equipment)).ToList(),
                    MissingEquipment = missingEquipment.Select(re => mapper.MapToDto(re.Equipment)).ToList(),
                    OptionalEquipment = optionalEquipment.Select(re => mapper.MapToDto(re.Equipment)).ToList()
                };
            }
        }
    }
}
