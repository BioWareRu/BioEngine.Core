using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class SectionRepository : SiteEntityRepository<TestSection, int>
    {
        public SectionRepository(BioRepositoryContext<TestSection, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}