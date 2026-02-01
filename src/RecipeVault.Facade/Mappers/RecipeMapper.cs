using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class RecipeMapper {
        private readonly SubjectMapper subjectMapper;

        public RecipeMapper(SubjectMapper subjectMapper) {
            this.subjectMapper = subjectMapper;
        }

        public RecipeDto MapToDto(Recipe entity) {
            if (entity == null) {
                return null;
            }

            return new RecipeDto {
                RecipeId = entity.RecipeId,
                RecipeResourceId = entity.RecipeResourceId,
                Title = entity.Title,
                Description = entity.Description,
                Yield = entity.Yield,
                PrepTimeMinutes = entity.PrepTimeMinutes,
                CookTimeMinutes = entity.CookTimeMinutes,
                TotalTimeMinutes = entity.TotalTimeMinutes,
                Source = entity.Source,
                OriginalImageUrl = entity.OriginalImageUrl,
                Ingredients = entity.Ingredients?.Select(i => new RecipeIngredientDto {
                    RecipeIngredientId = i.RecipeIngredientId,
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = entity.Instructions?.Select(i => new RecipeInstructionDto {
                    RecipeInstructionId = i.RecipeInstructionId,
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList(),
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public RecipeSearch Map(RecipeSearchDto dto) {
            if (dto == null) {
                return null;
            }

            return new RecipeSearch {
                RecipeResourceId = dto.RecipeResourceId,
                Title = dto.Title,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Sort = dto.Sort
            };
        }
    }
}
