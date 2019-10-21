using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Pages.Search;
using BioEngine.Core.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        public override void ConfigureDbContext(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureDbContext(services, configuration, environment);
            RegisterEntities<PagesModule>(services);
        }
    }

    public class PagesBioContextModelConfigurator : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterSiteEntity<Page>();
            modelBuilder.Entity<Page>().HasIndex(p => p.IsPublished);
            modelBuilder.Entity<Page>().HasIndex(p => p.Url).IsUnique();
        }
    }
}
