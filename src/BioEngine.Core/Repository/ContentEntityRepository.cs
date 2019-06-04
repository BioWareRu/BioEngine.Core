using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Repository
{
    public abstract class ContentEntityRepository<TEntity> : SiteEntityRepository<TEntity>,
        IContentEntityRepository<TEntity>
        where TEntity : class, IContentEntity
    {
        protected ContentEntityRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }

        public virtual async Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = true;
            item.DatePublished = DateTimeOffset.UtcNow;
            await UpdateAsync(item, operationContext);
        }

        public virtual async Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = false;
            item.DatePublished = null;
            await UpdateAsync(item, operationContext);
        }
    }
}
