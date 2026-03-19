using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class EquipmentService : IEquipmentService {
        private readonly ILogger<EquipmentService> logger;
        private readonly IEquipmentRepository equipmentRepository;
        private readonly IRecipeRepository recipeRepository;

        private static readonly Dictionary<string, string[]> EquipmentPatterns = new() {
            ["instant-pot"] = new[] { "instant pot", "pressure cooker", "pressure cook" },
            ["air-fryer"] = new[] { "air fryer", "air fry", "air-fry" },
            ["slow-cooker"] = new[] { "slow cooker", "crock pot", "crockpot" },
            ["stand-mixer"] = new[] { "stand mixer", "kitchenaid", "fitted with paddle" },
            ["food-processor"] = new[] { "food processor", "pulse until" },
            ["blender"] = new[] { "blender", "blend until", "puree" },
            ["immersion-blender"] = new[] { "immersion blender", "stick blender" },
            ["dutch-oven"] = new[] { "dutch oven" },
            ["cast-iron"] = new[] { "cast iron", "cast-iron skillet" },
            ["wok"] = new[] { "wok" },
            ["grill"] = new[] { "grill", "grilled", "barbecue", "bbq" },
            ["pizza-stone"] = new[] { "pizza stone", "baking stone" },
            ["steamer-basket"] = new[] { "steamer", "steam basket", "steaming" },
            ["double-boiler"] = new[] { "double boiler", "bain marie" },
            ["bundt-pan"] = new[] { "bundt pan", "bundt" },
            ["springform-pan"] = new[] { "springform", "springform pan" },
            ["tart-pan"] = new[] { "tart pan" },
            ["ramekins"] = new[] { "ramekin", "ramekins" },
            ["mandoline"] = new[] { "mandoline", "mandolin" },
            ["kitchen-scale"] = new[] { "kitchen scale", "digital scale", "weigh" },
            ["meat-thermometer"] = new[] { "meat thermometer", "instant-read thermometer" },
            ["piping-bags"] = new[] { "piping bag", "pastry bag" },
            ["torch"] = new[] { "kitchen torch", "culinary torch", "torch", "brûlée" },
            ["mortar-pestle"] = new[] { "mortar and pestle", "mortar & pestle" }
        };

        public EquipmentService(IEquipmentRepository equipmentRepository, IRecipeRepository recipeRepository, ILogger<EquipmentService> logger) {
            this.logger = logger;
            this.equipmentRepository = equipmentRepository;
            this.recipeRepository = recipeRepository;
        }

        public Task<List<Equipment>> GetAllEquipmentAsync() {
            return equipmentRepository.GetAllAsync();
        }

        public Task<List<UserEquipment>> GetUserEquipmentAsync(Guid subjectId) {
            return equipmentRepository.GetUserEquipmentAsync(subjectId);
        }

        public async Task<UserEquipment> AddUserEquipmentAsync(Guid subjectId, string equipmentCode) {
            var equipment = await equipmentRepository.GetByCodeAsync(equipmentCode).ConfigureAwait(false);
            if (equipment == null) {
                throw new EquipmentNotFoundException($"Equipment with code '{equipmentCode}' not found");
            }

            var existing = await equipmentRepository.GetUserEquipmentByCodeAsync(subjectId, equipmentCode).ConfigureAwait(false);
            if (existing != null) {
                logger.LogInformation("User already has equipment {EquipmentCode}", equipmentCode);
                return existing;
            }

            var userEquipment = new UserEquipment(subjectId, equipment.EquipmentId);
            await equipmentRepository.AddUserEquipmentAsync(userEquipment).ConfigureAwait(false);
            logger.LogInformation("Added equipment {EquipmentCode} to user {SubjectId}", equipmentCode, subjectId);
            return userEquipment;
        }

        public async Task RemoveUserEquipmentAsync(Guid subjectId, string equipmentCode) {
            var userEquipment = await equipmentRepository.GetUserEquipmentByCodeAsync(subjectId, equipmentCode).ConfigureAwait(false);
            if (userEquipment == null) {
                throw new EquipmentNotFoundException($"User equipment with code '{equipmentCode}' not found");
            }

            equipmentRepository.RemoveUserEquipment(userEquipment);
            logger.LogInformation("Removed equipment {EquipmentCode} from user {SubjectId}", equipmentCode, subjectId);
        }

        public Task<List<RecipeEquipment>> GetRecipeEquipmentAsync(int recipeId) {
            return equipmentRepository.GetRecipeEquipmentAsync(recipeId);
        }

        public async Task<List<string>> DetectEquipmentFromInstructionsAsync(int recipeId) {
            var recipe = await recipeRepository.GetByIdAsync(recipeId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeId} not found");
            }

            var instructionsText = string.Join(" ", recipe.Instructions.Select(i => i.Instruction)).ToLower(System.Globalization.CultureInfo.InvariantCulture);
            var detectedCodes = EquipmentPatterns
                .Where(kv => kv.Value.Any(pattern => instructionsText.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                .Select(kv => kv.Key)
                .ToList();

            logger.LogInformation("Detected {Count} equipment items for recipe {RecipeId}", detectedCodes.Count, recipeId);
            return detectedCodes;
        }

        public async Task SetRecipeEquipmentAsync(int recipeId, List<string> equipmentCodes) {
            var recipe = await recipeRepository.GetByIdAsync(recipeId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeId} not found");
            }

            // Remove existing equipment
            var existing = await equipmentRepository.GetRecipeEquipmentAsync(recipeId).ConfigureAwait(false);
            foreach (var re in existing) {
                equipmentRepository.RemoveRecipeEquipment(re);
            }

            // Add new equipment
            foreach (var code in equipmentCodes) {
                var equipment = await equipmentRepository.GetByCodeAsync(code).ConfigureAwait(false);
                if (equipment != null) {
                    var recipeEquipment = new RecipeEquipment(recipeId, equipment.EquipmentId, true);
                    await equipmentRepository.AddRecipeEquipmentAsync(recipeEquipment).ConfigureAwait(false);
                }
            }

            logger.LogInformation("Set {Count} equipment items for recipe {RecipeId}", equipmentCodes.Count, recipeId);
        }

        public async Task<bool> UserHasRequiredEquipmentAsync(Guid subjectId, int recipeId) {
            var recipeEquipment = await equipmentRepository.GetRecipeEquipmentAsync(recipeId).ConfigureAwait(false);
            var userEquipment = await equipmentRepository.GetUserEquipmentAsync(subjectId).ConfigureAwait(false);
            var userEquipmentIds = userEquipment.Select(ue => ue.EquipmentId).ToHashSet();

            var requiredEquipment = recipeEquipment.Where(re => re.IsRequired);
            return requiredEquipment.All(re => userEquipmentIds.Contains(re.EquipmentId));
        }

        public async Task<List<RecipeEquipment>> GetMissingEquipmentAsync(Guid subjectId, int recipeId) {
            var recipeEquipment = await equipmentRepository.GetRecipeEquipmentAsync(recipeId).ConfigureAwait(false);
            var userEquipment = await equipmentRepository.GetUserEquipmentAsync(subjectId).ConfigureAwait(false);
            var userEquipmentIds = userEquipment.Select(ue => ue.EquipmentId).ToHashSet();

            return recipeEquipment.Where(re => !userEquipmentIds.Contains(re.EquipmentId)).ToList();
        }
    }
}
