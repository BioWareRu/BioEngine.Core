using BioEngine.Core.Pages.Entities;
using BioEngine.Core.Repository;
using JetBrains.Annotations;

namespace BioEngine.Core.Pages.Db
{
    [UsedImplicitly]
    public class PagesRepository : ContentEntityRepository<Page>
    {
        public PagesRepository(BioRepositoryContext<Page> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
