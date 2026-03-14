namespace RecipeVault.Dto.Input {
    public class CreateApiTokenDto {
        public string Name { get; set; }
        public int? ExpiresInDays { get; set; }
    }
}
