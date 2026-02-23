using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface ICircleFacade {
        Task<CircleDto> CreateCircleAsync(UpdateCircleDto dto);
        Task<CircleDto> GetCircleAsync(Guid resourceId);
        Task<PagedList<CircleDto>> SearchCirclesAsync(CircleSearchDto search);
        Task<CircleDto> UpdateCircleAsync(Guid resourceId, UpdateCircleDto dto);
        Task DeleteCircleAsync(Guid resourceId);
        
        Task<CircleInviteDto> InviteToCircleAsync(Guid circleResourceId, InviteToCircleDto dto);
        Task<CircleDto> AcceptInviteAsync(Guid inviteToken);
        Task<CircleInviteDto> GetInviteDetailsAsync(Guid inviteToken);
        Task RemoveMemberAsync(Guid circleResourceId, int subjectId);
        Task LeaveCircleAsync(Guid circleResourceId);
        
        Task<CircleDto> ShareRecipeToCircleAsync(Guid circleResourceId, ShareRecipeToCircleDto dto);
        Task UnshareRecipeFromCircleAsync(Guid circleResourceId, Guid recipeResourceId);
        Task<PagedList<RecipeDto>> GetCircleRecipesAsync(Guid circleResourceId, int pageNumber = 1, int pageSize = 20);
    }
}
