using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Search
{
    public interface ISearcher
    {
        Task<bool> AddOrUpdateAsync(string indexName, IEnumerable<SearchModel> searchModels);
        Task<bool> DeleteAsync(string indexName, IEnumerable<SearchModel> searchModels);
        Task<bool> DeleteAsync(string indexName);
        Task<long> CountAsync(string indexName, string term, Site site);
        Task<SearchModel[]> SearchAsync(string indexName, string term, int limit, Site site);
        Task InitAsync(string indexName);
    }
}
