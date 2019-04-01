using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Search
{
    public interface ISearchProvider
    {
        Task DeleteIndexAsync();
        Task<long> CountAsync(string term);
        Task InitAsync();
    }

    public interface ISearchProvider<T> : ISearchProvider where T : BaseEntity
    {
        Task<IEnumerable<T>> SearchAsync(string term, int limit);
        Task AddOrUpdateEntityAsync(T entity);
        Task<bool> AddOrUpdateEntitiesAsync(IEnumerable<T> entities);
        Task<bool> DeleteEntityAsync(T entity);
    }
}
