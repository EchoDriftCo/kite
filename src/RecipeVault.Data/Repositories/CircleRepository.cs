using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class CircleRepository : ICircleRepository {
        private readonly IRecipeVaultDbContext context;

        public CircleRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<Circle>> SearchAsync(CircleSearch model) {
            var circles = model.Build(context.Circles
                .Include(x => x.Members)
                    .ThenInclude(m => m.Subject)
                .Include(x => x.SharedRecipes)
                    .ThenInclude(sr => sr.Recipe)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject));

            var result = new PagedList<Circle> {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                TotalItems = await circles.CountAsync().ConfigureAwait(false),
                Items = [],
            };

            circles = circles.ToSortedQuery(model.Sort);
            result.Items = await circles.ToPagedQuery(model.PageNumber, model.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<Circle> AddAsync(Circle circle) {
            var entity = await context.Circles.AddAsync(circle);
            return entity.Entity;
        }

        public Task<Circle> GetAsync(Guid id) {
            return context.Circles
                .Include(x => x.Members)
                    .ThenInclude(m => m.Subject)
                .Include(x => x.SharedRecipes)
                    .ThenInclude(sr => sr.Recipe)
                .Include(x => x.Invites)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(x => x.CircleResourceId == id);
        }

        public Task RemoveAsync(Circle circle) {
            context.RemoveRange(circle.Members);
            context.RemoveRange(circle.SharedRecipes);
            context.RemoveRange(circle.Invites);
            context.Remove(circle);
            return Task.CompletedTask;
        }

        public Task<CircleInvite> GetInviteByTokenAsync(Guid inviteToken) {
            return context.CircleInvites
                .Include(x => x.Circle)
                    .ThenInclude(c => c.Members)
                        .ThenInclude(m => m.Subject)
                .FirstOrDefaultAsync(x => x.InviteToken == inviteToken);
        }

        public Task<CircleRecipe> GetCircleRecipeAsync(int circleId, int recipeId) {
            return context.CircleRecipes
                .FirstOrDefaultAsync(x => x.CircleId == circleId && x.RecipeId == recipeId);
        }

        public Task<CircleMember> GetMemberAsync(int circleId, Guid subjectId) {
            return context.CircleMembers
                .FirstOrDefaultAsync(x => x.CircleId == circleId && x.SubjectId == subjectId);
        }
    }
}
