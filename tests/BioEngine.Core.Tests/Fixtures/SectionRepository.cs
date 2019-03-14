using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class SectionRepository : SiteEntityRepository<TestSection>
    {
        public SectionRepository(BioRepositoryContext<TestSection> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
