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

            return new RecipeTagDto {
                TagResourceId = rt.Tag?.TagResourceId ?? default,
                Name = rt.Tag?.Name,
                Category = rt.Tag != null ? (int)rt.Tag.Category : 0,
                CategoryName = rt.Tag?.Category.ToString(),
                IsAiAssigned = rt.IsAiAssigned,
                Confidence = rt.Confidence,
                IsOverridden = rt.IsOverridden,
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
