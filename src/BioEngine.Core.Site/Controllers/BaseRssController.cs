using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Site.Rss;
using BioEngine.Core.Web;
using cloudscribe.Syndication.Models.Rss;
using cloudscribe.Syndication.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Site.Controllers
{
    public abstract class BaseRssController : BaseSiteController
    {
        private readonly IEnumerable<IRssItemsProvider> _itemsProviders;
        private IXmlFormatter XmlFormatter { get; } = new DefaultXmlFormatter();

        protected BaseRssController(BaseControllerContext context,
            IEnumerable<IRssItemsProvider> itemsProviders = null) :
            base(context)
        {
            _itemsProviders = itemsProviders;
        }

        protected virtual int RssFeedSize { get; } = 20;

        [SuppressMessage("ReSharper", "UseAsyncSuffix")]
        public virtual async Task<IActionResult> IndexAsync()
        {
            var channel = new RssChannel
            {
                Title = Site.Title,
                Description = "Последние публикации",
                Link = new Uri(Site.Url),
                Language = CultureInfo.CurrentCulture,
                TimeToLive = 60,
                LastBuildDate = DateTime.Now,
                Copyright = $"(c) {Site.Title}"
            };

            var items = new List<RssItem>();
            if (_itemsProviders != null)
            {
                foreach (var itemsProvider in _itemsProviders)
                {
                    var providerItems = await itemsProvider.GetItemsAsync(Site, RssFeedSize);
                    items.AddRange(providerItems);
                }
            }

            channel.Items = items.OrderByDescending(i => i.PublicationDate);

            var syncIoFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIoFeature != null)
            {
                syncIoFeature.AllowSynchronousIO = true;
            }

            var xml = XmlFormatter.BuildXml(channel);
            return new XmlResult(xml);
        }
    }
}
