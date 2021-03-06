using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core
{
    public class BioEngine
    {
        private readonly List<IBioEngineModule> _modules = new List<IBioEngineModule>();
        private IHost _appHost;

        private readonly IHostBuilder _hostBuilder;

        private bool _registerEntities;

        public BioEngine(string[] args)
        {
            _hostBuilder = Host.CreateDefaultBuilder(args);
            AddModule<BioEngineCoreModule, BioEngineCoreModuleConfig>((configuration, environment) =>
            {
                var version = $"dev-{Guid.NewGuid()}";
                if (!string.IsNullOrEmpty(configuration["APP_VERSION"]))
                {
                    version = configuration["APP_VERSION"];
                }

                return new BioEngineCoreModuleConfig(version);
            });
        }

        public BioEngine AddEntities()
        {
            _registerEntities = true;
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddScoped<PropertiesProvider>();
                services.AddScoped<BioRepositoryHooksManager>();
                services.AddScoped(typeof(BioRepositoryContext<>));
            });
            return this;
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

        public BioEngine Run<TStartup>(int port = 0) where TStartup : class
        {
            GetHostBuilder().ConfigureWebHostDefaults(builder =>
                builder.UseStartup<TStartup>().UseUrls($"http://*:{port.ToString()}"));

            GetAppHost().Start();
            return this;
        }

        public async Task ExecuteAsync<TStartup>(Func<IServiceProvider, Task> command) where TStartup : class
        {
            GetHostBuilder().UseConsoleLifetime();
            using (var host = UseStartup<TStartup>().GetAppHost())
            {
                await InitAsync();

                var serviceProvider = host.Services;
                await host.StartAsync();
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        await command(scope.ServiceProvider);
                    }
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetService<ILogger<BioEngine>>();
                    logger.LogError(ex, ex.ToString());
                }

                await host.StopAsync();
            }
        }

        private BioEngine UseStartup<TStartup>() where TStartup : class
        {
            _hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
            });
            return this;
        }

        public async Task RunAsync<TStartup>() where TStartup : class
        {
            await UseStartup<TStartup>().RunAsync();
        }

        public IServiceProvider GetServices()
        {
            return GetAppHost().Services;
        }

        private IHost GetAppHost()
        {
            return _appHost ??= _hostBuilder.Build();
        }

        public IHostBuilder GetHostBuilder()
        {
            return _hostBuilder;
        }

        private async Task InitAsync()
        {
            var host = GetAppHost();
            using var scope = host.Services.CreateScope();
            foreach (var module in _modules)
            {
                await module.InitAsync(scope.ServiceProvider,
                    scope.ServiceProvider.GetRequiredService<IConfiguration>(),
                    scope.ServiceProvider.GetRequiredService<IHostEnvironment>());
            }
        }

        public BioEngine AddModule<TModule, TModuleConfig>(
            Func<IConfiguration, IHostEnvironment, TModuleConfig> configure)
            where TModule : IBioEngineModule<TModuleConfig>, new() where TModuleConfig : class
        {
            if (configure == null)
            {
                throw new Exception($"Module {typeof(TModule)} added without configuration method");
            }

            var module = new TModule();
            ConfigureModule(module, configure);
            _modules.Add(module);
            return this;
        }

        public BioEngine AddModule<TModule>()
            where TModule : IBioEngineModule, new()
        {
            if (typeof(TModule).GetTypeInfo().ImplementedInterfaces
                .Any(i => i.Name.StartsWith("IBioEngineModule") && i.IsGenericType))
            {
                throw new Exception($"Module {typeof(TModule)} is implementing IBioEngineModule<TConfig>. It must be added using AddModule<TModule, TModuleConfig> method.");
            }
            var module = new TModule();
            ConfigureModule(module);
            _modules.Add(module);
            return this;
        }


        private void ConfigureModule(IBioEngineModule module, IServiceCollection collection,
            IHostEnvironment environment, IConfiguration configuration)
        {
            module.ConfigureServices(collection, configuration, environment);
            var validators = module.RegisterValidation();
            if (validators != null && validators.Any())
            {
                foreach (var validator in validators)
                {
                    collection.AddScoped(validator.InterfaceType, validator.ValidatorType);
                }
            }

            if (_registerEntities)
            {
                module.ConfigureDbContext(collection, configuration, environment);
            }
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
