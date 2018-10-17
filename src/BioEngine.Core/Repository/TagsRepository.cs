using System.Threading.Tasks;
using BioEngine.Core.Entities;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class TagsRepository : BioRepository<Tag, int>
    {
        public TagsRepository(BioRepositoryContext<Tag, int> repositoryContext) : base(repositoryContext)
        {
        }

        public override async Task<AddOrUpdateOperationResult<Tag, int>> AddAsync(Tag item)
        {
            var existingTag = await DbContext.Tags.FirstOrDefaultAsync(t => t.Name == item.Name);
            if (existingTag != null)
            {
                return new AddOrUpdateOperationResult<Tag, int>(existingTag, new ValidationFailure[0]);
            }

            return await base.AddAsync(item);
        }
    }
}