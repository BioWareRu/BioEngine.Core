using BioEngine.Core.Repository;
using BioEngine.Core.Users;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestContentRepository : ContentItemRepository<TestContent>
    {
        public TestContentRepository(BioRepositoryContext<TestContent> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository, userDataProvider)
        {
        }
    }
}
