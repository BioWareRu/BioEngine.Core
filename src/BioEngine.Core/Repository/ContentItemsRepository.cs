using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Repository
{
    public class ContentItemsRepository : SectionEntityRepository<IContentItem>
    {
        public ContentItemsRepository(BioRepositoryContext<IContentItem> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }
    }
}
