using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestSectionRepository : ContentEntityRepository<TestSection>
    {
        public TestSectionRepository(BioRepositoryContext<TestSection> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
