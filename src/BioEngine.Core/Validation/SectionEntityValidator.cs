using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public sealed class SectionEntityValidator<T, TId> : AbstractValidator<T>
        where T : ISiteEntity<TId>, ISectionEntity<TId>
    {
        public SectionEntityValidator()
        {
            RuleFor(e => e.SectionIds).NotEmpty();
        }
    }
}