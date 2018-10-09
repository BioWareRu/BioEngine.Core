using BioEngine.Core.Entities;
using BioEngine.Core.Providers;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SiteEntityRepository<Section, int>
    {
        public SectionsRepository(BioRepositoryContext<Section, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}