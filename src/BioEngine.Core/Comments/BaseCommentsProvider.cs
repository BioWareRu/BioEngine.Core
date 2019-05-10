using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Comments
{
    public abstract class BaseCommentsProvider : ICommentsProvider
    {
        protected readonly BioContext DbContext;
        private readonly IUserDataProvider _userDataProvider;
        protected readonly ILogger<ICommentsProvider> Logger;

        protected BaseCommentsProvider(BioContext dbContext,
            IUserDataProvider userDataProvider,
            ILogger<ICommentsProvider> logger)
        {
            DbContext = dbContext;
            _userDataProvider = userDataProvider;
            Logger = logger;
        }

        public Task<int> GetCommentsCountAsync(IContentEntity entity)
        {
            return GetDbSet().Where(c => c.ContentId == entity.Id && c.Type == entity.GetType().FullName)
                .CountAsync();
        }

        protected abstract IQueryable<BaseComment> GetDbSet();

        public virtual async Task<Dictionary<Guid, int>> GetCommentsCountAsync(IEnumerable<IContentEntity> entities)
        {
            var result = new Dictionary<Guid, int>();
            foreach (var entity in entities)
            {
                var count = await GetCommentsCountAsync(entity);
                result.Add(entity.Id, count);
            }

            return result;
        }

        public abstract Task<Uri> GetCommentsUrlAsync(IContentEntity entity);
        public virtual async Task<IEnumerable<BaseComment>> GetLastCommentsAsync(Site site, int count)
        {
            var comments = await GetDbSet().Where(c => c.SiteIds.Contains(site.Id))
                .OrderByDescending(c => c.DateUpdated).Take(count).ToListAsync();
            var authors = await _userDataProvider.GetDataAsync(comments.Select(c => c.AuthorId).ToArray());
            foreach (var comment in comments)
            {
                if (comment.Type == typeof(Post).FullName)
                {
                    comment.Content =
                        await DbContext.Posts.Where(p => p.Id == comment.ContentId).FirstOrDefaultAsync();
                }

                if (comment.Type == typeof(Page).FullName)
                {
                    comment.Content =
                        await DbContext.Pages.Where(p => p.Id == comment.ContentId).FirstOrDefaultAsync();
                }

                if (comment.Type == typeof(Section).FullName)
                {
                    comment.Content = await DbContext.Sections.Where(s => s.Id == comment.ContentId)
                        .FirstOrDefaultAsync();
                }

                comment.Author = authors.FirstOrDefault(a => a.Id == comment.AuthorId);
            }

            return comments;
        }

        public virtual async Task<List<(IContentEntity entity, int commentsCount)>> GetMostCommentedAsync(Site site,
            int count,
            TimeSpan period)
        {
            var ids = await GetDbSet()
                .Where(c => c.DateUpdated >= DateTimeOffset.Now - period && c.SiteIds.Contains(site.Id))
                .GroupBy(c => new {c.ContentId, c.Type}).OrderByDescending(c => c.Count()).Select(c => c.Key)
                .Take(count)
                .ToListAsync();
            var results = new List<(IContentEntity entity, int commentsCount)>();
            foreach (var id in ids)
            {
                IContentEntity? entity = null;
                if (id.Type == typeof(Post).FullName)
                {
                    entity =
                        await DbContext.Posts.Where(p => p.Id == id.ContentId).FirstOrDefaultAsync();
                }

                if (id.Type == typeof(Page).FullName)
                {
                    entity =
                        await DbContext.Pages.Where(p => p.Id == id.ContentId).FirstOrDefaultAsync();
                }

                if (id.Type == typeof(Section).FullName)
                {
                    entity = await DbContext.Sections.Where(s => s.Id == id.ContentId)
                        .FirstOrDefaultAsync();
                }

                if (entity != null)
                {
                    var commentsCount = await GetCommentsCountAsync(entity);
                    results.Add((entity, commentsCount));
                }
            }

            return results;
        }
    }
}
