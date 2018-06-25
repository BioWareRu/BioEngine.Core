using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public abstract class ContentValidator<T> : SectionValidator<T> where T : ISiteEntity, ISectionEntity
    {
        protected ContentValidator() : base()
        {
            RuleFor(e => e.SectionIds).NotEmpty();
            RuleFor(e => e.SiteIds).NotEmpty();
        }
    }
}