using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class RecipeEquipmentDto : AuditableEntityDto {
        public int RecipeEquipmentId { get; set; }
        public int RecipeId { get; set; }
        public int EquipmentId { get; set; }
        public EquipmentDto Equipment { get; set; }
        public bool IsRequired { get; set; }
    }
}
