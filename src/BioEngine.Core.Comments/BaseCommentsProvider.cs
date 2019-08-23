using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Comments
{
    public abstract class BaseCommentsProvider : ICommentsProvider
    {
        protected readonly BioContext DbContext;
        private readonly IUserDataProvider _userDataProvider;
        protected readonly ILogger<BaseCommentsProvider> Logger;

        protected BaseCommentsProvider(BioContext dbContext,
            IUserDataProvider userDataProvider,
            ILogger<BaseCommentsProvider> logger)
        {
            DbContext = dbContext;
            _userDataProvider = userDataProvider;
            Logger = logger;
        }

        public Task<int> GetCommentsCountAsync(ContentItem entity)
        {
            return GetDbSet().Where(c => c.ContentId == entity.Id).CountAsync();
        }

        protected abstract IQueryable<BaseComment> GetDbSet();

        public virtual async Task<Dictionary<Guid, (int count, Uri? uri)>> GetCommentsDataAsync(
            ContentItem[] entities)
        {
            var result = new Dictionary<Guid, (int count, Uri? uri)>();
            var ids = entities.Select(e => e.Id).ToArray();
            var counts = await GetDbSet().Where(c => ids.Contains(c.ContentId))
                .GroupBy(c => c.ContentId).Select(g => new {g.Key, count = g.Count()})
                .ToListAsync();
            var urls = await GetCommentsUrlAsync(entities);
            foreach (var entity in entities)
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

            return result;
        }

        public abstract Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(ContentItem[] entities);

        public virtual async Task<IEnumerable<BaseComment>> GetLastCommentsAsync(Site site, int count)
        {
            var comments = await GetDbSet().Where(c => c.SiteIds.Contains(site.Id))
                .OrderByDescending(c => c.DateUpdated).Take(count).ToListAsync();
            var authors = await _userDataProvider.GetDataAsync(comments.Select(c => c.AuthorId).ToArray());
            var contentIds = comments.Select(c => c.ContentId).Distinct().ToList();
            var contentItems = await DbContext.ContentItems.Where(c => contentIds.Contains(c.Id)).ToListAsync();
            foreach (var comment in comments)
            {
                comment.Author = authors.FirstOrDefault(a => a.Id == comment.AuthorId);
                comment.ContentItem = contentItems.FirstOrDefault(a => a.Id == comment.ContentId);
            }

            return comments;
        }

        public virtual async Task<List<(ContentItem entity, int commentsCount)>> GetMostCommentedAsync(Site site,
            int count,
            TimeSpan period)
        {
            var ids = await GetDbSet()
                .Where(c => c.DateUpdated >= DateTimeOffset.Now - period && c.SiteIds.Contains(site.Id))
                .GroupBy(c => c.ContentId).OrderByDescending(c => c.Count()).Select(c => c.Key)
                .Take(count)
                .ToListAsync();
            var results = new List<(ContentItem entity, int commentsCount)>();
            var contentItems = await DbContext.ContentItems.Where(c => ids.Contains(c.Id)).ToListAsync();
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

        public abstract Task<BaseComment> AddCommentAsync(ContentItem entity, string text, string authorId,
            Guid? replyTo = null);

        public abstract Task<BaseComment> UpdateCommentAsync(ContentItem entity, Guid commentId, string text);

        public async Task<BaseComment> DeleteCommentAsync(ContentItem entity, Guid commentId)
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

        public async Task<IEnumerable<BaseComment>> GetCommentsAsync(ContentItem entity)
        {
            var comments = await GetDbSet().Where(c => c.ContentId == entity.Id).OrderBy(c => c.DateAdded).ToListAsync();
            var authors = await _userDataProvider.GetDataAsync(comments.Select(c => c.AuthorId).ToArray());
            foreach (var comment in comments)
            {
                comment.Author = authors.FirstOrDefault(a => a.Id == comment.AuthorId);
            }

            return comments;
        }

        public async Task<BaseComment> GetCommentByIdAsync(ContentItem entity, Guid commentId)
        {
            return await GetDbSet().Where(c => c.ContentId == entity.Id && c.Id == commentId).FirstOrDefaultAsync();
        }
    }
}
