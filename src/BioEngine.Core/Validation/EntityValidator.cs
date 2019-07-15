using BioEngine.Core.Abstractions;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class EntityValidator : AbstractValidator<IBioEntity>
    {
        public EntityValidator()
        {
            RuleFor(e => e.DateAdded).NotNull();
            RuleFor(e => e.DateUpdated).NotNull();
        }
    }
}
