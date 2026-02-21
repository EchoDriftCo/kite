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

namespace RecipeVault.DomainService {
    public class TagService : ITagService {
        private readonly ILogger<TagService> logger;
        private readonly ITagRepository tagRepository;
        private readonly IUserTagAliasRepository userTagAliasRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public TagService(ITagRepository tagRepository, IUserTagAliasRepository userTagAliasRepository, ILogger<TagService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.tagRepository = tagRepository;
            this.userTagAliasRepository = userTagAliasRepository;
            this.subjectPrincipal = subjectPrincipal;
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

                if (existingAlias != null) {
                    existingAlias.UpdateAlias(aliasName, showAliasPublicly);
                    logger.LogInformation("Updated alias for tag");
                    return existingAlias;
                }

                var newAlias = new UserTagAlias(CurrentSubjectId, tag.TagId, aliasName, showAliasPublicly);
                await userTagAliasRepository.AddAsync(newAlias).ConfigureAwait(false);
                logger.LogInformation("Created alias for tag");
                return newAlias;
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
