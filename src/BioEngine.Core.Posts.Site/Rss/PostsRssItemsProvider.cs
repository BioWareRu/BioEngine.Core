using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Routing;
using BioEngine.Core.Site.Rss;
using cloudscribe.Syndication.Models.Rss;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Posts.Site.Rss
{
    public class PostsRssItemsProvider : IRssItemsProvider
    {
        private readonly PostsRepository _postsRepository;
        private readonly LinkGenerator _linkGenerator;

        public PostsRssItemsProvider(PostsRepository postsRepository, LinkGenerator linkGenerator)
        {
            _postsRepository = postsRepository;
            _linkGenerator = linkGenerator;
        }

        public async Task<IEnumerable<RssItem>> GetItemsAsync(Core.Entities.Site site, int count)
        {
            var posts = await _postsRepository.GetAllAsync(entities =>
                entities.Where(e => e.IsPublished).ForSite(site).OrderByDescending(p => p.DatePublished).Take(count));
            DateTimeOffset? mostRecentPubDate = DateTimeOffset.MinValue;
            var items = new List<RssItem>();
            foreach (var post in posts.items)
            {
                if (post.DatePublished != null)
                {
                    var newsDate = post.DatePublished;
                    if (newsDate > mostRecentPubDate) mostRecentPubDate = newsDate;
                    var postUrl = _linkGenerator.GeneratePublicUrl(post, site);
                    var item = new RssItem
                    {
                        Title = post.Title,
                        Description = GetDescription(post),
                        Link = postUrl,
                        PublicationDate = newsDate.Value.Date,
                        Author = post.Author.Name,
                        Guid = new RssGuid(postUrl.ToString(), true)
                    };

                    
                    items.Add(item);
                }
            }

            return items;
        }

        private static string GetDescription(Post post)
        {
            var description = "";

            foreach (var block in post.Blocks)
            {
                switch (block)
                {
                    case CutBlock _:
                        return description;
                    case TextBlock textBlock:
                        description += textBlock.Data.Text;
                        break;
                    case PictureBlock pictureBlock:
                        description +=
                            $"<p style=\"text-align:center;\"><img src=\"{pictureBlock.Data.Picture.PublicUri}\" alt=\"{pictureBlock.Data.Picture.FileName}\" /></p>";
                        break;
                    default:
                        continue;
                }
            }

            return description;
        }
    }
}
