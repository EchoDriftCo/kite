using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.TestUtilities.Builders {
    public class TagBuilder {
        private string _name = "Test Tag";
        private TagCategory _category = TagCategory.Custom;
        private bool _isGlobal;

        public TagBuilder WithName(string name) {
            _name = name;
            return this;
        }

        public TagBuilder WithCategory(TagCategory category) {
            _category = category;
            return this;
        }

        public TagBuilder WithIsGlobal(bool isGlobal) {
            _isGlobal = isGlobal;
            return this;
        }

        public Tag Build() {
            return new Tag(_name, _category, _isGlobal);
        }
    }
}
