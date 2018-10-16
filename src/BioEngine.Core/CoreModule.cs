using System;
using System.Collections.Generic;
using System.Reflection;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Modules;
using BioEngine.Core.Repository;
using BioEngine.Core.Seo;
using BioEngine.Core.Settings;
using BioEngine.Core.Storage;
using BioEngine.Core.Web;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BioEngine.Core
{
    public class CoreModule : BioEngineModule<CoreModuleConfig>
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
            
            services.AddScoped<BaseControllerContext>();
            services.AddScoped(typeof(BaseControllerContext<,>));
        }

        private static void AddS3Storage(WebHostBuilderContext context, IServiceCollection services)
        {
            services.Configure<S3StorageOptions>(o =>
            {
                var uri = context.Configuration["BE_STORAGE_PUBLIC_URI"];
                if (string.IsNullOrEmpty(uri))
                {
                    throw new ArgumentException("Storage url is empty");
                }

                var success = Uri.TryCreate(uri, UriKind.Absolute, out var publicUri);
                if (!success)
                {
                    throw new ArgumentException($"URI {uri} is not proper URI");
                }

                var serverUriStr = context.Configuration["BE_STORAGE_S3_SERVER_URI"];
                if (string.IsNullOrEmpty(serverUriStr))
                {
                    throw new ArgumentException("S3 server url is empty");
                }

                var bucketName = context.Configuration["BE_STORAGE_S3_BUCKET"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new ArgumentException("S3 bucketName is empty");
                }

                var accessKey = context.Configuration["BE_STORAGE_S3_ACCESS_KEY"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new ArgumentException("S3 access key is empty");
                }

                var secretKey = context.Configuration["BE_STORAGE_S3_SECRET_KEY"];
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
                var path = context.Configuration["BE_STORAGE_FILE_PATH"];
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("File storage path is empty");
                }

                var uri = context.Configuration["BE_STORAGE_PUBLIC_URI"];
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
                Host = context.Configuration["BE_POSTGRES_HOST"] ?? "localhost",
                Port =
                    !string.IsNullOrEmpty(context.Configuration["BE_POSTGRES_PORT"])
                        ? int.Parse(context.Configuration["BE_POSTGRES_PORT"])
                        : 5432,
                Username = context.Configuration["BE_POSTGRES_USERNAME"] ?? "postgres",
                Password = context.Configuration["BE_POSTGRES_PASSWORD"] ?? "",
                Database = context.Configuration["BE_POSTGRES_DATABASE"] ?? "postgres",
                Pooling = false
            };

            Config.DbConfigure?.Invoke(connBuilder, context.Configuration);
            services.AddEntityFrameworkNpgsql();
            services.AddDbContextPool<BioContext>((p, options) =>
            {
                options.UseNpgsql(connBuilder.ConnectionString,
                    builder => builder.MigrationsAssembly(Config.MigrationsAssembly != null
                        ? Config.MigrationsAssembly.FullName
                        : typeof(DbContext).Assembly.FullName)).UseInternalServiceProvider(p);
                if (context.HostingEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            services.AddScoped<SettingsProvider>();

            // collect defined types
            var assembliesList = new List<Assembly>(Config.Assemblies)
                {Config.MigrationsAssembly, typeof(BioContext).Assembly};
            var types = new HashSet<TypeInfo>();
            foreach (var assembly in assembliesList)
            {
                
                foreach (var definedType in assembly.DefinedTypes)
                {
                    types.Add(definedType);    
                }
            }

            services.Scan(s =>
                s.FromAssemblies(assembliesList).AddClasses(classes => classes.AssignableTo<IBioRepository>())
                    .AsSelfWithInterfaces());

            foreach (var type in types)
            {
                services.RegisterEntityType(type);
            }

            services.AddScoped(typeof(BioRepositoryContext<,>));
        }
    }

    [PublicAPI]
    public class CoreModuleConfig
    {
        public bool EnableDatabase = true;
        public bool EnableValidation;
        public bool EnableFileStorage;
        public bool EnableS3Storage = true;
        public bool EnableSeoExtensions = true;

        public Action<NpgsqlConnectionStringBuilder, IConfiguration> DbConfigure;
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public Assembly MigrationsAssembly;
    }
}