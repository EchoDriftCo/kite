#pragma warning disable CS1591 // Missing XML comments

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class AddCookingLogPhotosModel {
        [Required]
        public List<CookingLogPhotoInputModel> Photos { get; set; }
    }

    public class CookingLogPhotoInputModel {
        [Required]
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int? SortOrder { get; set; }
    }
}
