using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class UserEquipmentDto : AuditableEntityDto {
        public int UserEquipmentId { get; set; }
        public Guid SubjectId { get; set; }
        public int EquipmentId { get; set; }
        public EquipmentDto Equipment { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
