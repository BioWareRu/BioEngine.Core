using BioEngine.Core.Entities;
using FluentValidation;

namespace BioEngine.Core.Validation
{
    public class PostValidator<T> : AbstractValidator<T> where T : Post
    {
        public PostValidator()
        {
            RuleFor(e => e.Title).NotEmpty();
            RuleFor(e => e.Url).NotEmpty();
            RuleFor(e => e.AuthorId).NotEmpty();
            RuleFor(e => e.Blocks).NotEmpty();
        }
    }
}