using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Seo;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Site.Model
{
    public abstract class PageViewModel
    {
        public readonly Entities.Site Site;
        public readonly Section Section;
        protected readonly PropertiesProvider PropertiesProvider;
        protected readonly LinkGenerator LinkGenerator;

        protected PageViewModel(PageViewModelContext context)
        {
            Site = context.Site;
            Section = context.Section;
            PropertiesProvider = context.PropertiesProvider;
            LinkGenerator = context.LinkGenerator;
        }


        private PageMetaModel _meta;

        public Task<TPropertySet> GetSitePropertiesAsync<TPropertySet>() where TPropertySet : PropertiesSet, new()
        {
            return PropertiesProvider.GetAsync<TPropertySet>(Site);
        }

        public virtual async Task<PageMetaModel> GetMetaAsync()
        {
            if (_meta == null)
            {
                _meta = new PageMetaModel {Title = Site.Title, CurrentUrl = new Uri(Site.Url)};
                SeoPropertiesSet seoPropertiesSet = null;
                if (Section != null)
                {
                    seoPropertiesSet = await PropertiesProvider.GetAsync<SeoPropertiesSet>(Section);
                }

                if (seoPropertiesSet == null)
                {
                    seoPropertiesSet = await PropertiesProvider.GetAsync<SeoPropertiesSet>(Site);
                }

                if (seoPropertiesSet != null)
                {
                    _meta.Description = seoPropertiesSet.Description;
                    _meta.Keywords = seoPropertiesSet.Keywords;
                }
            }

            return _meta;
        }

        protected string GetDescriptionFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText.Trim('\r', '\n')).Trim();
        }
    }
}
