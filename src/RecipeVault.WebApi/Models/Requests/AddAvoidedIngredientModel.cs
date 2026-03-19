#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class AddAvoidedIngredientModel {
        [Required]
        [StringLength(100)]
        public string IngredientName { get; set; }

        [StringLength(200)]
        public string Reason { get; set; }
    }
}
