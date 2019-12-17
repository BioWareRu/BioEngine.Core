using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Search
{
    public abstract class SearchModule<T> : BaseBioEngineModule<T> where T : SearchModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<IRepositoryHook, SearchRepositoryHook>();
            ConfigureSearch(services);
        }

        protected abstract void ConfigureSearch(IServiceCollection services);

        public override Task InitAsync(IServiceProvider serviceProvider, IConfiguration configuration,
            IHostEnvironment environment)
        {
            var searchProviders = serviceProvider.GetServices<ISearchProvider>();
            var logger = serviceProvider.GetService<ILogger<SearchModule<T>>>();
            if (searchProviders != null)
            {
                Task.Run(async () =>
                {
                    foreach (var searchProvider in searchProviders)
                    {
                        try
                        {
                            await searchProvider.InitAsync();
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, e.ToString());
                        }
                    }
                });
            }

            return Task.CompletedTask;
        }


    }

    public static class SearchModuleExtensions
    {
        public static IServiceCollection RegisterSearchProvider<TSearchProvider, TEntity>(
            this IServiceCollection serviceCollection) where TSearchProvider : class, ISearchProvider<TEntity>
            where TEntity : BaseEntity
        {
            serviceCollection.AddScoped<TSearchProvider>();
            serviceCollection.AddScoped<ISearchProvider<TEntity>, TSearchProvider>();
            return serviceCollection.AddScoped<ISearchProvider, TSearchProvider>();
        }
    }

    public abstract class SearchModuleConfig
    {
    }
}
