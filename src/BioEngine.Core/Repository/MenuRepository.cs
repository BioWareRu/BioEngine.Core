using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class MenuRepository : SiteEntityRepository<Menu, QueryContext<Menu>>
    {
        public MenuRepository(BioRepositoryContext<Menu> repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<Menu> GetSiteMenuAsync(Site site)
        {
            return await GetBaseQuery().Where(m => m.SiteIds.Contains(site.Id)).FirstOrDefaultAsync();
        }
    }
}
