using System;
using System.Collections.Generic;
using System.Reflection;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Modules;
using BioEngine.Core.Providers;
using BioEngine.Core.Repository;
using BioEngine.Core.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BioEngine.Core
{
    public class BioEngineCoreModule : BioEngineModule<BioEngineCoreModuleConfig>
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            if (Config.EnableDatabase)
            {
                AddDatabase(builderContext, services);
            }

            if (Config.EnableValidation)
            {
                AddValidation(services);
            }

            if (Config.EnableSeoExtensions)
            {
                AddSeo();
            }

            if (Config.EnableFileStorage)
            {
                AddFileStorage(builderContext, services);
            }

            if (Config.EnableS3Storage)
            {
                AddS3Storage(builderContext, services);
            }
        }

        private static void AddS3Storage(WebHostBuilderContext context, IServiceCollection services)
        {
            services.Configure<S3StorageOptions>(o =>
            {
                var uri = context.Configuration["BRC_STORAGE_PUBLIC_URI"];
                if (string.IsNullOrEmpty(uri))
                {
                    throw new ArgumentException("Storage url is empty");
                }

                var success = Uri.TryCreate(uri, UriKind.Absolute, out var publicUri);
                if (!success)
                {
                    throw new ArgumentException($"URI {uri} is not proper URI");
                }

                var serverUriStr = context.Configuration["BRC_STORAGE_S3_SERVER_URI"];
                if (string.IsNullOrEmpty(serverUriStr))
                {
                    throw new ArgumentException("S3 server url is empty");
                }

                var bucketName = context.Configuration["BRC_STORAGE_S3_BUCKET"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new ArgumentException("S3 bucketName is empty");
                }

                var accessKey = context.Configuration["BRC_STORAGE_S3_ACCESS_KEY"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new ArgumentException("S3 access key is empty");
                }

                var secretKey = context.Configuration["BRC_STORAGE_S3_SECRET_KEY"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new ArgumentException("S3 secret key is empty");
                }

                success = Uri.TryCreate(serverUriStr, UriKind.Absolute, out var serverUri);
                if (!success)
                {
                    throw new ArgumentException($"S3 server URI {uri} is not proper URI");
                }

                o.PublicUri = publicUri;
                o.Server = serverUri;
                o.Bucket = bucketName;
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
            });
            services.AddSingleton<IStorage, S3Storage>();
        }

        private static void AddFileStorage(WebHostBuilderContext context, IServiceCollection services)
        {
            services.Configure<FileStorageOptions>(o =>
            {
                var path = context.Configuration["BRC_STORAGE_FILE_PATH"];
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("File storage path is empty");
                }

                var uri = context.Configuration["BRC_STORAGE_PUBLIC_URI"];
                if (string.IsNullOrEmpty(uri))
                {
                    throw new ArgumentException("Storage url is empty");
                }

                var success = Uri.TryCreate(uri, UriKind.Absolute, out var publicUri);
                if (!success)
                {
                    throw new ArgumentException($"URI {uri} is not proper URI");
                }

                o.PublicUri = publicUri;
                o.StoragePath = path;
            });
            services.AddSingleton<IStorage, FileStorage>();
        }

        private static void AddSeo()
        {
            SettingsProvider.RegisterBioEngineSectionSettings<SeoSettings>();
            SettingsProvider.RegisterBioEngineContentSettings<SeoSettings>();
            SettingsProvider.RegisterBioEngineSettings<SeoSettings, Site>();
            SettingsProvider.RegisterBioEngineSettings<SeoSettings, Page>();
        }

        private void AddValidation(IServiceCollection services)
        {
            var assembliesList = new List<Assembly>(Config.Assemblies) {typeof(BioContext).Assembly};
            foreach (var assembly in assembliesList)
            {
                var validators = AssemblyScanner.FindValidatorsInAssembly(assembly);
                foreach (var validator in validators)
                {
                    services.AddScoped(validator.InterfaceType, validator.ValidatorType);
                }
            }
        }

        private void AddDatabase(WebHostBuilderContext context, IServiceCollection services)
        {
            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = context.Configuration["BIO_POSTGRES_HOST"] ?? "localhost",
                Port =
                    !string.IsNullOrEmpty(context.Configuration["BIO_POSTGRES_PORT"])
                        ? int.Parse(context.Configuration["BIO_POSTGRES_PORT"])
                        : 5432,
                Username = context.Configuration["BIO_POSTGRES_USERNAME"] ?? "postgres",
                Password = context.Configuration["BIO_POSTGRES_PASSWORD"] ?? "",
                Database = context.Configuration["BIO_POSTGRES_DATABASE"] ?? "postgres",
                Pooling = false
            };

            Config.DBConfigure?.Invoke(connBuilder, context.Configuration);
            services.AddDbContextPool<BioContext>(options =>
            {
                options.UseNpgsql(connBuilder.ConnectionString,
                    builder => builder.MigrationsAssembly(Config.MigrationsAssembly != null
                        ? Config.MigrationsAssembly.FullName
                        : typeof(DbContext).Assembly.FullName));
                if (context.HostingEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            services.AddScoped<SettingsProvider>();

            // collect defined types
            var assembliesList = new List<Assembly>(Config.Assemblies)
                {Config.MigrationsAssembly, typeof(BioContext).Assembly};
            var types = new List<TypeInfo>();
            foreach (var assembly in assembliesList)
            {
                types.AddRange(assembly.DefinedTypes);
            }


            var typesProvider = new TypesProvider();
            var repositoryTypes = new List<Type>();
            foreach (var type in types)
            {
                if (!type.IsAbstract && typeof(Section).IsAssignableFrom(type) &&
                    type.BaseType != null)
                {
                    typesProvider.AddSectionType(type);
                }

                if (!type.IsAbstract && typeof(ContentItem).IsAssignableFrom(type) &&
                    type.BaseType != null)
                {
                    typesProvider.AddContentType(type);
                }

                if (!type.IsAbstract && typeof(IBioRepository).IsAssignableFrom(type) &&
                    type.BaseType != null)
                {
                    repositoryTypes.Add(type);
                }
            }

            BioContext.TypesProvider = typesProvider;

            // register repositories
            services.AddScoped(typeof(BioRepositoryContext<,>));
            foreach (var type in repositoryTypes)
            {
                services.AddScoped(type);
            }
        }
    }

    public class BioEngineCoreModuleConfig
    {
        public bool EnableDatabase = true;
        public bool EnableValidation = false;
        public bool EnableFileStorage = false;
        public bool EnableS3Storage = true;
        public bool EnableSeoExtensions = true;

        public Action<NpgsqlConnectionStringBuilder, IConfiguration> DBConfigure;
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public Assembly MigrationsAssembly;
    }
}