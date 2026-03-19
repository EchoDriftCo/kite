#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class UpdateDietaryProfileModel {
        [Required]
        [StringLength(100)]
        public string ProfileName { get; set; }

        public bool IsDefault { get; set; }
    }
}
