using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Extra.Ads.Entities;
using BioEngine.Extra.Ads.Site;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BioEngine.Extra.Ads
{
    public class AdsModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<AdsProvider>();
        }

        public override void RegisterEntities(BioEntitiesManager entitiesManager)
        {
            base.RegisterEntities(entitiesManager);
            entitiesManager.Register<Ad>(builder =>
            {
                builder.Entity<Ad>().Property(a => a.Picture).HasConversion(
                    p => JsonConvert.SerializeObject(p), json => JsonConvert.DeserializeObject<StorageItem>(json));
            });
        }
    }
}
