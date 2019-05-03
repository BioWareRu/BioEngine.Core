using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Comments
{
    public abstract class BaseCommentsProvider : ICommentsProvider
    {
        protected readonly BioContext DbContext;
        protected readonly ILogger<ICommentsProvider> Logger;

        protected BaseCommentsProvider(BioContext dbContext, ILogger<ICommentsProvider> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        public Task<int> GetCommentsCountAsync(IContentEntity entity)
        {
            return DbContext.Comments.Where(c => c.ContentId == entity.Id && c.Type == entity.GetType().FullName)
                .CountAsync();
        }

        public async Task<Dictionary<Guid, int>> GetCommentsCountAsync(IEnumerable<IContentEntity> entities)
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

        public async Task<IEnumerable<Comment>> GetLastCommentsAsync(Site site, int count)
        {
            var comments = await DbContext.Comments.Where(c => c.SiteIds.Contains(site.Id))
                .OrderByDescending(c => c.DateUpdated).Take(count).ToListAsync();
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
            }

            return comments;
        }

        public async Task<List<(IContentEntity entity, int commentsCount)>> GetMostCommentedAsync(Site site, int count,
            TimeSpan period)
        {
            var ids = await DbContext.Comments
                .Where(c => c.DateUpdated >= DateTimeOffset.Now - period && c.SiteIds.Contains(site.Id))
                .GroupBy(c => new {c.ContentId, c.Type}).OrderByDescending(c => c.Count()).Select(c => c.Key)
                .Take(count)
                .ToListAsync();
            var results = new List<(IContentEntity entity, int commentsCount)>();
            foreach (var id in ids)
            {
                IContentEntity entity = null;
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
