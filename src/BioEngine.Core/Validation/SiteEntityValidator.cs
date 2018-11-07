using BioEngine.Core.Interfaces;
using FluentValidation;
using JetBrains.Annotations;

namespace BioEngine.Core.Validation
{
    [UsedImplicitly]
    internal class SiteEntityValidator<T, TId> : AbstractValidator<T> where T : ISiteEntity<TId>
    {
        public SiteEntityValidator()
        {
            RuleFor(e => e.SiteIds).NotEmpty();
        }
    }
    
    [UsedImplicitly]
    internal class SingleSiteEntityValidator<T, TId> : AbstractValidator<T> where T : ISingleSiteEntity<TId>
    {
        public SingleSiteEntityValidator()
        {
            RuleFor(e => e.SiteId).NotEmpty().GreaterThan(0);
        }
    }
}