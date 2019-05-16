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
    public abstract class SectionEntityRepository<T> : SiteEntityRepository<T>
        where T : class, IEntity, ISiteEntity, ISectionEntity
    {
        protected readonly SectionsRepository SectionsRepository;

        protected SectionEntityRepository(BioRepositoryContext<T> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext)
        {
            SectionsRepository = sectionsRepository;
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new SectionEntityValidator<T>());
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T>? queryContext)
        {
            if (queryContext != null && queryContext.SectionId != Guid.Empty)
            {
                var sectionId = queryContext.SectionId;
                query = query.Where(e => e.SectionIds.Contains(sectionId));
            }

            return base.ApplyContext(query, queryContext);
        }

        protected override async Task<bool> BeforeValidateAsync(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
        {
            var result = await base.BeforeValidateAsync(item, validationResult, changes, operationContext);

            if (!result)
                return false;

            if (item.SectionIds != null && item.SectionIds.Any())
            {
                var sections = (await SectionsRepository.GetByIdsAsync(item.SectionIds)).ToArray();

                if (sections.Any())
                {
                    item.SiteIds = sections.SelectMany(s => s.SiteIds).Distinct().ToArray();
                    if (item.SiteIds.Any())
                    {
                        item.MainSiteId = sections.First(s => s.Id == item.SectionIds.First()).MainSiteId;
                        return true;
                    }

                    validationResult.errors.Add(new ValidationFailure(nameof(ISiteEntity.SiteIds),
                        "Не найдены сайты"));
                }
                else
                {
                    validationResult.errors.Add(
                        new ValidationFailure(nameof(ISiteEntity.SiteIds), "Не найдены разделы"));
                }
            }
            else
            {
                validationResult.errors.Add(new ValidationFailure(nameof(ISectionEntity.SectionIds),
                    "Не указаны разделы"));
            }

            return false;
        }
    }
}
