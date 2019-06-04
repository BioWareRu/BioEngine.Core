using BioEngine.Core.Abstractions;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Site.Sitemaps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Pages.SiteMaps
{
    public class PagesSiteMapNodeService : BaseSiteMapNodeService<Page>
    {
        public PagesSiteMapNodeService(IHttpContextAccessor httpContextAccessor,
            IBioRepository<Page> repository, LinkGenerator linkGenerator) :
            base(httpContextAccessor, repository, linkGenerator)
        {
        }
    }
}
