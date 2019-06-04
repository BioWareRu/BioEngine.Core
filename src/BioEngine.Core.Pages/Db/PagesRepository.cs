using System.Linq;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Pages.Db
{
    public class PagesRepository : ContentEntityRepository<Page>
    {
        protected override IQueryable<Page> GetBaseQuery(IQueryContext<Page>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<Page>().Include(p => p.Blocks), queryContext);
        }

        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
