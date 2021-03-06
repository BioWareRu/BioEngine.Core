using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Seo
{
    public class SeoModule : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            PropertiesProvider.RegisterBioEngineSectionProperties<SeoContentPropertiesSet>("seo");
            PropertiesProvider.RegisterBioEngineContentProperties<SeoContentPropertiesSet>("seo");
            PropertiesProvider.RegisterBioEngineProperties<SeoSitePropertiesSet, Site>("seosite");
        }
    }
}
