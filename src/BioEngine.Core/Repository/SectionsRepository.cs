using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : BioRepository<Section, int>
    {
        public SectionsRepository(BioRepositoryContext<Section, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}