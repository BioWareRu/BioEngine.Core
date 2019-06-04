using System;
using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<T, TQueryContext> : BioRepository<T, TQueryContext>
        where T : class, ISiteEntity where TQueryContext : QueryContext<T>
    {
        protected SiteEntityRepository(BioRepositoryContext<T> repositoryContext) : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SiteEntityValidator<T>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, TQueryContext? queryContext)
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
