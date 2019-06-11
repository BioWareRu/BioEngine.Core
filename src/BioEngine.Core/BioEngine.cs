using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core
{
    public class BioEngine
    {
        private readonly BioEntitiesManager _entitiesManager = new BioEntitiesManager();
        private readonly List<IBioEngineModule> _modules = new List<IBioEngineModule>();
        private IHost _appHost;

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
                services.AddSingleton(_entitiesManager);
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

        public async Task RunAsync()
        {
            await InitAsync();

            await GetAppHost().RunAsync();
        }

        public async Task RunAsync<TStartup>() where TStartup : class
        {
            _hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
            });

            await RunAsync();
        }

        public IServiceProvider GetServices()
        {
            return GetAppHost().Services;
        }

        private IHost GetAppHost()
        {
            return _appHost ??= _hostBuilder.Build();
        }

        public async Task InitAsync()
        {
            var host = GetAppHost();
            using (var scope = host.Services.CreateScope())
            {
                foreach (var module in _modules)
                {
                    await module.InitAsync(scope.ServiceProvider,
                        scope.ServiceProvider.GetRequiredService<IConfiguration>(),
                        scope.ServiceProvider.GetRequiredService<IHostEnvironment>());
                }
            }
        }

        public BioEngine AddModule<TModule, TModuleConfig>(
            Func<IConfiguration, IHostEnvironment, TModuleConfig> configure)
            where TModule : IBioEngineModule<TModuleConfig>, new() where TModuleConfig : class
        {
            var module = new TModule();
            ConfigureModule(module, configure);
            _modules.Add(module);
            return this;
        }

        public BioEngine AddModule<TModule>()
            where TModule : IBioEngineModule, new()
        {
            var module = new TModule();
            ConfigureModule(module);
            _modules.Add(module);
            return this;
        }


        private void ConfigureModule(IBioEngineModule module, IServiceCollection collection,
            IHostEnvironment environment, IConfiguration configuration)
        {
            ConfigureCore(collection);
            module.ConfigureServices(collection, configuration, environment);
            var validators = module.RegisterValidation();
            if (validators != null && validators.Any())
            {
                foreach (var validator in validators)
                {
                    collection.AddScoped(validator.InterfaceType, validator.ValidatorType);
                }
            }

            module.ConfigureEntities(collection, _entitiesManager);
            collection.TryAddSingleton(_entitiesManager);
        }

        private void ConfigureModule(IBioEngineModule module)
        {
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

        public BioEngine ConfigureAppConfiguration(Action<IConfigurationBuilder> action)
        {
            _hostBuilder.ConfigureAppConfiguration(action);
            return this;
        }
    }
}
