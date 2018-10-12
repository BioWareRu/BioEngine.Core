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
}