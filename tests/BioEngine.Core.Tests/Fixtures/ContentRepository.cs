using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class ContentRepository : SectionEntityRepository<TestContent, int>
    {
        public ContentRepository(BioRepositoryContext<TestContent, int> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }
    }
}