using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Posts.Db
{
    public class UpdateSiteIdsHook<TUserPk> : BaseRepositoryHook
    {
        private readonly BioContext _dbContext;

        public UpdateSiteIdsHook(BioContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(Section).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[] changes = null,
            IBioRepositoryOperationContext operationContext = null)
        {
            var res = await base.AfterSaveAsync(item, changes, operationContext);
            if (res && item is Section && changes != null && changes.Any(c => c.Name == nameof(Section.SiteIds)))
            {
                var hasChanges = false;
                var contentItems = await _dbContext.Set<Post<TUserPk>>().Where(p => p.SectionIds.Contains(item.Id))
                    .ToArrayAsync();
                foreach (var contentItem in contentItems)
                {
                    var sections = await _dbContext.Sections.Where(s => contentItem.SectionIds.Contains(s.Id))
                        .ToArrayAsync();
                    contentItem.SiteIds = sections.SelectMany(s => s.SiteIds).Distinct().ToArray();
                    if (contentItem.SiteIds.Any())
                    {
                        _dbContext.Update(contentItem);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await _dbContext.SaveChangesAsync();
                }
            }

            return res;
        }
    }
}
