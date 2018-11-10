using System;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            webHostBuilder.ConfigureServices(module.ConfigureServices);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterEntityType(this IServiceCollection services, Type entityType)
        {
            if (entityType.IsAbstract || entityType.BaseType == null)
            {
                return services;
            }

            if (!typeof(Section).IsAssignableFrom(entityType) && !typeof(PostBlock).IsAssignableFrom(entityType))
            {
                return services;
            }

            var metaData = GetTypeMetadata(entityType);
            services.AddSingleton(typeof(EntityMetadata), metaData);

            return services;
        }

        private static EntityMetadata GetTypeMetadata(Type type)
        {
            var dataType = type.BaseType?.GenericTypeArguments[0];

            if (dataType == null)
            {
                throw new ArgumentException($"Entity type without data type: {type}");
            }

            return new EntityMetadata(type, dataType);
        }
    }
}