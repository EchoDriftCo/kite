using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class DietaryProfileRepository : IDietaryProfileRepository {
        private readonly IRecipeVaultDbContext context;

        public DietaryProfileRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DietaryProfile> AddAsync(DietaryProfile profile) {
            var entity = await context.DietaryProfiles.AddAsync(profile);
            return entity.Entity;
        }

        public Task<DietaryProfile> GetAsync(Guid profileResourceId) {
            return context.DietaryProfiles
                .Include(x => x.Restrictions)
                .Include(x => x.AvoidedIngredients)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(x => x.DietaryProfileResourceId == profileResourceId);
        }

        public Task<DietaryProfile> GetByIdAsync(int profileId) {
            return context.DietaryProfiles
                .Include(x => x.Restrictions)
                .Include(x => x.AvoidedIngredients)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(x => x.DietaryProfileId == profileId);
        }

        public Task<List<DietaryProfile>> GetProfilesBySubjectAsync(Guid subjectId) {
            return context.DietaryProfiles
                .Include(x => x.Restrictions)
                .Include(x => x.AvoidedIngredients)
                .Where(x => x.SubjectId == subjectId)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ProfileName)
                .ToListAsync();
        }

        public Task<DietaryProfile> GetDefaultProfileBySubjectAsync(Guid subjectId) {
            return context.DietaryProfiles
                .Include(x => x.Restrictions)
                .Include(x => x.AvoidedIngredients)
                .FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.IsDefault);
        }

        public Task RemoveAsync(DietaryProfile profile) {
            context.RemoveRange(profile.Restrictions);
            context.RemoveRange(profile.AvoidedIngredients);
            context.Remove(profile);
            return Task.CompletedTask;
        }

        public Task<DietaryRestriction> GetRestrictionAsync(int profileId, string restrictionCode) {
            return context.DietaryRestrictions
                .FirstOrDefaultAsync(x => x.DietaryProfileId == profileId && x.RestrictionCode == restrictionCode);
        }

        public Task<AvoidedIngredient> GetAvoidedIngredientAsync(int profileId, int avoidedIngredientId) {
            return context.AvoidedIngredients
                .FirstOrDefaultAsync(x => x.DietaryProfileId == profileId && x.AvoidedIngredientId == avoidedIngredientId);
        }
    }
}
