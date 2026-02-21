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

        public TagDto MapToDto(Tag entity, UserTagAlias alias = null) {
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
                Alias = alias?.Alias,
                ShowAliasPublicly = alias?.ShowAliasPublicly ?? false,
                NormalizedEntityId = alias?.NormalizedEntityId,
                NormalizedEntityType = alias?.NormalizedEntityType != null ? (int)alias.NormalizedEntityType.Value : (int?)null,
                NormalizedEntityTypeName = alias?.NormalizedEntityType?.ToString(),
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public RecipeTagDto MapToRecipeTagDto(RecipeTag rt, Guid? viewerSubjectId = null, Guid? ownerSubjectId = null, UserTagAlias alias = null) {
            if (rt == null) {
                return null;
            }

            var globalName = rt.Tag?.Name;
            var isOwner = viewerSubjectId.HasValue && ownerSubjectId.HasValue && viewerSubjectId == ownerSubjectId;
            var displayName = globalName;
            var isOwnerAlias = false;

            // If viewer is owner and has an alias, use it
            if (isOwner && alias != null) {
                displayName = alias.Alias;
                isOwnerAlias = true;
            } else if (!isOwner && alias != null && alias.ShowAliasPublicly) {
                // If viewer is not owner but alias is public, show it
                displayName = alias.Alias;
                isOwnerAlias = true; // Still marks that this is an alias from the owner
            }

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
                IsOwnerAlias = isOwnerAlias,
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
