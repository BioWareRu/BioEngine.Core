using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Comments
{
    public abstract class BaseCommentsProvider<TUserPk> : ICommentsProvider<TUserPk>
    {
        protected readonly BioContext DbContext;
        private readonly IUserDataProvider<TUserPk> _userDataProvider;
        protected readonly ILogger<BaseCommentsProvider<TUserPk>> Logger;

        protected BaseCommentsProvider(BioContext dbContext,
            IUserDataProvider<TUserPk> userDataProvider,
            ILogger<BaseCommentsProvider<TUserPk>> logger)
        {
            DbContext = dbContext;
            _userDataProvider = userDataProvider;
            Logger = logger;
        }

        public Task<int> GetCommentsCountAsync(IContentItem entity)
        {
            return GetDbSet().Where(c => c.ContentId == entity.Id && c.ContentType == entity.GetKey())
                .CountAsync();
        }

        protected abstract IQueryable<BaseComment<TUserPk>> GetDbSet();

        public virtual async Task<Dictionary<Guid, (int count, Uri? uri)>> GetCommentsDataAsync(
            IContentItem[] entities, Site site)
        {
            var result = new Dictionary<Guid, (int count, Uri? uri)>();
            var groups = entities.GroupBy(e => e.GetKey()).ToList();
            foreach (var group in groups)
            {
                var ids = group.Select(e => e.Id).ToArray();
                var counts = await GetDbSet().Where(c => ids.Contains(c.ContentId) && c.ContentType == group.Key)
                    .GroupBy(c => c.ContentId).Select(g => new {g.Key, count = g.Count()})
                    .ToListAsync();
                var urls = await GetCommentsUrlAsync(entities, site);
                foreach (var entity in group)
                {
                    var entityData = counts.FirstOrDefault(c => c.Key == entity.Id);

                    var count = 0;
                    Uri? uri = null;
                    if (entityData != null)
                    {
                        count = entityData.count;
                    }

                    if (urls.ContainsKey(entity.Id))
                    {
                        uri = urls[entity.Id];
                    }

                    result.Add(entity.Id, (count, uri));
                }
            }


            return result;
        }

        public abstract Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(IContentItem[] entities, Site site);

        public virtual async Task<IEnumerable<BaseComment<TUserPk>>> GetLastCommentsAsync<TContent>(Site site,
            int count) where TContent : class, IContentItem
        {
            var comments = await GetDbSet().Where(c =>
                    c.SiteIds.Contains(site.Id) && c.ContentType == EntityExtensions.GetKey<TContent>())
                .OrderByDescending(c => c.DateUpdated).Take(count).ToListAsync();
            var authors = await _userDataProvider.GetDataAsync(comments.Select(c => c.AuthorId).ToArray());
            var groups = comments.GroupBy(c => c.ContentType);
            foreach (var group in groups)
            {
                var contentIds = comments.Select(c => c.ContentId).Distinct().ToList();
                var contentItems = await DbContext.Set<TContent>().Where(c => contentIds.Contains(c.Id)).ToListAsync();
                foreach (var comment in group)
                {
                    comment.Author = authors.FirstOrDefault(a => a.Id.Equals(comment.AuthorId));
                    comment.ContentItem = contentItems.FirstOrDefault(a => a.Id == comment.ContentId);
                }
            }


            return comments;
        }

        public virtual async Task<List<(TContent entity, int commentsCount)>> GetMostCommentedAsync<TContent>(Site site,
            int count,
            TimeSpan period) where TContent : class, IContentItem
        {
            var ids = await GetDbSet()
                .Where(c => c.DateUpdated >= DateTimeOffset.Now - period && c.SiteIds.Contains(site.Id) &&
                            c.ContentType == EntityExtensions.GetKey<TContent>())
                .GroupBy(c => c.ContentId).OrderByDescending(c => c.Count()).Select(c => c.Key)
                .Take(count)
                .ToListAsync();
            var results = new List<(TContent entity, int commentsCount)>();
            var contentItems = await DbContext.Set<TContent>().Where(c => ids.Contains(c.Id)).ToListAsync();
            foreach (var id in ids)
            {
                var entity = contentItems.FirstOrDefault(c => c.Id == id);
                if (entity != null)
                {
                    var commentsCount = await GetCommentsCountAsync(entity);
                    results.Add((entity, commentsCount));
                }
            }

            return results;
        }

        public abstract Task<BaseComment<TUserPk>> AddCommentAsync(IContentItem entity, string text, TUserPk authorId,
            Guid? replyTo = null);

        public abstract Task<BaseComment<TUserPk>> UpdateCommentAsync(IContentItem entity, Guid commentId, string text);

        public async Task<BaseComment<TUserPk>> DeleteCommentAsync(IContentItem entity, Guid commentId)
        {
            var comment = await GetCommentByIdAsync(entity, commentId);
            if (comment == null)
            {
                throw new Exception($"Comment {commentId.ToString()} not found");
            }

            DbContext.Remove(comment);
            await DbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<IEnumerable<BaseComment<TUserPk>>> GetCommentsAsync(IContentItem entity, Site site)
        {
            var comments = await GetDbSet().Where(c => c.ContentId == entity.Id && c.SiteIds.Contains(site.Id))
                .OrderBy(c => c.DateAdded).ToListAsync();
            var authors = await _userDataProvider.GetDataAsync(comments.Select(c => c.AuthorId).ToArray());
            foreach (var comment in comments)
            {
                comment.Author = authors.FirstOrDefault(a => a.Id.Equals(comment.AuthorId));
            }

            return comments;
        }

        public async Task<BaseComment<TUserPk>> GetCommentByIdAsync(IContentItem entity, Guid commentId)
        {
            return await GetDbSet().Where(c => c.ContentId == entity.Id && c.Id == commentId).FirstOrDefaultAsync();
        }
    }
}
