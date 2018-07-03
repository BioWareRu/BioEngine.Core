using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SiteEntityRepository<Section, int>
    {
        public SectionsRepository(BioRepositoryContext<Section, int> repositoryContext, SitesRepository sitesRepository)
            : base(repositoryContext, sitesRepository)
        {
        }
    }
}