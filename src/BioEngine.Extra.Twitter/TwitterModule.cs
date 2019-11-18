using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Social;
using BioEngine.Extra.Twitter.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Twitter
{
    public class TwitterModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddSingleton<TwitterService>();
            services.AddScoped<IContentPublisher<TwitterPublishConfig>, TwitterContentPublisher>();
            services.AddScoped<TwitterContentPublisher>();

            PropertiesProvider.RegisterBioEngineProperties<TwitterSitePropertiesSet, Site>("twittersite");
        }
    }
    
    public class TwitterBioContextConfigurator: IBioContextModelConfigurator{
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterSiteEntity<TwitterPublishRecord>();
        }
    }
}
