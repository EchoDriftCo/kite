using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class DietaryProfileService : IDietaryProfileService {
        private readonly ILogger<DietaryProfileService> logger;
        private readonly IDietaryProfileRepository profileRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public DietaryProfileService(
            IDietaryProfileRepository profileRepository,
            ILogger<DietaryProfileService> logger,
            ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.profileRepository = profileRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<DietaryProfile> CreateProfileAsync(UpdateDietaryProfileDto dto) {
            // If setting as default, unset any existing default profile
            if (dto.IsDefault) {
                await UnsetDefaultProfileAsync().ConfigureAwait(false);
            }

            var entity = new DietaryProfile(CurrentSubjectId, dto.ProfileName, dto.IsDefault);

            using (logger.PushProperty("DietaryProfileResourceId", entity.DietaryProfileResourceId)) {
                await profileRepository.AddAsync(entity);
                logger.LogInformation("Created new dietary profile");
                return entity;
            }
        }

        public async Task<DietaryProfile> GetProfileAsync(Guid profileResourceId) {
            var entity = await profileRepository.GetAsync(profileResourceId).ConfigureAwait(false);
            if (entity == null || entity.SubjectId != CurrentSubjectId) {
                throw new DietaryProfileNotFoundException($"Dietary profile with id {profileResourceId} not found");
            }

            return entity;
        }

        public Task<List<DietaryProfile>> GetProfilesBySubjectAsync() {
            return profileRepository.GetProfilesBySubjectAsync(CurrentSubjectId);
        }

        public async Task<DietaryProfile> UpdateProfileAsync(Guid resourceId, UpdateDietaryProfileDto dto) {
            var entity = await GetProfileAsync(resourceId).ConfigureAwait(false);

            // If setting as default, unset any existing default profile
            if (dto.IsDefault && !entity.IsDefault) {
                await UnsetDefaultProfileAsync().ConfigureAwait(false);
            }

            using (logger.PushProperty("DietaryProfileResourceId", entity.DietaryProfileResourceId)) {
                entity.Update(dto.ProfileName, dto.IsDefault);
                logger.LogInformation("Updated dietary profile");
                return entity;
            }
        }

        public async Task DeleteProfileAsync(Guid resourceId) {
            var entity = await GetProfileAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("DietaryProfileResourceId", entity.DietaryProfileResourceId)) {
                await profileRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted dietary profile");
            }
        }

        public async Task<DietaryRestriction> AddRestrictionAsync(Guid profileResourceId, AddDietaryRestrictionDto dto) {
            var profile = await GetProfileAsync(profileResourceId).ConfigureAwait(false);

            // Check if restriction already exists
            var existing = await profileRepository.GetRestrictionAsync(profile.DietaryProfileId, dto.RestrictionCode).ConfigureAwait(false);
            if (existing != null) {
                throw new InvalidOperationException($"Restriction '{dto.RestrictionCode}' already exists in this profile");
            }

            using (logger.PushProperty("DietaryProfileResourceId", profile.DietaryProfileResourceId)) {
                var restriction = profile.AddRestriction(dto.RestrictionCode, dto.RestrictionType, dto.Severity);
                logger.LogInformation("Added restriction {RestrictionCode} to dietary profile", dto.RestrictionCode);
                return restriction;
            }
        }

        public async Task RemoveRestrictionAsync(Guid profileResourceId, string restrictionCode) {
            var profile = await GetProfileAsync(profileResourceId).ConfigureAwait(false);

            var restriction = await profileRepository.GetRestrictionAsync(profile.DietaryProfileId, restrictionCode).ConfigureAwait(false);
            if (restriction == null) {
                throw new DietaryRestrictionNotFoundException($"Restriction '{restrictionCode}' not found in profile");
            }

            using (logger.PushProperty("DietaryProfileResourceId", profile.DietaryProfileResourceId)) {
                profile.RemoveRestriction(restriction);
                logger.LogInformation("Removed restriction {RestrictionCode} from dietary profile", restrictionCode);
            }
        }

        public async Task<AvoidedIngredient> AddAvoidedIngredientAsync(Guid profileResourceId, AddAvoidedIngredientDto dto) {
            var profile = await GetProfileAsync(profileResourceId).ConfigureAwait(false);

            using (logger.PushProperty("DietaryProfileResourceId", profile.DietaryProfileResourceId)) {
                var avoidedIngredient = profile.AddAvoidedIngredient(dto.IngredientName, dto.Reason);
                logger.LogInformation("Added avoided ingredient {IngredientName} to dietary profile", dto.IngredientName);
                return avoidedIngredient;
            }
        }

        public async Task RemoveAvoidedIngredientAsync(Guid profileResourceId, int avoidedIngredientId) {
            var profile = await GetProfileAsync(profileResourceId).ConfigureAwait(false);

            var avoidedIngredient = await profileRepository.GetAvoidedIngredientAsync(profile.DietaryProfileId, avoidedIngredientId).ConfigureAwait(false);
            if (avoidedIngredient == null) {
                throw new AvoidedIngredientNotFoundException($"Avoided ingredient with id {avoidedIngredientId} not found in profile");
            }

            using (logger.PushProperty("DietaryProfileResourceId", profile.DietaryProfileResourceId)) {
                profile.RemoveAvoidedIngredient(avoidedIngredient);
                logger.LogInformation("Removed avoided ingredient {IngredientName} from dietary profile", avoidedIngredient.IngredientName);
            }
        }

        private async Task UnsetDefaultProfileAsync() {
            var profiles = await profileRepository.GetProfilesBySubjectAsync(CurrentSubjectId).ConfigureAwait(false);
            var currentDefault = profiles.FirstOrDefault(p => p.IsDefault);
            if (currentDefault != null) {
                currentDefault.Update(currentDefault.ProfileName, false);
            }
        }
    }
}
