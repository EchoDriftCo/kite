using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Facade.Mappers {
    public class SubjectMapper {
        public SubjectDto MapToDto(Subject entity) {
            if (entity == null) {
                return null;
            }

            return new SubjectDto {
                SubjectId = entity.SubjectId
            };
        }
    }
}
