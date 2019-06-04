using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SectionRepository<Section>
    {
        public SectionsRepository(BioRepositoryContext<Section> repositoryContext) : base(repositoryContext)
        {
        }
    }

    public abstract class SectionRepository<T> : ContentEntityRepository<T> where T : Section
    {
        protected SectionRepository(BioRepositoryContext<T> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<T> GetBaseQuery(ContentEntityQueryContext<T>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<T>().Include(p => p.Blocks), queryContext);
        }

        protected override async Task<bool> AfterSaveAsync(T item, PropertyChange[] changes = null,
            T oldItem = null,
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
