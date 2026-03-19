using System.Linq;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class EquipmentMapper {
        private readonly SubjectMapper subjectMapper;

        public EquipmentMapper(SubjectMapper subjectMapper) {
            this.subjectMapper = subjectMapper;
        }

        public EquipmentDto MapToDto(Equipment entity) {
            if (entity == null) {
                return null;
            }

            return new EquipmentDto {
                EquipmentId = entity.EquipmentId,
                Name = entity.Name,
                Code = entity.Code,
                Category = entity.Category.ToString(),
                Description = entity.Description,
                IsCommon = entity.IsCommon,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public UserEquipmentDto MapToDto(UserEquipment entity) {
            if (entity == null) {
                return null;
            }

            return new UserEquipmentDto {
                UserEquipmentId = entity.UserEquipmentId,
                SubjectId = entity.SubjectId,
                EquipmentId = entity.EquipmentId,
                Equipment = MapToDto(entity.Equipment),
                AddedDate = entity.AddedDate,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }

        public RecipeEquipmentDto MapToDto(RecipeEquipment entity) {
            if (entity == null) {
                return null;
            }

            return new RecipeEquipmentDto {
                RecipeEquipmentId = entity.RecipeEquipmentId,
                RecipeId = entity.RecipeId,
                EquipmentId = entity.EquipmentId,
                Equipment = MapToDto(entity.Equipment),
                IsRequired = entity.IsRequired,
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate,
                CreatedSubject = subjectMapper.MapToDto(entity.CreatedSubject),
                LastModifiedSubject = subjectMapper.MapToDto(entity.LastModifiedSubject),
            };
        }
    }
}
