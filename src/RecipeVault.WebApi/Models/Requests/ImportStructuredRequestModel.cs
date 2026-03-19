using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class ImportStructuredRequestModel {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public string Source { get; set; }
        public string OriginalImageUrl { get; set; }
        public List<string> RawIngredients { get; set; }
        public List<string> RawInstructions { get; set; }
        public List<string> Categories { get; set; }
    }
}
