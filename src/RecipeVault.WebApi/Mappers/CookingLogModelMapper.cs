#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class CookingLogModelMapper {
        private readonly SubjectModelMapper subjectModelMapper;

        public CookingLogModelMapper(SubjectModelMapper subjectModelMapper) {
            this.subjectModelMapper = subjectModelMapper;
        }

        public CookingLogModel Map(CookingLogDto dto) {
            if (dto == null) {
                return null;
            }

            return new CookingLogModel {
                CookingLogResourceId = dto.CookingLogResourceId,
                RecipeResourceId = dto.RecipeResourceId,
                RecipeTitle = dto.RecipeTitle,
                CookedDate = dto.CookedDate,
                ScaleFactor = dto.ScaleFactor,
                ServingsMade = dto.ServingsMade,
                Notes = dto.Notes,
                Rating = dto.Rating,
                Photos = dto.Photos?.Select(p => new CookingLogPhotoModel {
                    ImageUrl = p.ImageUrl,
                    Caption = p.Caption,
                    SortOrder = p.SortOrder
                }).ToList(),
                CreatedDate = dto.CreatedDate,
                CreatedSubject = subjectModelMapper.Map(dto.CreatedSubject),
                LastModifiedDate = dto.LastModifiedDate,
                LastModifiedSubject = subjectModelMapper.Map(dto.LastModifiedSubject)
            };
        }

        public CreateCookingLogDto MapToDto(CreateCookingLogModel model) {
            if (model == null) {
                return null;
            }

            return new CreateCookingLogDto {
                RecipeResourceId = model.RecipeResourceId,
                CookedDate = model.CookedDate,
                ScaleFactor = model.ScaleFactor,
                ServingsMade = model.ServingsMade,
                Notes = model.Notes,
                Rating = model.Rating
            };
        }

        public UpdateCookingLogDto MapToDto(UpdateCookingLogModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateCookingLogDto {
                CookedDate = model.CookedDate,
                ScaleFactor = model.ScaleFactor,
                ServingsMade = model.ServingsMade,
                Notes = model.Notes,
                Rating = model.Rating
            };
        }

        public CookingStatsModel Map(CookingStatsDto dto) {
            if (dto == null) {
                return null;
            }

            return new CookingStatsModel {
                TotalCooks = dto.TotalCooks,
                UniqueRecipes = dto.UniqueRecipes,
                CooksThisYear = dto.CooksThisYear,
                CurrentStreak = dto.CurrentStreak,
                LongestStreak = dto.LongestStreak,
                MostCookedRecipes = dto.MostCookedRecipes?.Select(m => new MostCookedRecipeModel {
                    RecipeResourceId = m.RecipeResourceId,
                    RecipeTitle = m.RecipeTitle,
                    CookCount = m.CookCount,
                    AverageRating = m.AverageRating,
                    LastCookedDate = m.LastCookedDate
                }).ToList()
            };
        }

        public RecipePersonalStatsModel Map(RecipePersonalStatsDto dto) {
            if (dto == null) {
                return null;
            }

            return new RecipePersonalStatsModel {
                CookCount = dto.CookCount,
                AverageRating = dto.AverageRating,
                LastCookedDate = dto.LastCookedDate
            };
        }
    }
}
