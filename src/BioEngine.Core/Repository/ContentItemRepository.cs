using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class ContentItemRepository<T, TId> : SectionEntityRepository<T, TId>
        where T : ContentItem, IEntity<TId>, ISiteEntity<TId>, ISectionEntity<TId>
    {
        protected ContentItemRepository(BioRepositoryContext<T, TId> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new ContentItemValidator<T>());
        }
    }
}