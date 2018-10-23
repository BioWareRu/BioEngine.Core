using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class MenuRepository : SiteEntityRepository<Menu, int>
    {
        public MenuRepository(BioRepositoryContext<Menu, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}