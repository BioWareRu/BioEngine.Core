using System;
using BioEngine.Core.DB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Modules
{
    public abstract class BioEngineModule : IBioEngineModule
    {
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
        }

        public virtual void ConfigureHostBuilder(IHostBuilder hostBuilder)
        {
        }

        public virtual void RegisterEntities(BioEntitiesManager entitiesManager)
        {
        }
    }

    public abstract class BioEngineModule<TConfig> : BioEngineModule, IBioEngineModule<TConfig> where TConfig : new()
    {
        protected readonly TConfig Config = new TConfig();

        public virtual void Configure(Action<TConfig> config)
        {
            config?.Invoke(Config);
        }
    }
}
