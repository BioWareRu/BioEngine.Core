using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Social;
using BioEngine.Core.Routing;
using BioEngine.Extra.Twitter.Service;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Twitter
{
    public class TwitterContentPublisher : BaseContentPublisher<TwitterPublishConfig, TwitterPublishRecord>
    {
        private readonly TwitterService _twitterService;
        private readonly LinkGenerator _linkGenerator;

        public TwitterContentPublisher(TwitterService twitterService, BioContext dbContext,
            ILogger<TwitterContentPublisher> logger, LinkGenerator linkGenerator) :
            base(dbContext, logger)
        {
            _twitterService = twitterService;
            _linkGenerator = linkGenerator;
        }

        protected override Task<TwitterPublishRecord> DoPublishAsync(TwitterPublishRecord record, IContentItem entity,
            Site site,
            TwitterPublishConfig config)
        {
            var text = ConstructText(entity, site, config.Tags);

            var tweetId = _twitterService.CreateTweet(text, config.Config);
            if (tweetId == 0)
            {
                throw new Exception($"Can't create tweet for item {entity.Title} ({entity.Id.ToString()})");
            }

            record.TweetId = tweetId;

            return Task.FromResult(record);
        }

        private string ConstructText(IContentItem content, Site site, IEnumerable<string> configTags)
        {
            var url = _linkGenerator.GeneratePublicUrl(content, site);
            var text = $"{content.Title} {url}";

            var tags = string.Join(" ", configTags
                .Select(s => $"#{s.Replace("#", "")}")
            );
            if (!string.IsNullOrEmpty(tags))
            {
                text = $"{text} {tags}";
            }

            return text;
        }

        protected override Task<bool> DoDeleteAsync(TwitterPublishRecord record, TwitterPublishConfig config)
        {
            var deleted = _twitterService.DeleteTweet(record.TweetId, config.Config);
            if (!deleted)
            {
                throw new Exception("Can't delete news tweet");
            }

            return Task.FromResult(true);
        }
    }
}
