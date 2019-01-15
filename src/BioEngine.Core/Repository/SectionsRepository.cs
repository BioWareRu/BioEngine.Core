using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SiteEntityRepository<Section, int>
    {
        public SectionsRepository(BioRepositoryContext<Section, int> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<Section> GetBaseQuery(QueryContext<Section, int> queryContext = null)
        {
            return base.GetBaseQuery(queryContext).Include(s => s.Logo).Include(s => s.LogoSmall);
        }
    }
}