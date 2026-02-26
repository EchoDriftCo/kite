using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class EquipmentCheckDto {
        public bool HasAllRequired { get; set; }
        public List<EquipmentDto> RequiredEquipment { get; set; }
        public List<EquipmentDto> MissingEquipment { get; set; }
        public List<EquipmentDto> OptionalEquipment { get; set; }
    }
}
