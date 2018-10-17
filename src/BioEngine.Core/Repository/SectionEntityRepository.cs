using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Validation;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public abstract class SectionEntityRepository<T, TId> : SiteEntityRepository<T, TId>
        where T : class, IEntity<TId>, ISiteEntity<TId>, ISectionEntity<TId>
    {
        private readonly SectionsRepository _sectionsRepository;

        protected SectionEntityRepository(BioRepositoryContext<T, TId> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext)
        {
            _sectionsRepository = sectionsRepository;
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SectionEntityValidator<T, TId>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SectionId).HasValue)
            {
                query = query.Where(e => e.SectionIds.Contains(queryContext.SectionId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }

        protected override async Task<bool> BeforeValidateAsync(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null)
        {
            var result = await base.BeforeValidateAsync(item, validationResult, changes);

            if (!result) return false;

            if (item.SectionIds != null && item.SectionIds.Any())
            {
                var sections = (await _sectionsRepository.GetByIdsAsync(item.SectionIds)).ToArray();

                if (sections.Any())
                {
                    item.SiteIds = sections.SelectMany(s => s.SiteIds).Distinct().ToArray();
                    if (item.SiteIds.Any())
                    {
                        return true;
                    }

                    validationResult.errors.Add(new ValidationFailure(nameof(ISiteEntity<TId>.SiteIds),
                        "Не найдены сайты"));
                }
                else
                {
                    validationResult.errors.Add(
                        new ValidationFailure(nameof(ISiteEntity<TId>.SiteIds), "Не найдены разделы"));
                }
            }
            else
            {
                validationResult.errors.Add(new ValidationFailure(nameof(ISectionEntity<TId>.SectionIds),
                    "Не указаны разделы"));
            }

            return false;
        }
    }
}