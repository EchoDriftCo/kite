#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class CollectionModelMapper {
        public CollectionModel Map(CollectionDto dto) {
            if (dto == null) {
                return null;
            }

            return new CollectionModel {
                CollectionResourceId = dto.CollectionResourceId,
                Name = dto.Name,
                Description = dto.Description,
                CoverImageUrl = dto.CoverImageUrl,
                IsPublic = dto.IsPublic,
                IsFeatured = dto.IsFeatured,
                SortOrder = dto.SortOrder,
                RecipeCount = dto.RecipeCount,
                CreatedDate = dto.CreatedDate,
                LastModifiedDate = dto.LastModifiedDate,
                Recipes = dto.Recipes?.Select(r => new CollectionRecipeModel {
                    RecipeResourceId = r.RecipeResourceId,
                    Title = r.Title,
                    Description = r.Description,
                    SourceImageUrl = r.SourceImageUrl,
                    SortOrder = r.SortOrder,
                    AddedDate = r.AddedDate,
                    TotalTimeMinutes = r.TotalTimeMinutes,
                    Rating = r.Rating
                }).ToList()
            };
        }

        public UpdateCollectionDto MapToDto(UpdateCollectionModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateCollectionDto {
                Name = model.Name,
                Description = model.Description,
                CoverImageUrl = model.CoverImageUrl,
                IsPublic = model.IsPublic
            };
        }

        public AddRecipeToCollectionDto MapToDto(AddRecipeToCollectionModel model) {
            if (model == null) {
                return null;
            }

            return new AddRecipeToCollectionDto {
                RecipeResourceId = model.RecipeResourceId,
                SortOrder = model.SortOrder
            };
        }

        public ReorderCollectionRecipesDto MapToDto(ReorderCollectionRecipesModel model) {
            if (model == null) {
                return null;
            }

            return new ReorderCollectionRecipesDto {
                Recipes = model.Recipes?.Select(r => new RecipeOrderDto {
                    RecipeResourceId = r.RecipeResourceId,
                    SortOrder = r.SortOrder
                }).ToList()
            };
        }

        public ReorderCollectionsDto MapToDto(ReorderCollectionsModel model) {
            if (model == null) {
                return null;
            }

            return new ReorderCollectionsDto {
                Collections = model.Collections?.Select(c => new CollectionOrderDto {
                    CollectionResourceId = c.CollectionResourceId,
                    SortOrder = c.SortOrder
                }).ToList()
            };
        }

        public CollectionSearchDto MapToDto(CollectionSearchModel model) {
            if (model == null) {
                return null;
            }

            return new CollectionSearchDto {
                IsPublic = model.IsPublic,
                IsFeatured = model.IsFeatured,
                SearchTerm = model.SearchTerm,
                PageNumber = model.PageNumber > 0 ? model.PageNumber : 1,
                PageSize = model.PageSize > 0 ? model.PageSize : 20,
                Sort = model.Sort
            };
        }
    }
}
