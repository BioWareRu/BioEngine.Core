using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class EntityValidator<TId> : AbstractValidator<IEntity<TId>>
    {
        public EntityValidator()
        {
            RuleFor(e => e.DateAdded).NotNull();
            RuleFor(e => e.DateUpdated).NotNull();
        }
    }
}