using System;

namespace RecipeVault.Dto.Output {
    public class ImportJobDto {
        public Guid ImportJobResourceId { get; set; }
        public string Type { get; set; } // Paprika, UrlBatch, MultiImage, Export
        public string Status { get; set; } // Pending, Processing, Complete, Failed
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ErrorMessage { get; set; }
        public decimal ProgressPercent => TotalItems > 0 ? (decimal)ProcessedItems / TotalItems * 100 : 0;
    }
}
