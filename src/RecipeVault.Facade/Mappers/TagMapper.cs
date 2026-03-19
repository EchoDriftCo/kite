using System;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class TagMapper {
        private readonly SubjectMapper subjectMapper;

        public TagMapper(SubjectMapper subjectMapper) {
            this.subjectMapper = subjectMapper;
        }

        public TagDto MapToDto(Tag entity) {
            if (entity == null) {
                return null;
            }

            return new TagDto {
                TagId = entity.TagId,
                TagResourceId = entity.TagResourceId,
                Name = entity.Name,
                Category = (int)entity.Category,
                CategoryName = entity.Category.ToString(),
                IsGlobal = entity.IsGlobal,
                SourceType = entity.SourceType.HasValue ? (int)entity.SourceType.Value : (int?)null,
                SourceTypeName = entity.SourceType?.ToString(),
                IsSystemTag = entity.IsSystemTag,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public RecipeTagDto MapToRecipeTagDto(RecipeTag rt) {
            if (rt == null) {
                return null;
            }

            var globalName = rt.Tag?.Name;
            // Display name is Detail if present, otherwise Tag.Name
            var displayName = !string.IsNullOrWhiteSpace(rt.Detail) ? rt.Detail : globalName;

            return new RecipeTagDto {
                TagResourceId = rt.Tag?.TagResourceId ?? default,
                GlobalName = globalName,
                DisplayName = displayName,
                Category = rt.Tag != null ? (int)rt.Tag.Category : 0,
                CategoryName = rt.Tag?.Category.ToString(),
                SourceType = rt.Tag?.SourceType != null ? (int)rt.Tag.SourceType.Value : (int?)null,
                SourceTypeName = rt.Tag?.SourceType?.ToString(),
                IsAiAssigned = rt.IsAiAssigned,
                Confidence = rt.Confidence,
                IsOverridden = rt.IsOverridden,
                Detail = rt.Detail,
                NormalizedEntityId = rt.NormalizedEntityId,
                NormalizedEntityType = rt.NormalizedEntityType != null ? (int)rt.NormalizedEntityType.Value : (int?)null,
            };
        }

        public TagSearch Map(TagSearchDto dto) {
            if (dto == null) {
                return null;
            }

            return new TagSearch {
                Name = dto.Name,
                Category = dto.Category,
                IsGlobal = dto.IsGlobal,
                CreatedSubjectId = dto.CreatedSubjectId,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Sort = dto.Sort
            };
        }
    }
}
