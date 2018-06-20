using BioEngine.Core.Entities;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class SiteValidator : AbstractValidator<Site>
    {
        public SiteValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MinimumLength(1024);
        }
    }
}