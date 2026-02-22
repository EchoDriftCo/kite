#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class ImportModelMapper {
        public ImportResultModel Map(ImportResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new ImportResultModel {
                TotalRecipes = dto.TotalRecipes,
                SuccessCount = dto.SuccessCount,
                FailureCount = dto.FailureCount,
                ImportedRecipes = dto.ImportedRecipes?.Select(r => new ImportedRecipeModel {
                    RecipeResourceId = r.RecipeResourceId,
                    Title = r.Title
                }).ToList(),
                Errors = dto.Errors?.Select(e => new ImportErrorModel {
                    RecipeName = e.RecipeName,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };
        }
    }
}
