using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public abstract class ContentValidator<T, TId> : SectionValidator<T, TId>
        where T : ISiteEntity<TId>, ISectionEntity<TId>
    {
        protected ContentValidator()
        {
            RuleFor(e => e.SectionIds).NotEmpty();
        }
    }
}