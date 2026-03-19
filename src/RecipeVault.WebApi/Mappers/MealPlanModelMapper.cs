#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class MealPlanModelMapper {
        private readonly SubjectModelMapper subjectModelMapper;

        public MealPlanModelMapper(SubjectModelMapper subjectModelMapper) {
            this.subjectModelMapper = subjectModelMapper;
        }

        public MealPlanModel Map(MealPlanDto dto) {
            if (dto == null) {
                return null;
            }

            return new MealPlanModel {
                MealPlanResourceId = dto.MealPlanResourceId,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Entries = dto.Entries?.Select(e => new MealPlanEntryModel {
                    MealPlanEntryId = e.MealPlanEntryId,
                    Date = e.Date,
                    MealSlot = e.MealSlot,
                    RecipeResourceId = e.RecipeResourceId,
                    RecipeTitle = e.RecipeTitle,
                    RecipeYield = e.RecipeYield,
                    Servings = e.Servings,
                    IsLeftover = e.IsLeftover
                }).ToList(),
                CreatedDate = dto.CreatedDate,
                CreatedSubject = subjectModelMapper.Map(dto.CreatedSubject),
                LastModifiedDate = dto.LastModifiedDate,
                LastModifiedSubject = subjectModelMapper.Map(dto.LastModifiedSubject)
            };
        }

        public MealPlanSearchDto MapToDto(MealPlanSearchModel model) {
            if (model == null) {
                return null;
            }

            return new MealPlanSearchDto {
                StartDateFrom = model.StartDateFrom,
                StartDateTo = model.StartDateTo,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Sort = model.Sort
            };
        }

        public UpdateMealPlanDto MapToDto(UpdateMealPlanModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateMealPlanDto {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Entries = model.Entries?.Select(e => new UpdateMealPlanEntryDto {
                    Date = e.Date,
                    MealSlot = e.MealSlot,
                    RecipeResourceId = e.RecipeResourceId,
                    Servings = e.Servings,
                    IsLeftover = e.IsLeftover
                }).ToList()
            };
        }

        public GroceryListModel Map(GroceryListDto dto) {
            if (dto == null) {
                return null;
            }

            return new GroceryListModel {
                Items = dto.Items?.Select(i => new GroceryItemModel {
                    Item = i.Item,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Category = i.Category,
                    Sources = i.Sources
                }).ToList()
            };
        }
    }
}
