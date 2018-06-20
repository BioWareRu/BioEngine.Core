using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class SitesRepository : BioRepository<Site, int>
    {
        internal SitesRepository(BioRepositoryContext<Site, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}