using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
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
    public static class HostBuilderExtensions
    {
        public static IWebHostBuilder AddBioEngineDB(this IWebHostBuilder hostBuilder,
            Action<NpgsqlConnectionStringBuilder, IConfiguration> configure = null, Assembly domainAssembly = null,
            params Assembly[] assemblies)
        {
            hostBuilder.ConfigureServices((context, services) =>
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

                configure?.Invoke(connBuilder, context.Configuration);
                services.AddDbContextPool<BioContext>(options =>
                {
                    options.UseNpgsql(connBuilder.ConnectionString,
                        builder => builder.MigrationsAssembly(domainAssembly != null
                            ? domainAssembly.FullName
                            : typeof(DbContext).Assembly.FullName));
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                    }
                });

                // collect defined types
                var assembliesList = new List<Assembly>(assemblies) {domainAssembly, typeof(BioContext).Assembly};
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
            });
            return hostBuilder;
        }

        private static IServiceCollection AddRepository<TRepository, TEntity, TId>(IServiceCollection services)
            where TRepository : BioRepository<TEntity, TId> where TEntity : class, IEntity<TId>
        {
            services.AddScoped<TRepository>(provider =>
            {
                var context = new BioRepositoryContext<TEntity, TId>(provider.GetRequiredService<BioContext>(),
                    provider.GetServices<IValidator<TEntity>>().ToArray());
                return (TRepository) Activator.CreateInstance(typeof(TRepository),
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] {context}, null);
            });

            return services;
        }

        public static IWebHostBuilder AddBioEngineValidation(this IWebHostBuilder webHostBuilder,
            params Assembly[] assemblies)
        {
            var assembliesList = new List<Assembly>(assemblies) {typeof(BioContext).Assembly};
            webHostBuilder.ConfigureServices((context, services) =>
            {
                foreach (var assembly in assembliesList)
                {
                    var validators = AssemblyScanner.FindValidatorsInAssembly(assembly);
                    foreach (var validator in validators)
                    {
                        services.AddScoped(validator.InterfaceType, validator.ValidatorType);
                    }
                }
            });

            return webHostBuilder;
        }

        public static IWebHostBuilder AddBioEngineFileStorage(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureServices((context, collection) =>
            {
                collection.Configure<FileStorageOptions>(o =>
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
                collection.AddSingleton<IStorage, FileStorage>();
            });
            return webHostBuilder;
        }

        public static IWebHostBuilder AddBioEngineS3Storage(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureServices((context, collection) =>
            {
                collection.Configure<S3StorageOptions>(o =>
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
                collection.AddSingleton<IStorage, S3Storage>();
            });
            return webHostBuilder;
        }
    }

    internal class RepostioryRegistrator
    {
        public IServiceCollection RegisterRepositories(IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}