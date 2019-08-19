using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Site.Model;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Site
{
    public abstract class SectionController<TSection, TRepository> : SiteController<TSection, TRepository>
        where TSection : Section, IEntity where TRepository : ContentEntityRepository<TSection>
    {
        protected SectionController(
            BaseControllerContext<TSection, TRepository> context) :
            base(context)
        {
        }

        public override async Task<IActionResult> ShowAsync(string url)
        {
            var entity =
                await Repository.GetWithBlocksAsync(entities =>
                    ApplyPublishConditions(entities).Where(e => e.Url == url));
            if (entity == null)
            {
                return PageNotFound();
            }

            return View(new EntityViewModel<TSection>(GetPageContext(), entity, ContentEntityViewMode.Entity));
        }

        protected virtual async Task<IActionResult> ShowContentAsync<TContent>(string url, int page = 0)
            where TContent : ContentItem
        {
            var section =
                await Repository.GetWithBlocksAsync(entities => entities.Where(e => e.Url == url && e.IsPublished));
            if (section == null)
            {
                return PageNotFound();
            }

            var contentRepository =
                HttpContext.RequestServices.GetRequiredService<IBioRepository<TContent>>() as
                    ContentEntityRepository<TContent>;
            var provider = new ListProvider<TContent, ContentEntityRepository<TContent>>(contentRepository);
            provider.SetPage(page).SetPageSize(ItemsPerPage).SetSite(Site);
            var (items, itemsCount) = await provider.GetAllAsync(queryable =>
                queryable.ForSection(section)
                    .Where(c => c.IsPublished));
            return View("Content", new SectionContentListViewModel<TSection, TContent>(GetPageContext(section), section,
                items,
                itemsCount, page, ItemsPerPage));
        }

        protected virtual PageViewModelContext GetPageContext(TSection section)
        {
            var context = new PageViewModelContext(LinkGenerator, PropertiesProvider, Site, section);

            return context;
        }
    }
}
