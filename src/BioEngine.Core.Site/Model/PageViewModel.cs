using System;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Seo;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Site.Model
{
    public abstract class PageViewModel
    {
        public readonly Entities.Site Site;
        public readonly Section Section;
        protected readonly PropertiesProvider PropertiesProvider;
        protected readonly LinkGenerator LinkGenerator;
        public string Version { get; }

        protected PageViewModel(PageViewModelContext context)
        {
            Site = context.Site;
            Section = context.Section;
            PropertiesProvider = context.PropertiesProvider;
            LinkGenerator = context.LinkGenerator;
            Version = context.Version;
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
                SeoContentPropertiesSet seoPropertiesSet = null;
                if (Section != null)
                {
                    seoPropertiesSet = await PropertiesProvider.GetAsync<SeoContentPropertiesSet>(Section);
                }

                if (seoPropertiesSet != null)
                {
                    _meta.Description = seoPropertiesSet.Description;
                    _meta.Keywords = seoPropertiesSet.Keywords;
                }
                else
                {
                    var sitePropertiesSet = await PropertiesProvider.GetAsync<SeoSitePropertiesSet>(Site);
                    if (sitePropertiesSet != null)
                    {
                        _meta.Description = sitePropertiesSet.Description;
                        _meta.Keywords = sitePropertiesSet.Keywords;
                    }
                }
            }

            return _meta;
        }
    }

    public class PageViewModel<T> : PageViewModel
    {
        public T Data { get; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }

        public PageViewModel(PageViewModelContext context, T data) : base(context)
        {
            Data = data;
        }

        public override Task<PageMetaModel> GetMetaAsync()
        {
            var meta = new PageMetaModel {Title = Title, Description = Description, Keywords = Keywords};
            return Task.FromResult(meta);
        }
    }
}
