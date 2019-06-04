using BioEngine.Core.API;
using BioEngine.Core.Modules;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Pages.Search;
using BioEngine.Core.Pages.SiteMaps;
using BioEngine.Core.Search;
using cloudscribe.Web.SiteMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Pages
{
    public class PagesModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.RegisterSearchRepositoryHook<Page>()
                .RegisterSearchProvider<PagesSearchProvider, Page>();
            services.AddScoped<ISiteMapNodeService, PagesSiteMapNodeService>();
            services.RegisterApiEntities<Page>();
        }
    }
}
