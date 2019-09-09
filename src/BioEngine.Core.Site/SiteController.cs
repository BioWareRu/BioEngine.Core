using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using BioEngine.Core.Site.Model;
using BioEngine.Core.Web;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Site
{
    public abstract class BaseSiteController : BaseController
    {
        protected BaseSiteController(BaseControllerContext context) : base(context)
        {
        }

        protected Entities.Site Site
        {
            get
            {
                var siteFeature = HttpContext.Features.Get<CurrentSiteFeature>();
                if (siteFeature == null)
                {
                    throw new ArgumentException("CurrentSiteFeature is empty");
                }

                return siteFeature.Site;
            }
        }

        protected virtual PageViewModelContext GetPageContext()
        {
            var context = new PageViewModelContext(LinkGenerator, PropertiesProvider, Site);

            return context;
        }

        protected virtual IActionResult PageNotFound()
        {
            return NotFound();
        }
    }

    public abstract class SiteController<TEntity, TRepository> : BaseSiteController
        where TEntity : class, IEntity, IRoutable, IContentEntity
        where TRepository : ContentEntityRepository<TEntity>
    {
        protected SiteController(
            BaseControllerContext<TEntity, TRepository> context)
            : base(context)
        {
            Repository = context.Repository;
        }

        protected int Page { get; private set; } = 1;
        protected virtual int ItemsPerPage { get; } = 20;

        [PublicAPI] protected TRepository Repository;


        public virtual async Task<IActionResult> ListAsync()
        {
            var (items, itemsCount) = await GetAllAsync(ApplyPublishConditions);
            return View("List", new ListViewModel<TEntity>(GetPageContext(), items,
                itemsCount, Page, ItemsPerPage));
        }

        public virtual async Task<IActionResult> ListPageAsync(int page)
        {
            var (items, itemsCount) = await GetAllAsync(ApplyPublishConditions, page);
            return View("List", new ListViewModel<TEntity>(GetPageContext(), items,
                itemsCount, Page, ItemsPerPage));
        }


        public virtual async Task<IActionResult> ShowAsync(string url)
        {
            var entity =
                await Repository.GetAsync(entities => ApplyPublishConditions(entities).Where(e => e.Url == url));
            if (entity == null)
            {
                return PageNotFound();
            }

            return View(new EntityViewModel<TEntity>(GetPageContext(), entity, ContentEntityViewMode.Entity));
        }

        protected virtual BioQuery<TEntity> ApplyPublishConditions(BioQuery<TEntity> query)
        {
            return query.Where(e => e.IsPublished);
        }

        protected virtual void ApplyDefaultOrder(BioQuery<TEntity> provider)
        {
            provider.OrderByDescending(e => e.DateAdded);
        }

        protected virtual BioQuery<TEntity> CreateListQuery()
        {
            return new BioQuery<TEntity>(Repository.GetBaseQuery()).ForSite(Site);
        }

        [PublicAPI]
        protected virtual Task<(TEntity[] items, int itemsCount)> GetAllAsync(
            Func<BioQuery<TEntity>, BioQuery<TEntity>>? configureQuery, int page = 0)
        {
            return Repository.GetAllWithBlocksAsync(bioQuery => ConfigureQuery(bioQuery, page));
        }

        protected BioQuery<TEntity> ConfigureQuery(BioQuery<TEntity> query, int page)
        {
            if (ControllerContext.HttpContext.Request.Query.ContainsKey("order"))
            {
                query.OrderByString(ControllerContext.HttpContext.Request.Query["order"]);
            }
            else
            {
                ApplyDefaultOrder(query);
            }

            if (page > 0)
            {
                Page = page;
            }
            else if (ControllerContext.HttpContext.Request.Query.ContainsKey("page"))
            {
                Page = int.Parse(ControllerContext.HttpContext.Request.Query["page"]);
                if (Page < 1) Page = 1;
            }

            query.Paginate(page, ItemsPerPage);
            return query;
        }
    }
}
