using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestContentRepository : ContentItemRepository<TestContent>
    {
        public TestContentRepository(BioRepositoryContext<TestContent> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext,
            sectionsRepository)
        {
        }
    }
}
