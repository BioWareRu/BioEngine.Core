using System.Linq;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Pages.Db
{
    public class PagesRepository : ContentEntityRepository<Page>
    {
        protected override IQueryable<Page> GetBaseQuery()
        {
            return DbContext.Set<Page>().Include(p => p.Blocks);
        }

        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
