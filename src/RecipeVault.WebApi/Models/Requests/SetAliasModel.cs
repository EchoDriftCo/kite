#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class SetAliasModel {
        [Required]
        [StringLength(100)]
        public string Alias { get; set; }
        
        public bool ShowAliasPublicly { get; set; }
    }
}
