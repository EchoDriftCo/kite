namespace RecipeVault.Dto.Search {
    public class CollectionSearchDto {
        public bool? IsPublic { get; set; }
        public bool? IsFeatured { get; set; }
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Sort { get; set; }
    }
}
