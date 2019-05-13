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
        void RegisterValidation(IServiceCollection serviceCollection);
        void RegisterRepositories(IServiceCollection serviceCollection, BioEntityMetadataManager metadataManager);
    }

    public interface IBioEngineModule<out TConfig> : IBioEngineModule where TConfig : new()
    {
        void Configure(Action<TConfig, IConfiguration, IHostEnvironment> config, IConfiguration configuration, IHostEnvironment environment);
    }
}
