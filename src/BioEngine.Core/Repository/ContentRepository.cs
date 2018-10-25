using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class ContentRepository : ContentItemRepository<ContentItem, int>
    {
        public ContentRepository(BioRepositoryContext<ContentItem, int> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository, userDataProvider)
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