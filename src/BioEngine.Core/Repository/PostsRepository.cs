using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class PostsRepository : ContentItemRepository<Post>
    {
        public PostsRepository(BioRepositoryContext<Post> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository, userDataProvider)
        {
        }

        protected override IQueryable<Post> ApplyContext(IQueryable<Post> query,
            QueryContext<Post> queryContext)
        {
            if ((queryContext?.TagId).HasValue)
            {
                query = query.Where(e => e.TagIds.Contains(queryContext.TagId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }

        protected override async Task<bool> AfterSaveAsync(Post item, PropertyChange[] changes = null,
            Post oldItem = null, IBioRepositoryOperationContext operationContext = null)
        {
            if (oldItem != null && changes != null && changes.Any())
            {
                var version = new PostVersion
                {
                    Id = Guid.NewGuid(),
                    PostId = oldItem.Id,
                    IsPublished = true,
                    DatePublished = DateTimeOffset.UtcNow,
                    Data = JsonConvert.SerializeObject(oldItem,
                        new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore})
                };
                if (operationContext?.User != null)
                {
                    version.ChangeAuthorId = operationContext.User.Id;
                }

                DbContext.Add(version);
                await DbContext.SaveChangesAsync();
            }

            return await base.AfterSaveAsync(item, changes, oldItem);
        }
    }
}
