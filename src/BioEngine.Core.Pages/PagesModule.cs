using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Pages.Search;
using BioEngine.Core.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Pages
{
    public abstract class PagesModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.RegisterSearchProvider<PagesSearchProvider, Page>();
        }

        public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
            base.ConfigureEntities(serviceCollection, entitiesManager);
            RegisterRepositories(typeof(Page).Assembly, serviceCollection, entitiesManager);
        }
    }
}
