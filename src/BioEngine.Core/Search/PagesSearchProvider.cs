using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Search
{
    [UsedImplicitly]
    public class PagesSearchProvider : BaseSearchProvider<Page>
    {
        private readonly PagesRepository _pagesRepository;

        public PagesSearchProvider(ISearcher searcher, ILogger<BaseSearchProvider<Page>> logger,
            PagesRepository pagesRepository) : base(searcher,
            logger)
        {
            _pagesRepository = pagesRepository;
        }

        protected override Task<IEnumerable<SearchModel>> GetSearchModelsAsync(IEnumerable<Page> entities)
        {
            return Task.FromResult(entities.Select(page =>
            {
                return new SearchModel(page.Id, page.Title, page.Url, 
                    string.Join(" ", page.Blocks.Select(b => b.ToString()).Where(s => !string.IsNullOrEmpty(s))), 
                    page.DateAdded)
                {
                    SiteIds = page.SiteIds
                };

            }));
        }

        protected override async Task<IEnumerable<Page>> GetEntitiesAsync(IEnumerable<SearchModel> searchModels)
        {
            var ids = searchModels.Select(s => s.Id).Distinct().ToArray();
            return await _pagesRepository.GetByIdsAsync(ids);
        }
    }
}
