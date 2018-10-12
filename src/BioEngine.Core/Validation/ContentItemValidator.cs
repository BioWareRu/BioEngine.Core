using BioEngine.Core.Entities;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class ContentItemValidator<T> : AbstractValidator<T> where T : ContentItem
    {
        public ContentItemValidator()
        {
            RuleFor(e => e.Title).NotEmpty();
            RuleFor(e => e.Url).NotEmpty();
            RuleFor(e => e.AuthorId).NotEmpty();
        }
    }
}