using System;
using System.Reflection;
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
            webHostBuilder.Configure(app =>
            {
                module.Configure(app, app.ApplicationServices.GetRequiredService<IHostingEnvironment>());
            });
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

            EntityMetadataType metadataType;
            if (typeof(Section).IsAssignableFrom(entityType))
            {
                metadataType = EntityMetadataType.Section;
            }
            else if (typeof(ContentItem).IsAssignableFrom(entityType))
            {
                metadataType = EntityMetadataType.ContentItem;
            }
            else
            {
                return services;
            }

            var metaData = GetTypeMetadata(entityType, metadataType);
            services.AddSingleton(typeof(EntityMetadata), metaData);

            return services;
        }

        private static EntityMetadata GetTypeMetadata(Type type, EntityMetadataType entityType)
        {
            var attr = type.GetCustomAttribute<TypedEntityAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Entity type without type attribute: {type}");
            }

            var dataType = type.BaseType?.GenericTypeArguments[0];

            if (dataType == null)
            {
                throw new ArgumentException($"Entity type without data type: {type}");
            }

            return new EntityMetadata(type, attr.Type, dataType, entityType);
        }
    }
}