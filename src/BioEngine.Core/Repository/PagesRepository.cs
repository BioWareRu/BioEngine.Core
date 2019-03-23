using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class PagesRepository : SiteEntityRepository<Page>
    {
        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
        
        protected override IQueryable<Page> GetBaseQuery(QueryContext<Page> queryContext = null)
        {
            return ApplyContext(DbContext.Set<Page>().Include(p => p.Blocks), queryContext);
        }
    }
}
