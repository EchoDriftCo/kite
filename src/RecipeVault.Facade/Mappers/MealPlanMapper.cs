using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class MealPlanMapper {
        private readonly SubjectMapper subjectMapper;

        public MealPlanMapper(SubjectMapper subjectMapper) {
            this.subjectMapper = subjectMapper;
        }

        public MealPlanDto MapToDto(MealPlan entity) {
            if (entity == null) {
                return null;
            }

            return new MealPlanDto {
                MealPlanId = entity.MealPlanId,
                MealPlanResourceId = entity.MealPlanResourceId,
                Name = entity.Name,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Entries = entity.Entries?.Select(e => new MealPlanEntryDto {
                    MealPlanEntryId = e.MealPlanEntryId,
                    Date = e.Date,
                    MealSlot = (int)e.MealSlot,
                    RecipeResourceId = e.Recipe?.RecipeResourceId ?? default,
                    RecipeTitle = e.Recipe?.Title,
                    RecipeYield = e.Recipe?.Yield ?? 0,
                    Servings = e.Servings,
                    IsLeftover = e.IsLeftover
                }).ToList(),
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public MealPlanSearch Map(MealPlanSearchDto dto) {
            if (dto == null) {
                return null;
            }

            return new MealPlanSearch {
                StartDateFrom = dto.StartDateFrom,
                StartDateTo = dto.StartDateTo,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Sort = dto.Sort
            };
        }
    }
}
