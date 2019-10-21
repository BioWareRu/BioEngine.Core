using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class ContentItemRepository<TEntity> : SectionEntityRepository<TEntity>
        where TEntity : BaseEntity, IContentItem, IEntity, ISiteEntity, ISectionEntity
    {
        protected ContentItemRepository(BioRepositoryContext<TEntity> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext,
            sectionsRepository)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new ContentItemValidator<TEntity>(DbContext));
        }
    }
}
