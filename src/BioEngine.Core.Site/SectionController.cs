using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
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
                await Repository.GetWithBlocksAsync(async entities =>
                    (await ApplyShowConditionsAsync(entities)).Where(e => e.Url == url));
            if (entity == null)
            {
                return PageNotFound();
            }

            return View(new EntityViewModel<TSection>(GetPageContext(), entity, ContentEntityViewMode.Entity));
        }

        protected virtual async Task<IActionResult> ShowContentAsync<TContent>(string url, int page = 0)
            where TContent : class, IContentItem
        {
            var section =
                await Repository.GetWithBlocksAsync(async entities =>
                    (await ApplyShowConditionsAsync(entities)).Where(e => e.Url == url));
            if (section == null)
            {
                return PageNotFound();
            }

            if (HttpContext.RequestServices.GetRequiredService<IBioRepository<TContent>>() is
                ContentEntityRepository<TContent> contentRepository)
            {
                var (items, itemsCount) = await contentRepository.GetAllWithBlocksAsync(queryable =>
                    queryable.ForSite(Site).ForSection(section).Take(ItemsPerPage)
                        .Where(c => c.IsPublished).Paginate(page, ItemsPerPage));
                return View("Content", new SectionContentListViewModel<TSection, TContent>(GetPageContext(section),
                    section,
                    items,
                    itemsCount, page, ItemsPerPage));
            }

            throw new Exception("Can't inject ContentEntityRepository");
        }

        protected virtual PageViewModelContext GetPageContext(TSection section)
        {
            var context = new PageViewModelContext(LinkGenerator, PropertiesProvider, Site, Version, section);

            return context;
        }
    }
}
