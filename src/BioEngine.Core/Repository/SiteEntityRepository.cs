using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Validation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<TEntity> : BioRepository<TEntity>
        where TEntity : class, ISiteEntity
    {
        protected SiteEntityRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }

        protected override async Task<bool> BeforeValidateAsync(TEntity item,
            (bool isValid, IList<ValidationFailure> errors) validationResult, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            if (item.SiteIds.Length == 0)
            {
                var sites = await DbContext.Sites.ToListAsync();
                if (sites.Count == 1)
                {
                    item.SiteIds = new[] {sites.First().Id};
                }
            }

            return await base.BeforeValidateAsync(item, validationResult, changes, operationContext);
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SiteEntityValidator<TEntity>());
        }
    }
}
