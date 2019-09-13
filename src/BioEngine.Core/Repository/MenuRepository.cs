using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class MenuRepository : SiteEntityRepository<Menu>
    {
        public MenuRepository(BioRepositoryContext<Menu> repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<Menu?> GetSiteMenuAsync(Site site)
        {
            return await GetAsync(q => Task.FromResult(q.Where(m => m.SiteIds.Contains(site.Id))));
        }
    }
}
