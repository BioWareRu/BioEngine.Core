using BioEngine.Core.Entities;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class SiteValidator : AbstractValidator<Site>
    {
        public SiteValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(1024).MinimumLength(5)
                .WithMessage("Название сайте должно быть от 5 до 1024 символов.");
        }
    }
}