using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SectionRepository<Section>
    {
        public SectionsRepository(BioRepositoryContext<Section> repositoryContext) : base(repositoryContext)
        {
        }
    }

    public abstract class SectionRepository<T> : SiteEntityRepository<T> where T : Section
    {
        public SectionRepository(BioRepositoryContext<T> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<T> GetBaseQuery(QueryContext<T>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<T>().Include(p => p.Blocks), queryContext);
        }
    }
}
