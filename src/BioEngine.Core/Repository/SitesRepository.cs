using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SitesRepository : BioRepository<Site, QueryContext<Site>>
    {
        public SitesRepository(BioRepositoryContext<Site> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
