using Bogus;
using RecipeVault.Domain.Entities;

namespace RecipeVault.TestUtilities.Builders {
    public class RecipeIngredientBuilder {
        private static readonly string[] UnitOptions = { "cup", "tablespoon", "teaspoon", "ounce", "gram", "ml", "liter" };
        private static readonly string[] PreparationOptions = { "diced", "chopped", "minced", "sliced", "", "sifted" };
        private int _sortOrder = 1;
        private decimal? _quantity = 1m;
        private string _unit = "cup";
        private string _item = "flour";
        private string _preparation = "";
        private string _rawText = "1 cup flour";

        public RecipeIngredientBuilder WithSortOrder(int sortOrder) {
            _sortOrder = sortOrder;
            return this;
        }

        public RecipeIngredientBuilder WithQuantity(decimal? quantity) {
            _quantity = quantity;
            return this;
        }

        public RecipeIngredientBuilder WithUnit(string unit) {
            _unit = unit;
            return this;
        }

        public RecipeIngredientBuilder WithItem(string item) {
            _item = item;
            return this;
        }

        public RecipeIngredientBuilder WithPreparation(string preparation) {
            _preparation = preparation;
            return this;
        }

        public RecipeIngredientBuilder WithRawText(string rawText) {
            _rawText = rawText;
            return this;
        }

        public RecipeIngredientBuilder WithRandomValues() {
            var faker = new Faker();
            _sortOrder = faker.Random.Int(1, 100);
            _quantity = faker.Random.Decimal(0.1m, 10m);
            _unit = faker.PickRandom(UnitOptions);
            _item = faker.Commerce.Product();
            _preparation = faker.PickRandom(PreparationOptions);
            _rawText = $"{_quantity} {_unit} {_item}" + (_preparation != "" ? $", {_preparation}" : "");
            return this;
        }

        public RecipeIngredient Build() {
            return new RecipeIngredient(_sortOrder, _quantity, _unit, _item, _preparation, _rawText);
        }
    }
}
