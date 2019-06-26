using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Posts.Db
{
    [UsedImplicitly]
    public class PostsRepository : ContentItemRepository<Post>
    {
        private readonly TagsRepository _tagsRepository;

        public PostsRepository(BioRepositoryContext<Post> repositoryContext,
            SectionsRepository sectionsRepository,
            TagsRepository tagsRepository,
            IUserDataProvider? userDataProvider = null) : base(repositoryContext,
            sectionsRepository, userDataProvider)
        {
            _tagsRepository = tagsRepository;
        }

        protected override async Task AfterLoadAsync(Post[] entities)
        {
            await base.AfterLoadAsync(entities);

            var sectionsIds = entities.SelectMany(p => p.SectionIds).Distinct().ToArray();
            var sections = await SectionsRepository.GetByIdsAsync(sectionsIds);

            var tagIds = entities.SelectMany(p => p.TagIds).Distinct().ToArray();
            var tags = await _tagsRepository.GetByIdsAsync(tagIds);

            foreach (var entity in entities)
            {
                entity.Sections = sections.Where(s => entity.SectionIds.Contains(s.Id)).ToList();
                entity.Tags = tags.Where(t => entity.TagIds.Contains(t.Id)).ToList();
            }
        }

        protected override async Task<bool> AfterSaveAsync(Post item, PropertyChange[]? changes = null,
            Post? oldItem = null, IBioRepositoryOperationContext? operationContext = null)
        {
            var version = new ContentVersion {Id = Guid.NewGuid(), ContentId = item.Id};
            version.SetContent(item);
            if (operationContext?.User != null)
            {
                version.ChangeAuthorId = operationContext.User.Id;
            }

            DbContext.Add(version);
            await DbContext.SaveChangesAsync();

            return await base.AfterSaveAsync(item, changes, oldItem, operationContext);
        }

        public async Task<List<ContentVersion>> GetVersionsAsync(Guid itemId)
        {
            return await DbContext.PostVersions.Where(v => v.ContentId == itemId).ToListAsync();
        }

        public async Task<ContentVersion> GetVersionAsync(Guid itemId, Guid versionId)
        {
            return await DbContext.PostVersions.Where(v => v.ContentId == itemId && v.Id == versionId)
                .FirstOrDefaultAsync();
        }
    }
}
