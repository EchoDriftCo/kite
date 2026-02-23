using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Search {
    public class CircleSearchDto : SearchDto {
        public bool? OwnedOnly { get; set; }
    }
}
