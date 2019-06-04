using BioEngine.Core.Abstractions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.DB
{
    public abstract class DatabaseModule<T> : BioEngineModule<T> where T : DatabaseModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddTransient(typeof(IQueryContext), typeof(QueryContext));
            services.AddTransient(typeof(IQueryContext<>), typeof(QueryContext<>));
        }
    }

    public abstract class DatabaseModuleConfig
    {
    }
}
