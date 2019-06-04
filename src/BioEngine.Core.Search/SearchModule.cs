using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Search
{
    public abstract class SearchModule<T> : BioEngineModule<T> where T : SearchModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            ConfigureSearch(services);
        }

        protected abstract void ConfigureSearch(IServiceCollection services);
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

        public static IServiceCollection RegisterSearchRepositoryHook<TEntity>(
            this IServiceCollection serviceCollection) where TEntity : BaseEntity
        {
            return serviceCollection.AddScoped<IRepositoryHook, SearchRepositoryHook<TEntity>>();
        }
    }

    public abstract class SearchModuleConfig
    {
    }
}
