using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class EntityValidator : AbstractValidator<IEntity>
    {
        public EntityValidator()
        {
            RuleFor(e => e.DateAdded).NotNull();
            RuleFor(e => e.DateUpdated).NotNull();
        }
    }
}
