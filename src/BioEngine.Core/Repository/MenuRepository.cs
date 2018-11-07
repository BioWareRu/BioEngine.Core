using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class MenuRepository : SingleSiteEntityRepository<Menu, int>
    {
        public MenuRepository(BioRepositoryContext<Menu, int> repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<Menu> GetSiteMenuAsync(Site site)
        {
            return await GetBaseQuery().Where(m => m.SiteId == site.Id).FirstOrDefaultAsync();
        }
    }
}