using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class SectionRepository : SiteEntityRepository<TestSection>
    {
        public SectionRepository(BioRepositoryContext<TestSection> repositoryContext,
            IMainSiteSelectionPolicy mainSiteSelectionPolicy) : base(repositoryContext, mainSiteSelectionPolicy)
        {
        }
    }
}
