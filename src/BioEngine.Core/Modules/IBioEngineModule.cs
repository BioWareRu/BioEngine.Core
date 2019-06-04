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
        void ConfigureDbContext(BioEntitiesManager entitiesManager);
        void RegisterValidation(IServiceCollection serviceCollection);
        void RegisterRepositories(IServiceCollection serviceCollection, BioEntityMetadataManager metadataManager);
    }

    public interface IBioEngineModule<TConfig> : IBioEngineModule where TConfig : class
    {
        void Configure(Func<IConfiguration, IHostEnvironment, TConfig> configure, IConfiguration configuration,
            IHostEnvironment environment);
    }
}
