using System;
using BioEngine.Core.DB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Modules
{
    public interface IBioEngineModule
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment);

        void ConfigureHostBuilder(IHostBuilder hostBuilder);
        void RegisterEntities(BioEntitiesManager entitiesManager);
    }

    public interface IBioEngineModule<out TConfig> : IBioEngineModule where TConfig : new()
    {
        void Configure(Action<TConfig> config);
    }
}
