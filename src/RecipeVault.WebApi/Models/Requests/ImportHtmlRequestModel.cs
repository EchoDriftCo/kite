using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class ImportHtmlRequestModel {
        [Required]
        public string Html { get; set; }
        public string Source { get; set; }
    }
}
