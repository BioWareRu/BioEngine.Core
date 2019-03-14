using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class PagesRepository : SiteEntityRepository<Page>
    {
        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
