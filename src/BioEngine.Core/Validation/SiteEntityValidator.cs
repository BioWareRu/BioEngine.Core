using BioEngine.Core.Abstractions;
using FluentValidation;
using JetBrains.Annotations;

namespace BioEngine.Core.Validation
{
    [UsedImplicitly]
    internal class SiteEntityValidator<T> : AbstractValidator<T> where T : ISiteEntity
    {
        public SiteEntityValidator()
        {
            RuleFor(e => e.SiteIds).NotEmpty();
        }
    }
}
