using System.Collections.Generic;
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

        protected override Task<IEnumerable<SearchModel>> GetSearchModelsAsync(IEnumerable<T> entities)
        {
            return Task.FromResult(entities.Select(post =>
            {
                var model = new SearchModel
                {
                    Id = post.Id,
                    Url = post.Url,
                    Title = post.Title,
                    Date = post.DateAdded,
                    SiteIds = post.SiteIds,
                    Content = string.Join(" ", post.Blocks.Select(b => b.ToString()).Where(s => !string.IsNullOrEmpty(s)))
                };

                return model;
            }));
        }
    }
}