using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Models;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Publishing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Comments
{
    public class IPBCommentsSynchronizer
    {
        private readonly BioContext _dbContext;
        private readonly SitesRepository _sitesRepository;
        private readonly PropertiesProvider _propertiesProvider;
        private readonly IPBApiClientFactory _ipbApiClientFactory;
        private readonly ILogger<IPBCommentsSynchronizer> _logger;

        public IPBCommentsSynchronizer(BioContext dbContext, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider, IPBApiClientFactory ipbApiClientFactory,
            ILogger<IPBCommentsSynchronizer> logger)
        {
            _dbContext = dbContext;
            _sitesRepository = sitesRepository;
            _propertiesProvider = propertiesProvider;
            _ipbApiClientFactory = ipbApiClientFactory;
            _logger = logger;
        }

        public async Task SyncNewAsync()
        {
            var sites = await _sitesRepository.GetAllAsync();
            var lastCommentId = await _dbContext.Set<IPBComment>().OrderByDescending(c => c.PostId)
                .Select(c => c.PostId).FirstOrDefaultAsync();
            var forumIds = new List<int>();
            foreach (var site in sites.items)
            {
                var settings = await _propertiesProvider.GetAsync<IPBSitePropertiesSet>(site);
                if (settings != null && settings.IsEnabled && settings.ForumId > 0 && !forumIds.Contains(settings.ForumId))
                {
                    forumIds.Add(settings.ForumId);
                }
            }

            if (forumIds.Count == 0)
            {
                return;
            }

            var client = _ipbApiClientFactory.GetReadOnlyClient();

            var posts = new List<Post>();

            try
            {
                var page = 1;

                while (true)
                {
                    var response = await client.GetForumsPostsAsync(forumIds.ToArray(), null, true, page, 1000);
                    posts.AddRange(response.Results);
                    var lastPostId = response.Results.OrderBy(p => p.Id).Select(p => p.Id).First();
                    if (page < response.TotalPages && lastPostId > lastCommentId)
                    {
                        page++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while downloading topics: {errorText}", ex.ToString());
            }

            var topicIds = posts.Select(p => p.ItemId).ToList();
            var records = await _dbContext.Set<IPBPublishRecord>().Where(r => topicIds.Contains(r.TopicId))
                .ToArrayAsync();

            await ProcessPostsAsync(posts.ToArray(), records);

            await _dbContext.SaveChangesAsync();
        }

        public async Task SyncAllAsync()
        {
            var records = await _dbContext.Set<IPBPublishRecord>().ToArrayAsync();
            var client = _ipbApiClientFactory.GetReadOnlyClient();
            var posts = new List<Post>();
            var topicsPosts = new Dictionary<int, List<int>>();
            foreach (var record in records)
            {
                topicsPosts.Add(record.TopicId, new List<int>());
                try
                {
                    var page = 1;

                    while (true)
                    {
                        var response = await client.GetTopicPostsAsync(record.TopicId, page, 1000);
                        foreach (var post in response.Results)
                        {
                            if (post.Id == record.PostId)
                            {
                                continue;
                            }

                            posts.Add(post);
                            topicsPosts[record.TopicId].Add(post.Id);
                        }

                        if (page < response.TotalPages)
                        {
                            page++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while downloading topics: {errorText}", ex.ToString());
                }
            }

            await ProcessPostsAsync(posts.ToArray(), records);

            foreach (var topicPosts in topicsPosts)
            {
                var toDelete = await _dbContext.Set<IPBComment>()
                    .Where(p => !topicPosts.Value.Contains(p.PostId) && p.TopicId == topicPosts.Key).ToListAsync();
                _dbContext.RemoveRange(toDelete);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task ProcessPostsAsync(Post[] posts, IPBPublishRecord[] records)
        {
            var postIds = posts.Select(p => p.Id).ToArray();
            var existingPosts = await _dbContext.Set<IPBComment>().Where(p => postIds.Contains(p.PostId)).ToListAsync();
            foreach (var post in posts)
            {
                var record = records.FirstOrDefault(r => r.TopicId == post.ItemId);
                if (record == null)
                {
                    continue;
                }

                if (record.PostId == post.Id)
                {
                    continue;
                }

                var comment = existingPosts.FirstOrDefault(p => p.PostId == post.Id) ?? new IPBComment
                {
                    ContentType = record.Type,
                    ContentId = record.ContentId,
                    AuthorId = post.Author?.Id ?? "0",
                    PostId = post.Id,
                    TopicId = post.ItemId,
                    DateAdded = post.Date
                };
                comment.DateUpdated = DateTimeOffset.Now;
                comment.SiteIds = record.SiteIds;
                if (comment.Id == Guid.Empty)
                {
                    _dbContext.Add(comment);
                }
                else
                {
                    _dbContext.Update(comment);
                }
            }
        }
    }
}
