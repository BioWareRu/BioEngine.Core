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
        
        protected override IQueryable<ContentItem> AddIncludes(IQueryable<ContentItem> query)
        {
            return base.AddIncludes(query).Include(p=>p.Blocks);
        }
    }
}
