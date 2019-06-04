using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using BioEngine.Core.Validation;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class ContentItemsRepository : SectionEntityRepository<ContentItem>
    {
        public ContentItemsRepository(BioRepositoryContext<ContentItem> repositoryContext,
            SectionsRepository sectionsRepository) : base(repositoryContext, sectionsRepository)
        {
        }
    }

    public abstract class ContentItemRepository<T> : SectionEntityRepository<T>
        where T : ContentItem, IEntity, ISiteEntity, ISectionEntity
    {
        private readonly IUserDataProvider? _userDataProvider;

        protected ContentItemRepository(BioRepositoryContext<T> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider? userDataProvider = null) : base(repositoryContext,
            sectionsRepository)
        {
            _userDataProvider = userDataProvider;
        }

        protected override IQueryable<T> GetBaseQuery(ContentEntityQueryContext<T>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<T>().Include(p => p.Blocks), queryContext);
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new ContentItemValidator<T>(DbContext));
        }

        protected override async Task AfterLoadAsync(T[] entities)
        {
            if (_userDataProvider != null && entities != null && entities.Length > 0)
            {
                await base.AfterLoadAsync(entities);
                var userIds = entities.Select(e => e.AuthorId).Distinct().ToArray();
                var data = await _userDataProvider.GetDataAsync(userIds);
                foreach (var entity in entities)
                {
                    entity.Author = data.FirstOrDefault(d => d.Id == entity.AuthorId);
                }
            }
        }
    }
}
