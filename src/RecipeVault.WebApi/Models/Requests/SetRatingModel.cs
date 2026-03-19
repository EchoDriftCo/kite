#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class SetRatingModel {
        [Range(1, 5)]
        public int? Rating { get; set; }
    }
}
