using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class ContentItemsRepository : SectionEntityRepository<ContentItem>
    {
        public ContentItemsRepository(BioRepositoryContext<ContentItem> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }
    }
}
