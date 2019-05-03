using System;
using BioEngine.Core.DB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Modules
{
    public interface IBioEngineModule
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment);

        void ConfigureHostBuilder(IWebHostBuilder hostBuilder);
        void RegisterEntities(BioEntitiesManager entitiesManager);
    }

    public interface IBioEngineModule<out TConfig> : IBioEngineModule where TConfig : new()
    {
        void Configure(Action<TConfig> config);
    }
}
