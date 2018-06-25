using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class ContentRepository : BioRepository<ContentItem, int>
    {
        public ContentRepository(BioRepositoryContext<ContentItem, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}