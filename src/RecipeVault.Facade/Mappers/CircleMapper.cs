using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using System;

namespace RecipeVault.Facade.Mappers {
    public class CircleMapper {
        private readonly SubjectMapper subjectMapper;
        private readonly RecipeMapper recipeMapper;

        public CircleMapper(SubjectMapper subjectMapper, RecipeMapper recipeMapper) {
            this.subjectMapper = subjectMapper;
            this.recipeMapper = recipeMapper;
        }

        public CircleDto MapToDto(Circle entity) {
            return MapToDto(entity, null);
        }

        public CircleDto MapToDto(Circle entity, Guid? currentSubjectId) {
            if (entity == null) {
                return null;
            }

            var ownerSubjectId = entity.OwnerSubjectId;

            return new CircleDto {
                CircleResourceId = entity.CircleResourceId,
                Name = entity.Name,
                Description = entity.Description,
                OwnerSubjectId = entity.OwnerSubjectId,
                CreatedDate = entity.CreatedDate,
                Members = entity.Members?.Select(m => new CircleMemberDto {
                    SubjectId = m.SubjectId,
                    Role = m.Role.ToString(),
                    Status = m.Status.ToString(),
                    JoinedDate = m.JoinedDate
                }).ToList(),
                SharedRecipes = entity.SharedRecipes?.Select(sr => new CircleRecipeDto {
                    RecipeResourceId = sr.Recipe?.RecipeResourceId ?? default,
                    Title = sr.Recipe?.Title,
                    SharedBySubjectId = sr.SharedBySubjectId,
                    SharedDate = sr.SharedDate
                }).ToList(),
                MemberCount = entity.Members?.Count ?? 0,
                RecipeCount = entity.SharedRecipes?.Count ?? 0,
                IsOwner = currentSubjectId.HasValue && ownerSubjectId == currentSubjectId
            };
        }

        public CircleInviteDto MapToDto(CircleInvite entity) {
            if (entity == null) {
                return null;
            }

            return new CircleInviteDto {
                InviteToken = entity.InviteToken,
                CircleName = entity.Circle?.Name,
                InviteeEmail = entity.InviteeEmail,
                ExpiresDate = entity.ExpiresDate,
                Status = entity.Status.ToString()
            };
        }

        public CircleSearch Map(CircleSearchDto dto) {
            if (dto == null) {
                return null;
            }

            return new CircleSearch {
                OwnedOnly = dto.OwnedOnly,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Sort = dto.Sort
            };
        }
    }
}
