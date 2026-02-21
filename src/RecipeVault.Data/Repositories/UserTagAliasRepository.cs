using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class UserTagAliasRepository : IUserTagAliasRepository {
        private readonly IRecipeVaultDbContext context;

        public UserTagAliasRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UserTagAlias> AddAsync(UserTagAlias userTagAlias) {
            var entity = await context.UserTagAliases.AddAsync(userTagAlias);
            return entity.Entity;
        }

        public Task<UserTagAlias> GetAsync(int userTagAliasId) {
            return context.UserTagAliases
                .Include(uta => uta.Tag)
                .Include(uta => uta.CreatedSubject)
                .Include(uta => uta.LastModifiedSubject)
                .FirstOrDefaultAsync(uta => uta.UserTagAliasId == userTagAliasId);
        }

        public Task<UserTagAlias> GetByUserAndTagAsync(Guid userId, int tagId) {
            return context.UserTagAliases
                .Include(uta => uta.Tag)
                .Include(uta => uta.CreatedSubject)
                .Include(uta => uta.LastModifiedSubject)
                .FirstOrDefaultAsync(uta => uta.UserId == userId && uta.TagId == tagId);
        }

        public async Task<List<UserTagAlias>> GetByUserIdAsync(Guid userId) {
            return await context.UserTagAliases
                .Include(uta => uta.Tag)
                .Include(uta => uta.CreatedSubject)
                .Include(uta => uta.LastModifiedSubject)
                .Where(uta => uta.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<UserTagAlias>> GetByTagIdAsync(int tagId) {
            return await context.UserTagAliases
                .Include(uta => uta.Tag)
                .Include(uta => uta.CreatedSubject)
                .Include(uta => uta.LastModifiedSubject)
                .Where(uta => uta.TagId == tagId)
                .ToListAsync();
        }

        public Task RemoveAsync(UserTagAlias userTagAlias) {
            context.Remove(userTagAlias);
            return Task.CompletedTask;
        }
    }
}
