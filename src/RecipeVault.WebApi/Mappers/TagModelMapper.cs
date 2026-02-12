#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class TagModelMapper {
        private readonly SubjectModelMapper subjectModelMapper;

        public TagModelMapper(SubjectModelMapper subjectModelMapper) {
            this.subjectModelMapper = subjectModelMapper;
        }

        public TagModel Map(TagDto dto) {
            if (dto == null) {
                return null;
            }

            return new TagModel {
                TagId = dto.TagId,
                TagResourceId = dto.TagResourceId,
                Name = dto.Name,
                Category = dto.Category,
                CategoryName = dto.CategoryName,
                IsGlobal = dto.IsGlobal,
                CreatedDate = dto.CreatedDate,
                CreatedSubject = subjectModelMapper.Map(dto.CreatedSubject),
                LastModifiedDate = dto.LastModifiedDate,
                LastModifiedSubject = subjectModelMapper.Map(dto.LastModifiedSubject)
            };
        }

        public TagSearchDto MapToDto(TagSearchModel model) {
            if (model == null) {
                return null;
            }

            return new TagSearchDto {
                Name = model.Name,
                Category = model.Category,
                IsGlobal = model.IsGlobal,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Sort = model.Sort
            };
        }

        public UpdateTagDto MapToDto(UpdateTagModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateTagDto {
                Name = model.Name,
                Category = model.Category
            };
        }

        public AssignTagDto MapToDto(AssignTagItemModel model) {
            if (model == null) {
                return null;
            }

            return new AssignTagDto {
                TagResourceId = model.TagResourceId,
                Name = model.Name,
                Category = model.Category
            };
        }
    }
}
