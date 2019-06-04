using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public abstract class SectionRepository<TEntity> : ContentEntityRepository<TEntity> where TEntity : Section
    {
        protected SectionRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<TEntity> GetBaseQuery(QueryContext<TEntity>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<TEntity>().Include(p => p.Blocks), queryContext);
        }

        protected override async Task<bool> AfterSaveAsync(TEntity item, PropertyChange[] changes = null,
            TEntity oldItem = null,
            IBioRepositoryOperationContext operationContext = null)
        {
            var res = await base.AfterSaveAsync(item, changes, oldItem, operationContext);
            if (res && changes != null && changes.Any(c => c.Name == nameof(Section.SiteIds)))
            {
                var hasChanges = false;
                var contentItems = await DbContext.ContentItems.Where(p => p.SectionIds.Contains(item.Id)).ToArrayAsync();
                foreach (var contentItem in contentItems)
                {
                    var sections = await DbContext.Sections.Where(s => contentItem.SectionIds.Contains(s.Id)).ToArrayAsync();
                    contentItem.SiteIds = sections.SelectMany(s => s.SiteIds).Distinct().ToArray();
                    if (contentItem.SiteIds.Any())
                    {
                        DbContext.Update(contentItem);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await DbContext.SaveChangesAsync();
                }
            }

            return res;
        }
    }
}
