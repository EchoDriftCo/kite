#pragma warning disable CS1591 // Missing XML comments

using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class CreateCookingLogModel {
        [Required]
        public Guid RecipeResourceId { get; set; }
        [Required]
        public DateTime CookedDate { get; set; }
        public decimal? ScaleFactor { get; set; }
        public int? ServingsMade { get; set; }
        public string Notes { get; set; }
        [Range(1, 5)]
        public int? Rating { get; set; }
    }
}
