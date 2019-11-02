using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Social;
using BioEngine.Extra.Facebook.Entities;
using BioEngine.Extra.Facebook.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Facebook
{
    public class FacebookModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddSingleton<FacebookService>();
            services.AddScoped<IContentPublisher<FacebookConfig>, FacebookContentPublisher>();
            services.AddScoped<FacebookContentPublisher>();

            PropertiesProvider.RegisterBioEngineProperties<FacebookSitePropertiesSet, Site>("facebooksite");
        }
    }
    
    public class FacebookBioContextConfigurator: IBioContextModelConfigurator{
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterEntity<FacebookPublishRecord>();
        }
    }
    
}
