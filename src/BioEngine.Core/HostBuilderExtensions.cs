using System;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddBioEngineModule<TModule, TModuleConfig>(this IHostBuilder webHostBuilder,
            Action<TModuleConfig>? configure = null)
            where TModule : IBioEngineModule<TModuleConfig>, new() where TModuleConfig : new()
        {
            var module = new TModule();
            if (configure != null)
            {
                module.Configure(configure);
            }
            ConfigureModule(webHostBuilder, module);
            return webHostBuilder;
        }

        public static IHostBuilder AddBioEngineModule<TModule>(this IHostBuilder webHostBuilder)
            where TModule : IBioEngineModule, new()
        {
            ConfigureModule(webHostBuilder, new TModule());
            return webHostBuilder;
        }

        private static readonly BioEntitiesManager EntitiesManager = new BioEntitiesManager();

        private static void ConfigureModule(IHostBuilder webHostBuilder, IBioEngineModule module)
        {
            module.ConfigureHostBuilder(webHostBuilder);
            webHostBuilder.ConfigureServices(
                (context, collection) =>
                {
                    module.ConfigureServices(collection, context.Configuration, context.HostingEnvironment);
                    module.RegisterEntities(EntitiesManager);
                    collection.TryAddSingleton(EntitiesManager);
                }
            );
        }
    }
}
