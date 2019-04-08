using System;
using BioEngine.Core.Modules;
using Microsoft.AspNetCore.Hosting;

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

        private static void ConfigureModule(IWebHostBuilder webHostBuilder, IBioEngineModule module)
        {
            module.ConfigureHostBuilder(webHostBuilder);
            webHostBuilder.ConfigureServices((context, collection) =>
                module.ConfigureServices(collection, context.Configuration, context.HostingEnvironment));
        }
    }
}
