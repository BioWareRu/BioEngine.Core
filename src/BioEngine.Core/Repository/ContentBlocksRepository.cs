using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class ContentBlocksRepository : BioRepository<ContentBlock>
    {
        public ContentBlocksRepository(BioRepositoryContext<ContentBlock> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
