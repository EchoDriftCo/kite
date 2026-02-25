#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class AddDietaryRestrictionModel {
        [Required]
        [StringLength(50)]
        public string RestrictionCode { get; set; }

        [Required]
        [StringLength(50)]
        public string RestrictionType { get; set; }

        [Required]
        [StringLength(20)]
        public string Severity { get; set; }
    }
}
