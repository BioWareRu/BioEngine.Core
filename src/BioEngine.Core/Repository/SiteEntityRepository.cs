using BioEngine.Core.Abstractions;
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
    }
}
