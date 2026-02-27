using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IEquipmentRepository {
        Task<List<Equipment>> GetAllAsync();
        Task<Equipment> GetByCodeAsync(string code);
        Task<Equipment> GetByIdAsync(int equipmentId);
        Task<List<UserEquipment>> GetUserEquipmentAsync(Guid subjectId);
        Task<UserEquipment> AddUserEquipmentAsync(UserEquipment userEquipment);
        Task<UserEquipment> GetUserEquipmentByCodeAsync(Guid subjectId, string code);
        void RemoveUserEquipment(UserEquipment userEquipment);
        Task<List<RecipeEquipment>> GetRecipeEquipmentAsync(int recipeId);
        Task<RecipeEquipment> AddRecipeEquipmentAsync(RecipeEquipment recipeEquipment);
        void RemoveRecipeEquipment(RecipeEquipment recipeEquipment);
    }
}
