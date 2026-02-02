using System;
using System.Collections.Generic;
using Bogus;
using RecipeVault.Dto.Output;

namespace RecipeVault.TestUtilities.Builders {
    public class RecipeDtoBuilder {
        private int _recipeId = 1;
        private Guid _recipeResourceId = Guid.NewGuid();
        private string _title = "Test Recipe";
        private string _description = "A test recipe description";
        private int _yield = 4;
        private int? _prepTimeMinutes = 15;
        private int? _cookTimeMinutes = 30;
        private int? _totalTimeMinutes = 45;
        private string _source = "Test Source";
        private string _originalImageUrl = "https://example.com/image.jpg";
        private List<RecipeIngredientDto> _ingredients = new();
        private List<RecipeInstructionDto> _instructions = new();

        public RecipeDtoBuilder WithRecipeId(int recipeId) {
            _recipeId = recipeId;
            return this;
        }

        public RecipeDtoBuilder WithRecipeResourceId(Guid recipeResourceId) {
            _recipeResourceId = recipeResourceId;
            return this;
        }

        public RecipeDtoBuilder WithTitle(string title) {
            _title = title;
            return this;
        }

        public RecipeDtoBuilder WithDescription(string description) {
            _description = description;
            return this;
        }

        public RecipeDtoBuilder WithYield(int yield) {
            _yield = yield;
            return this;
        }

        public RecipeDtoBuilder WithPrepTimeMinutes(int? prepTimeMinutes) {
            _prepTimeMinutes = prepTimeMinutes;
            return this;
        }

        public RecipeDtoBuilder WithCookTimeMinutes(int? cookTimeMinutes) {
            _cookTimeMinutes = cookTimeMinutes;
            return this;
        }

        public RecipeDtoBuilder WithTotalTimeMinutes(int? totalTimeMinutes) {
            _totalTimeMinutes = totalTimeMinutes;
            return this;
        }

        public RecipeDtoBuilder WithSource(string source) {
            _source = source;
            return this;
        }

        public RecipeDtoBuilder WithOriginalImageUrl(string originalImageUrl) {
            _originalImageUrl = originalImageUrl;
            return this;
        }

        public RecipeDtoBuilder WithIngredients(List<RecipeIngredientDto> ingredients) {
            _ingredients = ingredients;
            return this;
        }

        public RecipeDtoBuilder WithInstructions(List<RecipeInstructionDto> instructions) {
            _instructions = instructions;
            return this;
        }

        public RecipeDtoBuilder WithRandomValues() {
            var faker = new Faker();
            _title = faker.Commerce.Product();
            _description = faker.Lorem.Paragraph();
            _yield = faker.Random.Int(1, 12);
            _prepTimeMinutes = faker.Random.Int(5, 60);
            _cookTimeMinutes = faker.Random.Int(10, 120);
            _totalTimeMinutes = _prepTimeMinutes + _cookTimeMinutes;
            _source = faker.Company.CompanyName();
            return this;
        }

        public RecipeDto Build() {
            return new RecipeDto {
                RecipeId = _recipeId,
                RecipeResourceId = _recipeResourceId,
                Title = _title,
                Description = _description,
                Yield = _yield,
                PrepTimeMinutes = _prepTimeMinutes,
                CookTimeMinutes = _cookTimeMinutes,
                TotalTimeMinutes = _totalTimeMinutes,
                Source = _source,
                OriginalImageUrl = _originalImageUrl,
                Ingredients = _ingredients,
                Instructions = _instructions
            };
        }
    }
}
