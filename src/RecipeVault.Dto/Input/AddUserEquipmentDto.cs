using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class AddUserEquipmentDto {
        [Required]
        [StringLength(100)]
        public string EquipmentCode { get; set; }
    }
}
