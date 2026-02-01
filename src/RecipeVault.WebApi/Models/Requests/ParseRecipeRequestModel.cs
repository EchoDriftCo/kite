#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class ParseRecipeRequestModel {
        [Required]
        public string Image { get; set; }
        [Required]
        public string MimeType { get; set; }
    }
}
