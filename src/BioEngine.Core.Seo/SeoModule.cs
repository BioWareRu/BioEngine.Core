using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Seo
{
    public class SeoModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            PropertiesProvider.RegisterBioEngineSectionProperties<SeoPropertiesSet>("seo");
            PropertiesProvider.RegisterBioEngineContentProperties<SeoPropertiesSet>("seo");
            PropertiesProvider.RegisterBioEngineProperties<SeoPropertiesSet, Site>("seo");
        }
    }
}
