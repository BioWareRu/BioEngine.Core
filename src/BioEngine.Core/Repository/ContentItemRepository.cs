using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Users;
using BioEngine.Core.Validation;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public abstract class ContentItemRepository<T, TId> : SectionEntityRepository<T, TId>
        where T : Post, IEntity<TId>, ISiteEntity<TId>, ISectionEntity<TId>
    {
        private readonly IUserDataProvider _userDataProvider;

        protected ContentItemRepository(BioRepositoryContext<T, TId> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository)
        {
            _userDataProvider = userDataProvider;
        }

        protected override IQueryable<T> GetBaseQuery(QueryContext<T, TId> queryContext = null)
        {
            return base.GetBaseQuery(queryContext).Include(c => c.Blocks);
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new ContentItemValidator<T>());
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