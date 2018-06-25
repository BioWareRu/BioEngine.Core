using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public abstract class SectionValidator<T> : AbstractValidator<T> where T : ISiteEntity
    {
        protected SectionValidator()
        {
            RuleFor(e => e.SiteIds).NotEmpty();
        }
    }
}