using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.DomainService {
    public interface IEquipmentService {
        Task<List<Equipment>> GetAllEquipmentAsync();
        Task<List<UserEquipment>> GetUserEquipmentAsync(Guid subjectId);
        Task<UserEquipment> AddUserEquipmentAsync(Guid subjectId, string equipmentCode);
        Task RemoveUserEquipmentAsync(Guid subjectId, string equipmentCode);
        Task<List<RecipeEquipment>> GetRecipeEquipmentAsync(int recipeId);
        Task<List<string>> DetectEquipmentFromInstructionsAsync(int recipeId);
        Task SetRecipeEquipmentAsync(int recipeId, List<string> equipmentCodes);
        Task<bool> UserHasRequiredEquipmentAsync(Guid subjectId, int recipeId);
        Task<List<RecipeEquipment>> GetMissingEquipmentAsync(Guid subjectId, int recipeId);
    }
}
