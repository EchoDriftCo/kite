using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface ICircleRepository {
        Task<Circle> AddAsync(Circle circle);
        Task<Circle> GetAsync(Guid id);
        Task<PagedList<Circle>> SearchAsync(CircleSearch model);
        Task RemoveAsync(Circle circle);
        Task<CircleInvite> GetInviteByTokenAsync(Guid inviteToken);
        Task<CircleRecipe> GetCircleRecipeAsync(int circleId, int recipeId);
        Task<CircleMember> GetMemberAsync(int circleId, int subjectId);
    }
}
