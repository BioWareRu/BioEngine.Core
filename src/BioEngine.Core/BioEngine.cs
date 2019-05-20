using System;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core
{
    public class BioEngine
    {
        private readonly BioEntitiesManager _entitiesManager = new BioEntitiesManager();
        private readonly BioEntityMetadataManager _entityMetadataManager = new BioEntityMetadataManager();

        private bool _coreConfigured;
        private readonly IHostBuilder _hostBuilder;

        public BioEngine(string[] args)
        {
            _hostBuilder = Host.CreateDefaultBuilder(args);
        }

        private void ConfigureCore(IServiceCollection services)
        {
            if (!_coreConfigured)
            {
                services.AddScoped<PropertiesProvider>();
                services.AddScoped<BioRepositoryHooksManager>();
                services.AddSingleton(_entityMetadataManager);
                services.AddScoped(typeof(BioRepositoryContext<>));
                _coreConfigured = true;
            }
        }

        public BioEngine ConfigureServices(Action<IServiceCollection> conifgure)
        {
            _hostBuilder.ConfigureServices(conifgure);
            return this;
        }

        public BioEngine ConfigureServices(Action<HostBuilderContext, IServiceCollection> conifgure)
        {
            _hostBuilder.ConfigureServices(conifgure);
            return this;
        }

        public IHost Build()
        {
            return _hostBuilder.Build();
        }

        public IHostBuilder GetHostBuilder()
        {
            return _hostBuilder;
        }

        public BioEngine AddModule<TModule, TModuleConfig>(
            Func<IConfiguration, IHostEnvironment, TModuleConfig> configure)
            where TModule : IBioEngineModule<TModuleConfig>, new() where TModuleConfig : class
        {
            var module = new TModule();
            ConfigureModule(module, configure);
            return this;
        }

        public BioEngine AddModule<TModule>()
            where TModule : IBioEngineModule, new()
        {
            ConfigureModule(new TModule());
            return this;
        }


        private void ConfigureModule(IBioEngineModule module, IServiceCollection collection,
            IHostEnvironment environment, IConfiguration configuration)
        {
            ConfigureCore(collection);
            module.ConfigureServices(collection, configuration, environment);
            module.RegisterEntities(_entitiesManager);
            module.RegisterValidation(collection);
            module.RegisterRepositories(collection, _entityMetadataManager);
            collection.TryAddSingleton(_entitiesManager);
        }

        private void ConfigureModule(IBioEngineModule module)
        {
            module.ConfigureHostBuilder(_hostBuilder);
            _hostBuilder.ConfigureServices(
                (context, collection) =>
                {
                    ConfigureModule(module, collection, context.HostingEnvironment, context.Configuration);
                }
            );
        }

        private void ConfigureModule<TModuleConfig>(IBioEngineModule<TModuleConfig> module,
            Func<IConfiguration, IHostEnvironment, TModuleConfig> configure) where TModuleConfig : class
        {
            module.ConfigureHostBuilder(_hostBuilder);
            _hostBuilder.ConfigureServices(
                (context, collection) =>
                {
                    if (configure != null)
                    {
                        module.Configure(configure, context.Configuration, context.HostingEnvironment);
                    }

                    ConfigureModule(module, collection, context.HostingEnvironment, context.Configuration);
                }
            );
        }
    }
}
