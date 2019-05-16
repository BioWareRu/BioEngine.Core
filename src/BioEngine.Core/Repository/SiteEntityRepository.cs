using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Validation;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<T> : BioRepository<T>
        where T : class, IEntity, ISiteEntity
    {
        protected SiteEntityRepository(BioRepositoryContext<T> repositoryContext)
            : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SiteEntityValidator<T>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T>? queryContext)
        {
            if (queryContext != null && queryContext.SiteId != Guid.Empty)
            {
                var siteId = queryContext.SiteId;
                query = query.Where(e => e.SiteIds.Contains(siteId));
            }

            return base.ApplyContext(query, queryContext);
        }

        protected override Task<bool> BeforeValidateAsync(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult, PropertyChange[] changes = null,
            IBioRepositoryOperationContext operationContext = null)
        {
            if (item.SiteIds.Any())
            {
                item.MainSiteId = item.SiteIds.First();
            }

            return base.BeforeValidateAsync(item, validationResult, changes, operationContext);
        }
    }

    public abstract class SingleSiteEntityRepository<T> : BioRepository<T>
        where T : class, IEntity, ISingleSiteEntity
    {
        protected SingleSiteEntityRepository(BioRepositoryContext<T> repositoryContext)
            : base(repositoryContext)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SingleSiteEntityValidator<T>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T>? queryContext)
        {
            if (queryContext != null && queryContext.SiteId != Guid.Empty)
            {
                var siteId = queryContext.SiteId;
                query = query.Where(e => e.SiteId == siteId);
            }

            return base.ApplyContext(query, queryContext);
        }
    }
}
