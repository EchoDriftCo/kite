using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class FoodSearchDto {
        public int FdcId { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string BrandOwner { get; set; }
        public List<NutrientDto> Nutrients { get; set; }
    }

    public class NutrientDto {
        public int NutrientId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Unit { get; set; }
        public decimal Value { get; set; }
    }
}
