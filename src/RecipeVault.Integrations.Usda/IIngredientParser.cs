using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public interface IIngredientParser {
        ParsedIngredient Parse(string ingredientText);
    }
}
