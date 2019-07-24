using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Posts.Search;
using BioEngine.Core.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Posts
{
    public abstract class PostsModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.RegisterSearchProvider<PostsSearchProvider, Post>();
        }

        public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
            base.ConfigureEntities(serviceCollection, entitiesManager);
            RegisterRepositories(typeof(Post).Assembly, serviceCollection, entitiesManager);
        }
    }
}
