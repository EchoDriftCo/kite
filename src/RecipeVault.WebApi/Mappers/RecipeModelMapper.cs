#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class RecipeModelMapper {
        private readonly SubjectModelMapper subjectModelMapper;

        public RecipeModelMapper(SubjectModelMapper subjectModelMapper) {
            this.subjectModelMapper = subjectModelMapper;
        }

        public RecipeModel Map(RecipeDto dto) {
            if (dto == null) {
                return null;
            }

            return new RecipeModel {
                RecipeResourceId = dto.RecipeResourceId,
                Title = dto.Title,
                Description = dto.Description,
                Yield = dto.Yield,
                PrepTimeMinutes = dto.PrepTimeMinutes,
                CookTimeMinutes = dto.CookTimeMinutes,
                TotalTimeMinutes = dto.TotalTimeMinutes,
                Source = dto.Source,
                OriginalImageUrl = dto.OriginalImageUrl,
                Ingredients = dto.Ingredients?.Select(i => new RecipeIngredientModel {
                    RecipeIngredientId = i.RecipeIngredientId,
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = dto.Instructions?.Select(i => new RecipeInstructionModel {
                    RecipeInstructionId = i.RecipeInstructionId,
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList(),
                CreatedDate = dto.CreatedDate,
                CreatedSubject = subjectModelMapper.Map(dto.CreatedSubject),
                LastModifiedDate = dto.LastModifiedDate,
                LastModifiedSubject = subjectModelMapper.Map(dto.LastModifiedSubject)
            };
        }

        public RecipeSearchDto MapToDto(RecipeSearchModel model) {
            if (model == null) {
                return null;
            }

            return new RecipeSearchDto {
                RecipeResourceId = model.RecipeResourceId,
                Title = model.Title,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Sort = model.Sort
            };
        }

        public UpdateRecipeDto MapToDto(UpdateRecipeModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateRecipeDto {
                Title = model.Title,
                Description = model.Description,
                Yield = model.Yield,
                PrepTimeMinutes = model.PrepTimeMinutes,
                CookTimeMinutes = model.CookTimeMinutes,
                Source = model.Source,
                OriginalImageUrl = model.OriginalImageUrl,
                Ingredients = model.Ingredients?.Select(i => new UpdateRecipeIngredientDto {
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = model.Instructions?.Select(i => new UpdateRecipeInstructionDto {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList()
            };
        }
    }
}
