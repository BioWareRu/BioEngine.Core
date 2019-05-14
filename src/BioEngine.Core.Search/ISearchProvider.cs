using System;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Search
{
    public interface ISearchProvider
    {
        bool CanProcess(Type type);
        Task DeleteIndexAsync();
        Task<long> CountAsync(string term);
        Task InitAsync();
    }

    public interface ISearchProvider<T> : ISearchProvider where T : BaseEntity
    {
        Task<T[]> SearchAsync(string term, int limit, Site site);
        Task AddOrUpdateEntityAsync(T entity);
        Task<bool> AddOrUpdateEntitiesAsync(T[] entities);
        Task<bool> DeleteEntityAsync(T entity);
    }
}
