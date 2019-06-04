using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class ContentBlocksRepository : BioRepository<ContentBlock, QueryContext<ContentBlock>>
    {
        public ContentBlocksRepository(BioRepositoryContext<ContentBlock> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
