using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Modules
{
    public abstract class BioEngineModule : IBioEngineModule
    {
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
        }

        public virtual void ConfigureHostBuilder(IWebHostBuilder hostBuilder)
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
