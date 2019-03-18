using System;
using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Validation
{
    public class PostValidator<T> : AbstractValidator<T> where T : Post
    {
        public PostValidator(BioContext dbContext)
        {
            RuleFor(e => e.Title).NotEmpty();
            RuleFor(e => e.Url).NotEmpty();
            RuleFor(e => e.Url).CustomAsync(async (url, context, _) =>
            {
                if (context.InstanceToValidate is Post post && post.Id != Guid.Empty)
                {
                    var count = await dbContext.Set<Post>().Where(p => p.Url == url && p.Id != post.Id).CountAsync();
                    if (count > 0)
                    {
                        context.AddFailure(
                            $"Url {url} already taken");
                    }
                }
            });
            RuleFor(e => e.AuthorId).NotEmpty();
            RuleFor(e => e.Blocks).NotEmpty();
        }
    }
}
