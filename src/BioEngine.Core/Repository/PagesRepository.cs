using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class PagesRepository : SiteEntityRepository<Page, int>
    {
        public PagesRepository(BioRepositoryContext<Page, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}