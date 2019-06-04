using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class TagsRepository : BioRepository<Tag>
    {
        public TagsRepository(BioRepositoryContext<Tag> repositoryContext) : base(repositoryContext)
        {
        }

        public override async Task<AddOrUpdateOperationResult<Tag>> AddAsync(Tag item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var existingTag = await DbContext.Tags.FirstOrDefaultAsync(t => t.Title == item.Title);
            if (existingTag != null)
            {
                return new AddOrUpdateOperationResult<Tag>(existingTag, new ValidationFailure[0],
                    new PropertyChange[0]);
            }

            return await base.AddAsync(item, operationContext);
        }
    }
}
