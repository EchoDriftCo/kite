using System;
using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class CollectionMapper {
        /// <summary>
        /// Maps a collection as <paramref name="viewerSubjectId"/> sees it: only the recipes that viewer
        /// may open, meaning public ones and their own. A public collection can hold its owner's private
        /// recipes, or another user's recipe that has since been made private. Null means anonymous.
        /// </summary>
        public CollectionDto MapToDto(Collection entity, Guid? viewerSubjectId) {
            if (entity == null) {
                return null;
            }

            var visibleRecipes = entity.CollectionRecipes?
                .Where(cr => cr.Recipe != null && (cr.Recipe.IsPublic || (viewerSubjectId.HasValue && cr.Recipe.CreatedSubject?.SubjectId == viewerSubjectId)))
                .OrderBy(cr => cr.SortOrder)
                .ToList();

            return new CollectionDto {
                CollectionResourceId = entity.CollectionResourceId,
                Name = entity.Name,
                Description = entity.Description,
                CoverImageUrl = entity.CoverImageUrl,
                IsPublic = entity.IsPublic,
                IsFeatured = entity.IsFeatured,
                SortOrder = entity.SortOrder,
                RecipeCount = visibleRecipes?.Count ?? 0,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                Recipes = visibleRecipes?.Select(cr => new CollectionRecipeDto {
                    RecipeResourceId = cr.Recipe.RecipeResourceId,
                    Title = cr.Recipe.Title,
                    Description = cr.Recipe.Description,
                    SourceImageUrl = cr.Recipe.SourceImageUrl,
                    SortOrder = cr.SortOrder,
                    AddedDate = cr.AddedDate,
                    TotalTimeMinutes = cr.Recipe.TotalTimeMinutes,
                    Rating = cr.Recipe.Rating
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
