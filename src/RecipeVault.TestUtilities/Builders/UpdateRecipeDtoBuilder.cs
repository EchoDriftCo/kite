using System.Collections.Generic;
using Bogus;
using RecipeVault.Dto.Input;

namespace RecipeVault.TestUtilities.Builders {
    public class UpdateRecipeDtoBuilder {
        private string _title = "Test Recipe";
        private string _description = "A test recipe description";
        private int _yield = 4;
        private int? _prepTimeMinutes = 15;
        private int? _cookTimeMinutes = 30;
        private string _source = "Test Source";
        private string _originalImageUrl = "https://example.com/image.jpg";
        private List<UpdateRecipeIngredientDto> _ingredients = new();
        private List<UpdateRecipeInstructionDto> _instructions = new();

        public UpdateRecipeDtoBuilder WithTitle(string title) {
            _title = title;
            return this;
        }

        public UpdateRecipeDtoBuilder WithDescription(string description) {
            _description = description;
            return this;
        }

        public UpdateRecipeDtoBuilder WithYield(int yield) {
            _yield = yield;
            return this;
        }

        public UpdateRecipeDtoBuilder WithPrepTimeMinutes(int? prepTimeMinutes) {
            _prepTimeMinutes = prepTimeMinutes;
            return this;
        }

        public UpdateRecipeDtoBuilder WithCookTimeMinutes(int? cookTimeMinutes) {
            _cookTimeMinutes = cookTimeMinutes;
            return this;
        }

        public UpdateRecipeDtoBuilder WithSource(string source) {
            _source = source;
            return this;
        }

        public UpdateRecipeDtoBuilder WithOriginalImageUrl(string originalImageUrl) {
            _originalImageUrl = originalImageUrl;
            return this;
        }

        public UpdateRecipeDtoBuilder WithIngredients(List<UpdateRecipeIngredientDto> ingredients) {
            _ingredients = ingredients;
            return this;
        }

        public UpdateRecipeDtoBuilder WithInstructions(List<UpdateRecipeInstructionDto> instructions) {
            _instructions = instructions;
            return this;
        }

        public UpdateRecipeDtoBuilder WithRandomValues() {
            var faker = new Faker();
            _title = faker.Commerce.Product();
            _description = faker.Lorem.Paragraph();
            _yield = faker.Random.Int(1, 12);
            _prepTimeMinutes = faker.Random.Int(5, 60);
            _cookTimeMinutes = faker.Random.Int(10, 120);
            _source = faker.Company.CompanyName();
            return this;
        }

        public UpdateRecipeDto Build() {
            return new UpdateRecipeDto {
                Title = _title,
                Description = _description,
                Yield = _yield,
                PrepTimeMinutes = _prepTimeMinutes,
                CookTimeMinutes = _cookTimeMinutes,
                Source = _source,
                OriginalImageUrl = _originalImageUrl,
                Ingredients = _ingredients,
                Instructions = _instructions
            };
        }
    }
}
