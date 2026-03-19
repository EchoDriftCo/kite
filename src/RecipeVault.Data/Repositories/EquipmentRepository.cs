using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class EquipmentRepository : IEquipmentRepository {
        private readonly IRecipeVaultDbContext context;

        public EquipmentRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<Equipment>> GetAllAsync() {
            return context.Equipment
                .OrderBy(e => e.Category)
                .ThenBy(e => e.Name)
                .ToListAsync();
        }

        public Task<Equipment> GetByCodeAsync(string code) {
            return context.Equipment
                .FirstOrDefaultAsync(e => e.Code == code);
        }

        public Task<Equipment> GetByIdAsync(int equipmentId) {
            return context.Equipment
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);
        }

        public Task<List<UserEquipment>> GetUserEquipmentAsync(Guid subjectId) {
            return context.UserEquipment
                .Include(ue => ue.Equipment)
                .Where(ue => ue.SubjectId == subjectId)
                .OrderBy(ue => ue.Equipment.Category)
                .ThenBy(ue => ue.Equipment.Name)
                .ToListAsync();
        }

        public async Task<UserEquipment> AddUserEquipmentAsync(UserEquipment userEquipment) {
            var entity = await context.UserEquipment.AddAsync(userEquipment);
            return entity.Entity;
        }

        public Task<UserEquipment> GetUserEquipmentByCodeAsync(Guid subjectId, string code) {
            return context.UserEquipment
                .Include(ue => ue.Equipment)
                .FirstOrDefaultAsync(ue => ue.SubjectId == subjectId && ue.Equipment.Code == code);
        }

        public void RemoveUserEquipment(UserEquipment userEquipment) {
            context.Remove(userEquipment);
        }

        public Task<List<RecipeEquipment>> GetRecipeEquipmentAsync(int recipeId) {
            return context.RecipeEquipment
                .Include(re => re.Equipment)
                .Where(re => re.RecipeId == recipeId)
                .OrderBy(re => re.Equipment.Category)
                .ThenBy(re => re.Equipment.Name)
                .ToListAsync();
        }

        public async Task<RecipeEquipment> AddRecipeEquipmentAsync(RecipeEquipment recipeEquipment) {
            var entity = await context.RecipeEquipment.AddAsync(recipeEquipment);
            return entity.Entity;
        }

        public void RemoveRecipeEquipment(RecipeEquipment recipeEquipment) {
            context.Remove(recipeEquipment);
        }
    }
}
