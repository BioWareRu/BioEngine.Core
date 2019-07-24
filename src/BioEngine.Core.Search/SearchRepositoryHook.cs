using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Search
{
    public class SearchRepositoryHook : BaseRepositoryHook
    {
        private readonly IEnumerable<ISearchProvider> _searchProviders;

        public SearchRepositoryHook(IEnumerable<ISearchProvider> searchProviders)
        {
            _searchProviders = searchProviders;
        }

        private ISearchProvider<T> GetSearchProvider<T>() where T : IBioEntity
        {
            var provider = _searchProviders.FirstOrDefault(s => s.CanProcess(typeof(T)));
            return provider as ISearchProvider<T>;
        }

        private ISearchProvider GetSearchProvider(Type entityType)
        {
            var provider = _searchProviders.FirstOrDefault(s => s.CanProcess(entityType));
            return provider;
        }

        public override bool CanProcess(Type type)
        {
            return GetSearchProvider(type) != null;
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var provider = GetSearchProvider<T>();
            if (provider != null)
            {
                var needIndex = true;
                if (item is IContentEntity contentEntity)
                {
                    needIndex = contentEntity.IsPublished;
                }

                if (needIndex)
                {
                    await provider.AddOrUpdateEntityAsync(item);
                }
                else
                {
                    await provider.DeleteEntityAsync(item);
                }

                return true;
            }

            return false;
        }
    }
}
