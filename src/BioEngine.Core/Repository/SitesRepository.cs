using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SitesRepository : BioRepository<Site, int>
    {
        public SitesRepository(BioRepositoryContext<Site, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}