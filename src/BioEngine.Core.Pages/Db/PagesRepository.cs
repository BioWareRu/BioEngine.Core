using System.Linq;
using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Pages.Db
{
    public class PagesRepository : ContentEntityRepository<Page>
    {
        protected override IQueryable<Page> AddIncludes(IQueryable<Page> query)
        {
            return base.AddIncludes(query).Include(p => p.Blocks);
        }

        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
