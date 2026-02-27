using System;

namespace RecipeVault.Dto.Input {
    public class UpdateCookingLogDto {
        public DateTime CookedDate { get; set; }
        public decimal? ScaleFactor { get; set; }
        public int? ServingsMade { get; set; }
        public string Notes { get; set; }
        public int? Rating { get; set; }
    }
}
