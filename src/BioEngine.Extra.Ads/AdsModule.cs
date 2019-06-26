using BioEngine.Core.Api;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Extra.Ads.Entities;
using BioEngine.Extra.Ads.Site;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.Ads
{
    public class AdsModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<AdsProvider>();
            services.RegisterApiEntities(GetType().Assembly);
        }

        public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
            base.ConfigureEntities(serviceCollection, entitiesManager);
            entitiesManager.ConfigureDbContext(modelBuilder =>
            {
                modelBuilder.Entity<Ad>().HasMany(contentItem => contentItem.Blocks).WithOne()
                    .HasForeignKey(c => c.ContentId);
            });
        }
    }
}
