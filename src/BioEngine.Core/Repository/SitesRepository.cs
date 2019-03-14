using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SitesRepository : BioRepository<Site>
    {
        public SitesRepository(BioRepositoryContext<Site> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
