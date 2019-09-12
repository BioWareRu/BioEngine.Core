using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Site;
using BioEngine.Core.Site.Model;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;

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
            var post = await Repository.GetWithBlocksAsync(entities =>
                ApplyPublishConditions(entities).Where(e => e.Url == url));
            if (post == null)
            {
                return PageNotFound();
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
            if (!tags.items.Any())
            {
                return PageNotFound();
            }

            var (items, itemsCount) =
                await Repository.GetAllWithBlocksAsync(entities =>
                    ConfigureQuery(entities, page).WithTags(tags.items).Where(e => e.IsPublished));
            return View("List", new ListViewModel<Post>(GetPageContext(), items,
                itemsCount, Page, ItemsPerPage) {Tags = tags.items});
        }

        protected override void ApplyDefaultOrder(BioQuery<Post> query)
        {
            query.OrderByDescending(p => p.DatePublished);
        }
    }
}
