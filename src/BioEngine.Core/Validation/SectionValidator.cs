using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public abstract class SectionValidator<T, TId> : AbstractValidator<T> where T : ISiteEntity<TId>
    {
        protected SectionValidator()
        {
            RuleFor(e => e.SiteIds).NotEmpty();
        }
    }
}