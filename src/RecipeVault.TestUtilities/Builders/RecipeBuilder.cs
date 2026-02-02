using System;
using System.Collections.Generic;
using Bogus;
using RecipeVault.Domain.Entities;

namespace RecipeVault.TestUtilities.Builders {
    public class RecipeBuilder {
        private string _title = "Test Recipe";
        private int _yield = 4;
        private int? _prepTimeMinutes = 15;
        private int? _cookTimeMinutes = 30;
        private string _description = "A test recipe description";
        private string _source = "Test Source";
        private string _originalImageUrl = "https://example.com/image.jpg";
        private List<RecipeIngredient> _ingredients = new();
        private List<RecipeInstruction> _instructions = new();

        public RecipeBuilder WithTitle(string title) {
            _title = title;
            return this;
        }

        public RecipeBuilder WithYield(int yield) {
            _yield = yield;
            return this;
        }

        public RecipeBuilder WithPrepTimeMinutes(int? prepTimeMinutes) {
            _prepTimeMinutes = prepTimeMinutes;
            return this;
        }

        public RecipeBuilder WithCookTimeMinutes(int? cookTimeMinutes) {
            _cookTimeMinutes = cookTimeMinutes;
            return this;
        }

        public RecipeBuilder WithDescription(string description) {
            _description = description;
            return this;
        }

        public RecipeBuilder WithSource(string source) {
            _source = source;
            return this;
        }

        public RecipeBuilder WithOriginalImageUrl(string originalImageUrl) {
            _originalImageUrl = originalImageUrl;
            return this;
        }

        public RecipeBuilder WithIngredients(List<RecipeIngredient> ingredients) {
            _ingredients = ingredients;
            return this;
        }

        public RecipeBuilder WithInstructions(List<RecipeInstruction> instructions) {
            _instructions = instructions;
            return this;
        }

        public RecipeBuilder WithRandomValues() {
            var faker = new Faker();
            _title = faker.Commerce.Product();
            _yield = faker.Random.Int(1, 12);
            _prepTimeMinutes = faker.Random.Int(5, 60);
            _cookTimeMinutes = faker.Random.Int(10, 120);
            _description = faker.Lorem.Paragraph();
            _source = faker.Company.CompanyName();
            return this;
        }

        public Recipe Build() {
            var recipe = new Recipe(_title, _yield, _prepTimeMinutes, _cookTimeMinutes, _description, _source, _originalImageUrl);
            if (_ingredients.Count > 0) {
                recipe.SetIngredients(_ingredients);
            }
            if (_instructions.Count > 0) {
                recipe.SetInstructions(_instructions);
            }
            return recipe;
        }
    }
}
