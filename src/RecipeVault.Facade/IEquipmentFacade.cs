using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IEquipmentFacade {
        Task<List<EquipmentDto>> GetAllEquipmentAsync();
        Task<List<UserEquipmentDto>> GetUserEquipmentAsync(Guid subjectId);
        Task<UserEquipmentDto> AddUserEquipmentAsync(Guid subjectId, AddUserEquipmentDto dto);
        Task RemoveUserEquipmentAsync(Guid subjectId, string equipmentCode);
        Task<List<RecipeEquipmentDto>> GetRecipeEquipmentAsync(Guid recipeResourceId);
        Task<List<string>> DetectEquipmentFromInstructionsAsync(Guid recipeResourceId);
        Task<EquipmentCheckDto> CheckRecipeEquipmentAsync(Guid subjectId, Guid recipeResourceId);
    }
}
