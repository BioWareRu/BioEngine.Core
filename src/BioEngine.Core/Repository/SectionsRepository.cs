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

    public abstract class SectionRepository<T> : SiteEntityRepository<T> where T : Section
    {
        protected SectionRepository(BioRepositoryContext<T> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<T> GetBaseQuery(QueryContext<T>? queryContext = null)
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
                var posts = await DbContext.Posts.Where(p => p.SectionIds.Contains(item.Id)).ToArrayAsync();
                foreach (var post in posts)
                {
                    var sections = await DbContext.Sections.Where(s => post.SectionIds.Contains(s.Id)).ToArrayAsync();
                    post.SiteIds = sections.SelectMany(s => s.SiteIds).Distinct().ToArray();
                    if (post.SiteIds.Any())
                    {
                        DbContext.Update(post);
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
