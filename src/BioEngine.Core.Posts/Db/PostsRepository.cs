using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Posts.Db
{
    [UsedImplicitly]
    public class PostsRepository<TUserPk> : ContentItemRepository<Post<TUserPk>>
    {
        private readonly TagsRepository _tagsRepository;
        private readonly IUserDataProvider<TUserPk> _userDataProvider;

        public PostsRepository(BioRepositoryContext<Post<TUserPk>> repositoryContext,
            SectionsRepository sectionsRepository,
            TagsRepository tagsRepository,
            IUserDataProvider<TUserPk>? userDataProvider = null) : base(repositoryContext,
            sectionsRepository)
        {
            _tagsRepository = tagsRepository;
            _userDataProvider = userDataProvider;
        }

        protected override async Task AfterLoadAsync(Post<TUserPk>[] entities)
        {
            await base.AfterLoadAsync(entities);

            var sectionsIds = entities.SelectMany(p => p.SectionIds).Distinct().ToArray();
            var sections = await SectionsRepository.GetByIdsAsync(sectionsIds);

            var tagIds = entities.SelectMany(p => p.TagIds).Distinct().ToArray();
            var tags = await _tagsRepository.GetByIdsAsync(tagIds);

            var userIds = entities.Select(e => e.AuthorId).Distinct().ToArray();
            var users = await _userDataProvider.GetDataAsync(userIds);

            foreach (var entity in entities)
            {
                entity.Sections = sections.Where(s => entity.SectionIds.Contains(s.Id)).ToList();
                entity.Tags = tags.Where(t => entity.TagIds.Contains(t.Id)).ToList();
                entity.Author = users.FirstOrDefault(d => d.Id.Equals(entity.AuthorId));
            }
        }

        protected override async Task<bool> AfterSaveAsync(Post<TUserPk> item, PropertyChange[]? changes = null,
            Post<TUserPk>? oldItem = null, IBioRepositoryOperationContext? operationContext = null)
        {
            var version = new PostVersion<TUserPk> {Id = Guid.NewGuid(), ContentId = item.Id};
            version.SetContent(item);
            if (operationContext is IUserBioRepositoryOperationContext<TUserPk> userBioRepositoryOperationContext)
            {
                version.ChangeAuthorId = userBioRepositoryOperationContext.User.Id;
            }

            DbContext.Add(version);
            await DbContext.SaveChangesAsync();

            return await base.AfterSaveAsync(item, changes, oldItem, operationContext);
        }

        public async Task<List<PostVersion<TUserPk>>> GetVersionsAsync(Guid itemId)
        {
            return await DbContext.Set<PostVersion<TUserPk>>().Where(v => v.ContentId == itemId).ToListAsync();
        }

        public async Task<PostVersion<TUserPk>> GetVersionAsync(Guid itemId, Guid versionId)
        {
            return await DbContext.Set<PostVersion<TUserPk>>().Where(v => v.ContentId == itemId && v.Id == versionId)
                .FirstOrDefaultAsync();
        }
    }
}
