using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Comments;
using BioEngine.Core.Entities;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Routing;
using BioEngine.Core.Site;
using BioEngine.Core.Site.Model;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;
using WilderMinds.RssSyndication;

namespace BioEngine.Core.Posts.Site
{
    public abstract class BasePostsController : SiteController<Post, PostsRepository>
    {
        protected readonly TagsRepository TagsRepository;
        private readonly ICommentsProvider _commentsProvider;

        protected BasePostsController(
            BaseControllerContext<Post, PostsRepository> context,
            TagsRepository tagsRepository,
            ICommentsProvider commentsProvider) : base(context)
        {
            TagsRepository = tagsRepository;
            _commentsProvider = commentsProvider;
        }

        public override async Task<IActionResult> ShowAsync(string url)
        {
            var post = await Repository.GetAsync(entities => entities.Where(e => e.Url == url && e.IsPublished));
            if (post == null)
            {
                return NotFound();
            }

            var commentsData = await _commentsProvider.GetCommentsDataAsync(new ContentItem[] {post});

            return View(new ContentItemViewModel(GetPageContext(), post, commentsData[post.Id].count,
                commentsData[post.Id].uri, ContentEntityViewMode.Entity));
        }

        public virtual Task<IActionResult> ListByTagPageAsync(string tagNames, int page)
        {
            return ShowListByTagAsync(tagNames, page);
        }

        public virtual Task<IActionResult> ListByTagAsync(string tagNames)
        {
            return ShowListByTagAsync(tagNames, 0);
        }

        protected virtual async Task<IActionResult> ShowListByTagAsync(string tagNames, int page)
        {
            if (string.IsNullOrEmpty(tagNames))
            {
                return BadRequest();
            }

            var titles = tagNames.Split("+").Select(t => t.ToLowerInvariant()).ToArray();

            var tags = await TagsRepository.GetAllAsync(q => q.Where(t => titles.Contains(t.Title.ToLower())));
            if (!tags.Any())
            {
                return NotFound();
            }

            var context = GetQueryContext(page);
            context.SetTags(tags);

            var (items, itemsCount) =
                await Repository.GetAllAsync(context, entities => entities.Where(e => e.IsPublished));
            return View("List", new ListViewModel<Post>(GetPageContext(), items,
                itemsCount, Page, ItemsPerPage) {Tags = tags});
        }

        public virtual async Task<IActionResult> RssAsync()
        {
            var feed = new Feed
            {
                Title = Site.Title,
                Description = "Последние публикации",
                Link = new Uri(Site.Url),
                Copyright = $"(c) {Site.Title}"
            };

            var context = GetQueryContext();

            var posts = await Repository.GetAllAsync(context,entities => entities.Where(e => e.IsPublished));
            var mostRecentPubDate = DateTime.MinValue;
            var commentsData =
                await _commentsProvider.GetCommentsDataAsync(posts.items.Select(p => p as ContentItem).ToArray());
            foreach (var post in posts.items)
            {
                var postDate = post.DateAdded.DateTime;
                if (postDate > mostRecentPubDate) mostRecentPubDate = postDate;
                var postUrl = LinkGenerator.GeneratePublicUrl(post, Site);


                var item = new Item
                {
                    Title = post.Title,
                    Body = GetDescription(post),
                    Link = postUrl,
                    PublishDate = postDate,
                    Author = new Author {Name = post.Author.Name},
                };

                if (commentsData.ContainsKey(post.Id))
                {
                    item.Comments = commentsData[post.Id].uri;
                }

                foreach (var section in post.Sections)
                {
                    item.Categories.Add(section.Title);
                }

                feed.Items.Add(item);
            }


            var rss = feed.Serialize();

            return Content(rss, "text/xml; charset=utf-8");
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
                        description += $"<p style=\"text-align:center;\">{pictureBlock.Data.Picture.PublicUri}</p>";
                        break;
                    default:
                        continue;
                }
            }

            return description;
        }
    }
}
