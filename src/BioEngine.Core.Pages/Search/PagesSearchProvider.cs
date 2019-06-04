using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Pages.Db;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Search;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Pages.Search
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

        protected override Task<SearchModel[]> GetSearchModelsAsync(Page[] entities)
        {
            return Task.FromResult(entities.Select(page =>
            {
                return new SearchModel(page.Id, page.Title, page.Url, 
                    string.Join(" ", page.Blocks.Select(b => b.ToString()).Where(s => !string.IsNullOrEmpty(s))), 
                    page.DateAdded)
                {
                    SiteIds = page.SiteIds
                };

            }).ToArray());
        }

        protected override Task<Page[]> GetEntitiesAsync(SearchModel[] searchModels)
        {
            var ids = searchModels.Select(s => s.Id).Distinct().ToArray();
            return _pagesRepository.GetByIdsAsync(ids);
        }
    }
}
