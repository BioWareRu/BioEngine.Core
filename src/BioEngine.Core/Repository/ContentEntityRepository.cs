using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public abstract class ContentEntityRepository<T> : SiteEntityRepository<T, ContentEntityQueryContext<T>>,
        IContentEntityRepository<T, ContentEntityQueryContext<T>>
        where T : class, IContentEntity
    {
        protected ContentEntityRepository(BioRepositoryContext<T> repositoryContext) : base(repositoryContext)
        {
        }

        public virtual async Task PublishAsync(T item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = true;
            item.DatePublished = DateTimeOffset.UtcNow;
            await UpdateAsync(item, operationContext);
        }

        public virtual async Task UnPublishAsync(T item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = false;
            item.DatePublished = null;
            await UpdateAsync(item, operationContext);
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, ContentEntityQueryContext<T>? queryContext)
        {
            if (queryContext == null)
            {
                return query;
            }

            if (!queryContext.IncludeUnpublished)
            {
                query = query.Where(x => x.IsPublished);
            }

            query = base.ApplyContext(query, queryContext);

            return query;
        }
    }
}
