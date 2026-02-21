using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class TagFacade : ITagFacade {
        private readonly IUnitOfWork uow;
        private readonly ITagService tagService;
        private readonly TagMapper mapper;
        private readonly ILogger<TagFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public TagFacade(ILogger<TagFacade> logger, IUnitOfWork uow, ITagService tagService, TagMapper mapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.tagService = tagService;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"TagResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<TagDto> CreateTagAsync(UpdateTagDto dto) {
            var tag = await tagService.CreateTagAsync(dto.Name, (TagCategory)dto.Category).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(tag);
        }

        public async Task<TagDto> GetTagAsync(Guid tagResourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var tag = await tagService.GetTagAsync(tagResourceId).ConfigureAwait(false);
                var alias = await tagService.GetUserAliasForTagAsync(tag.TagId).ConfigureAwait(false);
                return mapper.MapToDto(tag, alias);
            }
        }

        public async Task<PagedList<TagDto>> SearchTagsAsync(TagSearchDto search) {
            var tagSearch = mapper.Map(search);
            tagSearch.CreatedSubjectId = CurrentSubjectId;
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var tags = await tagService.SearchTagsAsync(tagSearch).ConfigureAwait(false);
                var aliases = await tagService.GetUserAliasesAsync().ConfigureAwait(false);
                var aliasDict = aliases.ToDictionary(a => a.TagId);
                
                return tags.Convert(x => {
                    aliasDict.TryGetValue(x.TagId, out var alias);
                    return mapper.MapToDto(x, alias);
                });
            }
        }

        public async Task<TagDto> UpdateTagAsync(Guid tagResourceId, UpdateTagDto dto) {
            var lockName = GetLockName(tagResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var tag = await tagService.UpdateTagAsync(tagResourceId, dto.Name, (TagCategory)dto.Category).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(tag);
            }
        }

        public async Task DeleteTagAsync(Guid tagResourceId) {
            var lockName = GetLockName(tagResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await tagService.DeleteTagAsync(tagResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<TagDto> SetAliasAsync(Guid tagResourceId, SetAliasDto dto) {
            var lockName = GetLockName(tagResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var tag = await tagService.GetTagAsync(tagResourceId).ConfigureAwait(false);
                var alias = await tagService.SetAliasAsync(tagResourceId, dto.Alias, dto.ShowAliasPublicly).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(tag, alias);
            }
        }

        public async Task RemoveAliasAsync(Guid tagResourceId) {
            var lockName = GetLockName(tagResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await tagService.RemoveAliasAsync(tagResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<TagDto>> GetUserAliasesAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var aliases = await tagService.GetUserAliasesAsync().ConfigureAwait(false);
                return aliases.Select(a => mapper.MapToDto(a.Tag, a)).ToList();
            }
        }
    }
}
