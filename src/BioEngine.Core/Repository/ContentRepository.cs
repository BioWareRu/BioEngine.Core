using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class ContentRepository : SectionEntityRepository<ContentItem, int>
    {
        public ContentRepository(BioRepositoryContext<ContentItem, int> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext,
            sectionsRepository)
        {
        }

        protected override IQueryable<ContentItem> ApplyContext(IQueryable<ContentItem> query,
            QueryContext<ContentItem, int> queryContext)
        {
            if ((queryContext?.TagId).HasValue)
            {
                query = query.Where(e => e.TagIds.Contains(queryContext.TagId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }
    }
}