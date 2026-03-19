using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeVault.Exceptions;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public class UserPantryService : IUserPantryService {
        private readonly IUserPantryRepository repository;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly ILogger<UserPantryService> logger;

        private static readonly List<string> DefaultPantryStaples = new() {
            "salt", "black pepper", "water", "olive oil", "vegetable oil",
            "butter", "all-purpose flour", "granulated sugar", "garlic", "onion"
        };

        public UserPantryService(
            IUserPantryRepository repository,
            ISubjectPrincipal subjectPrincipal,
            ILogger<UserPantryService> logger) {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.subjectPrincipal = subjectPrincipal ?? throw new ArgumentNullException(nameof(subjectPrincipal));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public Task<List<UserPantryItem>> GetUserPantryAsync() {
            return repository.GetBySubjectIdAsync(CurrentSubjectId);
        }

        public async Task<UserPantryItem> AddPantryItemAsync(CreatePantryItemDto dto) {
            var subjectId = CurrentSubjectId;

            var item = new UserPantryItem(
                subjectId,
                dto.IngredientName.Trim(),
                dto.IsStaple,
                dto.ExpirationDate
            );

            await repository.AddAsync(item).ConfigureAwait(false);
            logger.LogInformation("Added pantry item {IngredientName} for user {SubjectId}", dto.IngredientName, subjectId);
            return item;
        }

        public async Task<UserPantryItem> UpdatePantryItemAsync(int id, UpdatePantryItemDto dto) {
            var subjectId = CurrentSubjectId;
            var item = await repository.GetByIdAsync(id).ConfigureAwait(false);

            if (item == null || item.SubjectId != subjectId) {
                throw new PantryItemNotFoundException("Pantry item not found");
            }

            item.Update(dto.IngredientName.Trim(), dto.IsStaple, dto.ExpirationDate);
            logger.LogInformation("Updated pantry item {Id} for user {SubjectId}", id, subjectId);
            return item;
        }

        public async Task DeletePantryItemAsync(int id) {
            var subjectId = CurrentSubjectId;
            var item = await repository.GetByIdAsync(id).ConfigureAwait(false);

            if (item == null || item.SubjectId != subjectId) {
                throw new PantryItemNotFoundException("Pantry item not found");
            }

            repository.Remove(item);
            logger.LogInformation("Deleted pantry item {Id} for user {SubjectId}", id, subjectId);
        }

        public Task<List<string>> GetDefaultStaplesAsync() {
            return Task.FromResult(new List<string>(DefaultPantryStaples));
        }

        public async Task EnsureUserPantrySeededAsync() {
            var subjectId = CurrentSubjectId;
            var existingCount = await repository.CountAsync(subjectId).ConfigureAwait(false);

            if (existingCount == 0) {
                var stapleItems = DefaultPantryStaples.Select(name => new UserPantryItem(
                    subjectId,
                    name,
                    isStaple: true
                )).ToList();

                await repository.AddRangeAsync(stapleItems).ConfigureAwait(false);
                logger.LogInformation("Seeded {Count} default pantry staples for user {SubjectId}", stapleItems.Count, subjectId);
            }
        }
    }
}
