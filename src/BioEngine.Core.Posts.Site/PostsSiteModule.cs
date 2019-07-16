using BioEngine.Core.Posts.Site.Rss;
using BioEngine.Core.Posts.Site.SiteMaps;
using BioEngine.Core.Site.Rss;
using cloudscribe.Web.SiteMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Posts.Site
{
    public class PostsSiteModule : PostsModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ISiteMapNodeService, PostsSiteMapNodeService>();
            services.AddScoped<IRssItemsProvider, PostsRssItemsProvider>();
        }
    }
}
