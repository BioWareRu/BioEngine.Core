using BioEngine.Core.Pages.Site.SiteMaps;
using cloudscribe.Web.SiteMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Pages.Site
{
    public class PagesSiteModule : PagesModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ISiteMapNodeService, PagesSiteMapNodeService>();
        }
    }
}
