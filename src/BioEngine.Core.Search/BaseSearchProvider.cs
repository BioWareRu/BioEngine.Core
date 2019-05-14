using System;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Search
{
    public abstract class BaseSearchProvider<T> : ISearchProvider<T> where T : BaseEntity
    {
        private readonly ISearcher _searcher;
        protected readonly ILogger<BaseSearchProvider<T>> Logger;

        protected BaseSearchProvider(ISearcher searcher, ILogger<BaseSearchProvider<T>> logger)
        {
            _searcher = searcher;
            Logger = logger;
        }

        private string IndexName => typeof(T).FullName.ToLowerInvariant();

        public bool CanProcess(Type type)
        {
            return typeof(T) == type;
        }

        public Task DeleteIndexAsync()
        {
            return _searcher.DeleteAsync(IndexName);
        }

        public Task<long> CountAsync(string term)
        {
            return _searcher.CountAsync(IndexName, term);
        }

        public Task InitAsync()
        {
            return _searcher.InitAsync(IndexName);
        }

        public async Task<T[]> SearchAsync(string term, int limit, Site site)
        {
            var result = await _searcher.SearchAsync(IndexName, term, limit, site);
            return await GetEntitiesAsync(result);
        }

        public Task AddOrUpdateEntityAsync(T entity)
        {
            return AddOrUpdateEntitiesAsync(new[] {entity});
        }

        public async Task<bool> AddOrUpdateEntitiesAsync(T[] entities)
        {
            return await _searcher.AddOrUpdateAsync(IndexName, await GetSearchModelsAsync(entities));
        }

        public async Task<bool> DeleteEntityAsync(T entity)
        {
            return await _searcher.DeleteAsync(IndexName, await GetSearchModelsAsync(new[] {entity}));
        }

        protected abstract Task<SearchModel[]> GetSearchModelsAsync(T[] entities);
        protected abstract Task<T[]> GetEntitiesAsync(SearchModel[] searchModels);
    }
}
