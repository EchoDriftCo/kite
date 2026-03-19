using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class CollectionMapper {
        public CollectionDto MapToDto(Collection entity) {
            if (entity == null) {
                return null;
            }

            return new CollectionDto {
                CollectionResourceId = entity.CollectionResourceId,
                Name = entity.Name,
                Description = entity.Description,
                CoverImageUrl = entity.CoverImageUrl,
                IsPublic = entity.IsPublic,
                IsFeatured = entity.IsFeatured,
                SortOrder = entity.SortOrder,
                RecipeCount = entity.CollectionRecipes?.Count ?? 0,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                Recipes = entity.CollectionRecipes?.OrderBy(cr => cr.SortOrder).Select(cr => new CollectionRecipeDto {
                    RecipeResourceId = cr.Recipe?.RecipeResourceId ?? default,
                    Title = cr.Recipe?.Title,
                    Description = cr.Recipe?.Description,
                    SourceImageUrl = cr.Recipe?.SourceImageUrl,
                    SortOrder = cr.SortOrder,
                    AddedDate = cr.AddedDate,
                    TotalTimeMinutes = cr.Recipe?.TotalTimeMinutes,
                    Rating = cr.Recipe?.Rating
                }).ToList()
            };
        }

        public CollectionSearch Map(CollectionSearchDto dto) {
            if (dto == null) {
                return null;
            }

            var search = new CollectionSearch {
                IsPublic = dto.IsPublic,
                IsFeatured = dto.IsFeatured,
                SearchTerm = dto.SearchTerm
            };

            search.PageNumber = dto.PageNumber;
            search.PageSize = dto.PageSize;
            search.Sort = dto.Sort;

            return search;
        }
    }
}
