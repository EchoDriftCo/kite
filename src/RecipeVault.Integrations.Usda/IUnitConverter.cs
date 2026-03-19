using System.Threading.Tasks;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public interface IUnitConverter {
        Task<decimal?> ConvertToGramsAsync(decimal quantity, string unit, string ingredientName, int? fdcId = null);
    }
}
