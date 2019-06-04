using BioEngine.Core.API;
using BioEngine.Core.Modules;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Posts.Search;
using BioEngine.Core.Posts.SiteMaps;
using BioEngine.Core.Search;
using cloudscribe.Web.SiteMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Posts
{
    public class PostsModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.RegisterSearchRepositoryHook<Post>()
                .RegisterSearchProvider<PostsSearchProvider, Post>();
            services.AddScoped<ISiteMapNodeService, PostsSiteMapNodeService>();
            services.RegisterApiEntities<Post>();
        }
    }
}
