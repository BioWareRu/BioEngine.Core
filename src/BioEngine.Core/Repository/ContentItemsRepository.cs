using System.Linq;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class ContentItemsRepository : SectionEntityRepository<ContentItem>
    {
        public ContentItemsRepository(BioRepositoryContext<ContentItem> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }
        
        protected override IQueryable<ContentItem> GetBaseQuery()
        {
            return DbContext.Set<ContentItem>().Include(p => p.Blocks);
        }

    }
}
