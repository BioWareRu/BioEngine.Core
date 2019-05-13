using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Storage
{
    public abstract class StorageModule<T> : BioEngineModule<T> where T : StorageModuleConfig, new()
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            ConfigureStorage(services);
        }

        protected abstract void ConfigureStorage(IServiceCollection services);
    }

    public abstract class StorageModuleConfig
    {
    }
}
