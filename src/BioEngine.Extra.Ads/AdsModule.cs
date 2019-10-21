using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Extra.Ads.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Ads
{
    public abstract class AdsModule : BaseBioEngineModule
    {
        public override void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            base.ConfigureDbContext(services, configuration, environment);
            RegisterEntities<AdsModule>(services);
        }
    }

    public class AdsBioContextModelConfigurator : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterSiteEntity<Ad>();
        }
    }
}
