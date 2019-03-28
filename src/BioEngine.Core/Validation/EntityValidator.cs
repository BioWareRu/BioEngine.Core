using BioEngine.Core.Entities;
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
