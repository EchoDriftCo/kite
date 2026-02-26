using System;
using System.Linq;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class CookingLogMapper {
        private readonly SubjectMapper subjectMapper;

        public CookingLogMapper(SubjectMapper subjectMapper) {
            this.subjectMapper = subjectMapper;
        }

        public CookingLogDto MapToDto(CookingLog entity) {
            if (entity == null) {
                return null;
            }

            return new CookingLogDto {
                CookingLogResourceId = entity.CookingLogResourceId,
                RecipeResourceId = entity.Recipe?.RecipeResourceId ?? Guid.Empty,
                RecipeTitle = entity.Recipe?.Title,
                CookedDate = entity.CookedDate,
                ScaleFactor = entity.ScaleFactor,
                ServingsMade = entity.ServingsMade,
                Notes = entity.Notes,
                Rating = entity.Rating,
                Photos = entity.Photos?.Select(p => new CookingLogPhotoDto {
                    ImageUrl = p.ImageUrl,
                    Caption = p.Caption,
                    SortOrder = p.SortOrder
                }).ToList(),
                CreatedDate = entity.CreatedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedDate = entity.LastModifiedDate,
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject)
            };
        }
    }
}
