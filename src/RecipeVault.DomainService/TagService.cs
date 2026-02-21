using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    public class TagService : ITagService {
        private readonly ILogger<TagService> logger;
        private readonly ITagRepository tagRepository;
        private readonly IUserTagAliasRepository userTagAliasRepository;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly IGeminiClient geminiClient;

        public TagService(ITagRepository tagRepository, IUserTagAliasRepository userTagAliasRepository, IGeminiClient geminiClient, ILogger<TagService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.tagRepository = tagRepository;
            this.userTagAliasRepository = userTagAliasRepository;
            this.subjectPrincipal = subjectPrincipal;
            this.geminiClient = geminiClient;
        }
        
        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<Tag> CreateTagAsync(string name, TagCategory category) {
            // If a global tag with the same name+category already exists, return it
            var existing = await tagRepository.GetByNameAndCategoryAsync(name, category).ConfigureAwait(false);
            if (existing != null) {
                return existing;
            }

            var entity = new Tag(name, category, isGlobal: false);

            using (logger.PushProperty("TagResourceId", entity.TagResourceId)) {
                await tagRepository.AddAsync(entity);
                logger.LogInformation("Created new tag");
                return entity;
            }
        }

        public async Task<Tag> GetOrCreateTagAsync(string name, TagCategory category) {
            var existing = await tagRepository.GetByNameAndCategoryAsync(name, category).ConfigureAwait(false);
            if (existing != null) {
                return existing;
            }

            return await CreateTagAsync(name, category).ConfigureAwait(false);
        }

        public async Task<Tag> GetTagAsync(Guid tagResourceId) {
            var entity = await tagRepository.GetAsync(tagResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new TagNotFoundException($"Tag with id {tagResourceId} not found");
            }
            return entity;
        }

        public Task<PagedList<Tag>> SearchTagsAsync(TagSearch search) {
            return tagRepository.SearchAsync(search);
        }

        public async Task<Tag> UpdateTagAsync(Guid tagResourceId, string name, TagCategory category) {
            var entity = await GetOwnTagAsync(tagResourceId).ConfigureAwait(false);

            using (logger.PushProperty("TagResourceId", entity.TagResourceId)) {
                entity.Update(name, category);
                logger.LogInformation("Updated tag");
                return entity;
            }
        }

        public async Task DeleteTagAsync(Guid tagResourceId) {
            var entity = await GetOwnTagAsync(tagResourceId).ConfigureAwait(false);

            using (logger.PushProperty("TagResourceId", entity.TagResourceId)) {
                await tagRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted tag");
            }
        }

        private async Task<Tag> GetOwnTagAsync(Guid tagResourceId) {
            var entity = await tagRepository.GetAsync(tagResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new TagNotFoundException($"Tag with id {tagResourceId} not found");
            }

            if (entity.IsGlobal) {
                throw new TagNotFoundException($"Tag with id {tagResourceId} not found");
            }

            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            if (entity.CreatedSubject?.SubjectId != currentSubjectId) {
                throw new TagNotFoundException($"Tag with id {tagResourceId} not found");
            }

            return entity;
        }

        public async Task<UserTagAlias> SetAliasAsync(Guid tagResourceId, string aliasName, bool showAliasPublicly) {
            var tag = await GetTagAsync(tagResourceId).ConfigureAwait(false);

            using (logger.PushProperty("TagResourceId", tagResourceId)) {
                var existingAlias = await userTagAliasRepository.GetByUserAndTagAsync(CurrentSubjectId, tag.TagId).ConfigureAwait(false);

                UserTagAlias aliasToReturn;
                bool isNewAlias = existingAlias == null;
                bool aliasChanged = existingAlias == null || existingAlias.Alias != aliasName;

                if (existingAlias != null) {
                    existingAlias.UpdateAlias(aliasName, showAliasPublicly);
                    logger.LogInformation("Updated alias for tag");
                    aliasToReturn = existingAlias;
                } else {
                    var newAlias = new UserTagAlias(CurrentSubjectId, tag.TagId, aliasName, showAliasPublicly);
                    await userTagAliasRepository.AddAsync(newAlias).ConfigureAwait(false);
                    logger.LogInformation("Created alias for tag");
                    aliasToReturn = newAlias;
                }

                // Attempt Gemini normalization for Chef/Restaurant/Cookbook if alias changed
                if (aliasChanged && tag.Category == TagCategory.Source && tag.SourceType.HasValue) {
                    var sourceTypeValue = (int)tag.SourceType.Value;
                    // Only normalize for Chef (2), Restaurant (3), Cookbook (4)
                    if (sourceTypeValue >= 2 && sourceTypeValue <= 4) {
                        try {
                            logger.LogInformation("Attempting entity normalization for alias '{Alias}' with SourceType {SourceType}", aliasName, tag.SourceType.Value);
                            var normalizationResult = await geminiClient.NormalizeEntityAsync(aliasName, sourceTypeValue).ConfigureAwait(false);

                            if (normalizationResult?.IsRecognized == true && !string.IsNullOrWhiteSpace(normalizationResult.NormalizedEntityId)) {
                                aliasToReturn.SetNormalizedEntity(normalizationResult.NormalizedEntityId, tag.SourceType.Value);
                                logger.LogInformation("Set normalized entity: {NormalizedEntityId} for alias '{Alias}'", normalizationResult.NormalizedEntityId, aliasName);
                            } else {
                                aliasToReturn.ClearNormalizedEntity();
                                logger.LogInformation("Entity not recognized or confidence too low for alias '{Alias}'", aliasName);
                            }
                        } catch (Exception ex) {
                            logger.LogWarning(ex, "Failed to normalize entity for alias '{Alias}', continuing without normalization", aliasName);
                            // Don't fail the entire operation if normalization fails
                        }
                    }
                }

                return aliasToReturn;
            }
        }

        public async Task RemoveAliasAsync(Guid tagResourceId) {
            var tag = await GetTagAsync(tagResourceId).ConfigureAwait(false);

            using (logger.PushProperty("TagResourceId", tagResourceId)) {
                var existingAlias = await userTagAliasRepository.GetByUserAndTagAsync(CurrentSubjectId, tag.TagId).ConfigureAwait(false);

                if (existingAlias != null) {
                    await userTagAliasRepository.RemoveAsync(existingAlias).ConfigureAwait(false);
                    logger.LogInformation("Removed alias for tag");
                }
            }
        }

        public Task<List<UserTagAlias>> GetUserAliasesAsync() {
            return userTagAliasRepository.GetByUserIdAsync(CurrentSubjectId);
        }

        public Task<UserTagAlias> GetUserAliasForTagAsync(int tagId) {
            return userTagAliasRepository.GetByUserAndTagAsync(CurrentSubjectId, tagId);
        }
    }
}
