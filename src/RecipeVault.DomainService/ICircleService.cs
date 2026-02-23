using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface ICircleService {
        Task<Circle> CreateCircleAsync(UpdateCircleDto dto);
        Task<Circle> GetCircleAsync(Guid circleResourceId);
        Task<PagedList<Circle>> SearchCirclesAsync(CircleSearch search);
        Task<Circle> UpdateCircleAsync(Guid resourceId, UpdateCircleDto dto);
        Task DeleteCircleAsync(Guid resourceId);
        
        // Member management
        Task<CircleInvite> InviteToCircleAsync(Guid circleResourceId, InviteToCircleDto dto);
        Task<Circle> AcceptInviteAsync(Guid inviteToken);
        Task<CircleInvite> GetInviteDetailsAsync(Guid inviteToken);
        Task RemoveMemberAsync(Guid circleResourceId, int subjectId);
        Task LeaveCircleAsync(Guid circleResourceId);
        
        // Recipe sharing
        Task<Circle> ShareRecipeToCircleAsync(Guid circleResourceId, ShareRecipeToCircleDto dto);
        Task UnshareRecipeFromCircleAsync(Guid circleResourceId, Guid recipeResourceId);
        Task<PagedList<Recipe>> GetCircleRecipesAsync(Guid circleResourceId, int pageNumber = 1, int pageSize = 20);
    }
}
