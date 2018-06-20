using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Repository;
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
                var repositoryRegisterMethod = typeof(HostBuilderExtensions)
                    .GetMethod(nameof(AddRepository), BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var type in repositoryTypes)
                {
                    if (type.BaseType != null)
                    {
                        var entityType = type.BaseType.GenericTypeArguments[0];
                        var entityIdType = type.BaseType.GenericTypeArguments[1];

                        var registerMethod =
                            repositoryRegisterMethod?.MakeGenericMethod(type, entityType, entityIdType);

                        registerMethod?.Invoke(null, new object[] {services});
                    }
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
    }

    internal class RepostioryRegistrator
    {
        public IServiceCollection RegisterRepositories(IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}