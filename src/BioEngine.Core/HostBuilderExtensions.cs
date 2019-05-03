using System;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BioEngine.Core
{
    public static class HostBuilderExtensions
    {
        public static IWebHostBuilder AddBioEngineModule<TModule, TModuleConfig>(this IWebHostBuilder webHostBuilder,
            Action<TModuleConfig> configure = null)
            where TModule : IBioEngineModule<TModuleConfig>, new() where TModuleConfig : new()
        {
            var module = new TModule();
            module.Configure(configure);
            ConfigureModule(webHostBuilder, module);
            return webHostBuilder;
        }

        public static IWebHostBuilder AddBioEngineModule<TModule>(this IWebHostBuilder webHostBuilder)
            where TModule : IBioEngineModule, new()
        {
            ConfigureModule(webHostBuilder, new TModule());
            return webHostBuilder;
        }

        private static readonly BioEntitiesManager EntitiesManager = new BioEntitiesManager();

        private static void ConfigureModule(IWebHostBuilder webHostBuilder, IBioEngineModule module)
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
