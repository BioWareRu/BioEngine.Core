using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SectionRepository<Section>
    {
        public SectionsRepository(BioRepositoryContext<Section> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
