using System.Collections.Generic;
using System.Threading.Tasks;
using cloudscribe.Syndication.Models.Rss;

namespace BioEngine.Core.Site.Rss
{
    public interface IRssItemsProvider
    {
        Task<IEnumerable<RssItem>> GetItemsAsync(Core.Entities.Site site, int count);
    }
}
