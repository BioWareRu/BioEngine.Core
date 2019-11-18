using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.DB
{
    public abstract class DatabaseModule<T> : BaseBioEngineModule<T> where T : DatabaseModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = null;
            });
        }
    }

    public abstract class DatabaseModuleConfig
    {
    }
}
