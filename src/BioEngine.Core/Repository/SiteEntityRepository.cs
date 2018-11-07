using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<T, TId> : BioRepository<T, TId>
        where T : class, IEntity<TId>, ISiteEntity<TId>
    {
        protected SiteEntityRepository(BioRepositoryContext<T, TId> repositoryContext)
            : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SiteEntityValidator<T, TId>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SiteId).HasValue)
            {
                query = query.Where(e => e.SiteIds.Contains(queryContext.SiteId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }
    }

    public abstract class SingleSiteEntityRepository<T, TId> : BioRepository<T, TId>
        where T : class, IEntity<TId>, ISingleSiteEntity<TId>
    {
        protected SingleSiteEntityRepository(BioRepositoryContext<T, TId> repositoryContext)
            : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SingleSiteEntityValidator<T, TId>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SiteId).HasValue)
            {
                query = query.Where(e => e.SiteId == queryContext.SiteId.Value);
            }

            return base.ApplyContext(query, queryContext);
        }
    }
}