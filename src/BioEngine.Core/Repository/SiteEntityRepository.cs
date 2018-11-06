using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public override async Task<(T[] items, int itemsCount)> GetAllAsync(QueryContext<T, TId> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
        {
            var entities = await base.GetAllAsync(queryContext, addConditionsCallback);

            return entities;
        }
    }
}