using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Interfaces
{
    public interface IBioEngineModule
    {
        void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services);
        void ConfigureHostBuilder(IWebHostBuilder hostBuilder);
    }

    public interface IBioEngineModule<out TConfig> : IBioEngineModule where TConfig : new()
    {
        void Configure(Action<TConfig> config);
    }
}