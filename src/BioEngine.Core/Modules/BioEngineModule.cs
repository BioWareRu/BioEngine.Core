using System;
using BioEngine.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Modules
{
    public abstract class BioEngineModule : IBioEngineModule
    {
        public virtual void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
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