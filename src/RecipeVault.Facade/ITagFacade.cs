using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface ITagFacade {
        Task<TagDto> CreateTagAsync(UpdateTagDto dto);
        Task<TagDto> GetTagAsync(Guid tagResourceId);
        Task<PagedList<TagDto>> SearchTagsAsync(TagSearchDto search);
        Task<TagDto> UpdateTagAsync(Guid tagResourceId, UpdateTagDto dto);
        Task DeleteTagAsync(Guid tagResourceId);
    }
}
