using System;
using BioEngine.Core.Interfaces;
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

    [UsedImplicitly]
    internal class SingleSiteEntityValidator<T> : AbstractValidator<T> where T : ISingleSiteEntity
    {
        public SingleSiteEntityValidator()
        {
            RuleFor(e => e.SiteId).NotEmpty().NotEqual(Guid.Empty);
        }
    }
}
