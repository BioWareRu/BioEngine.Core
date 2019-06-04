using BioEngine.Core.Abstractions;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public sealed class SectionEntityValidator<T> : AbstractValidator<T>
        where T : ISiteEntity, ISectionEntity
    {
        public SectionEntityValidator()
        {
            RuleFor(e => e.SectionIds).NotEmpty();
        }
    }
}
