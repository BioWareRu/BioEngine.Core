using System;
using System.Linq;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<TEntity> : BioRepository<TEntity>
        where TEntity : class, ISiteEntity
    {
        protected SiteEntityRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SiteEntityValidator<TEntity>());
        }

        protected override IQueryable<TEntity> ApplyContext(IQueryable<TEntity> query, QueryContext<TEntity>? queryContext)
        {
            if (queryContext != null && queryContext.SiteId != Guid.Empty)
            {
                var siteId = queryContext.SiteId;
                query = query.Where(e => e.SiteIds.Contains(siteId));
            }

            return base.ApplyContext(query, queryContext);
        }
    }
}
