using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class ContentRepository : ContentItemRepository<Post>
    {
        public ContentRepository(BioRepositoryContext<Post> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository, userDataProvider)
        {
        }

        protected override IQueryable<Post> ApplyContext(IQueryable<Post> query,
            QueryContext<Post> queryContext)
        {
            if ((queryContext?.TagId).HasValue)
            {
                query = query.Where(e => e.TagIds.Contains(queryContext.TagId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }
    }
}
