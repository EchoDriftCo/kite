using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class DietaryProfileFacade : IDietaryProfileFacade {
        private readonly IUnitOfWork uow;
        private readonly IDietaryProfileService dietaryProfileService;
        private readonly IDietaryConflictService conflictService;
        private readonly IRecipeRepository recipeRepository;
        private readonly IDietaryProfileRepository profileRepository;
        private readonly DietaryProfileMapper mapper;
        private readonly ILogger<DietaryProfileFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public DietaryProfileFacade(
            ILogger<DietaryProfileFacade> logger,
            IUnitOfWork uow,
            IDietaryProfileService dietaryProfileService,
            IDietaryConflictService conflictService,
            IRecipeRepository recipeRepository,
            IDietaryProfileRepository profileRepository,
            DietaryProfileMapper mapper,
            IDistributedLockProvider lockProvider,
            ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.dietaryProfileService = dietaryProfileService;
            this.conflictService = conflictService;
            this.recipeRepository = recipeRepository;
            this.profileRepository = profileRepository;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"DietaryProfileResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<DietaryProfileDto> CreateProfileAsync(UpdateDietaryProfileDto dto) {
            var profile = await dietaryProfileService.CreateProfileAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(profile);
        }

        public async Task<DietaryProfileDto> GetProfileAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var profile = await dietaryProfileService.GetProfileAsync(resourceId);
                return mapper.MapToDto(profile);
            }
        }

        public async Task<List<DietaryProfileDto>> GetProfilesBySubjectAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var profiles = await dietaryProfileService.GetProfilesBySubjectAsync().ConfigureAwait(false);
                return profiles.Select(p => mapper.MapToDto(p)).ToList();
            }
        }

        public async Task<DietaryProfileDto> UpdateProfileAsync(Guid resourceId, UpdateDietaryProfileDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var profile = await dietaryProfileService.UpdateProfileAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(profile);
            }
        }

        public async Task DeleteProfileAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await dietaryProfileService.DeleteProfileAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<DietaryProfileDto> AddRestrictionAsync(Guid profileResourceId, AddDietaryRestrictionDto dto) {
            var lockName = GetLockName(profileResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await dietaryProfileService.AddRestrictionAsync(profileResourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                // Return updated profile
                var profile = await dietaryProfileService.GetProfileAsync(profileResourceId).ConfigureAwait(false);
                return mapper.MapToDto(profile);
            }
        }

        public async Task RemoveRestrictionAsync(Guid profileResourceId, string restrictionCode) {
            var lockName = GetLockName(profileResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await dietaryProfileService.RemoveRestrictionAsync(profileResourceId, restrictionCode).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<DietaryProfileDto> AddAvoidedIngredientAsync(Guid profileResourceId, AddAvoidedIngredientDto dto) {
            var lockName = GetLockName(profileResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await dietaryProfileService.AddAvoidedIngredientAsync(profileResourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                // Return updated profile
                var profile = await dietaryProfileService.GetProfileAsync(profileResourceId).ConfigureAwait(false);
                return mapper.MapToDto(profile);
            }
        }

        public async Task RemoveAvoidedIngredientAsync(Guid profileResourceId, int avoidedIngredientId) {
            var lockName = GetLockName(profileResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await dietaryProfileService.RemoveAvoidedIngredientAsync(profileResourceId, avoidedIngredientId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<DietaryConflictCheckDto> CheckRecipeAsync(Guid recipeResourceId, Guid? profileResourceId = null) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
                if (recipe == null) {
                    throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
                }

                Domain.Entities.DietaryProfile profile;
                if (profileResourceId.HasValue) {
                    profile = await dietaryProfileService.GetProfileAsync(profileResourceId.Value).ConfigureAwait(false);
                } else {
                    // Use default profile
                    profile = await profileRepository.GetDefaultProfileBySubjectAsync(CurrentSubjectId).ConfigureAwait(false);
                    if (profile == null) {
                        throw new DietaryProfileNotFoundException("No default dietary profile found. Please create one first.");
                    }
                }

                return await conflictService.CheckRecipeAsync(recipe, profile).ConfigureAwait(false);
            }
        }
    }
}
