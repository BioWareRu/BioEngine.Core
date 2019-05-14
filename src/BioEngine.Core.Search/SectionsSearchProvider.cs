using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Search
{
    [UsedImplicitly]
    public abstract class SectionsSearchProvider<T> : BaseSearchProvider<T>
        where T : Section
    {
        public SectionsSearchProvider(ISearcher searcher, ILogger<BaseSearchProvider<T>> logger) : base(searcher,
            logger)
        {
        }

        protected override Task<SearchModel[]> GetSearchModelsAsync(T[] entities)
        {
            return Task.FromResult(entities.Select(section =>
            {
                return new SearchModel(section.Id, section.Title, section.Url,
                    string.Join(" ", section.Blocks.Select(b => b.ToString()).Where(s => !string.IsNullOrEmpty(s))),
                    section.DateAdded) {SiteIds = section.SiteIds};
            }).ToArray());
        }
    }
}
