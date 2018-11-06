using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Users;
using BioEngine.Core.Validation;

namespace BioEngine.Core.Repository
{
    public abstract class ContentItemRepository<T, TId> : SectionEntityRepository<T, TId>
        where T : ContentItem, IEntity<TId>, ISiteEntity<TId>, ISectionEntity<TId>
    {
        private readonly IUserDataProvider _userDataProvider;

        protected ContentItemRepository(BioRepositoryContext<T, TId> repositoryContext,
            SectionsRepository sectionsRepository, IUserDataProvider userDataProvider = null) : base(repositoryContext,
            sectionsRepository)
        {
            _userDataProvider = userDataProvider;
        }

        protected override void RegisterValidators()
        {
            base.RegisterValidators();
            Validators.Add(new ContentItemValidator<T>());
        }

        protected override async Task AfterLoadAsync(T[] entities)
        {
            if (_userDataProvider != null)
            {
                var entitiesArray = entities as T[] ?? entities.ToArray();
                await base.AfterLoadAsync(entitiesArray);
                var userIds = entitiesArray.Select(e => e.AuthorId).Distinct().ToArray();
                var data = await _userDataProvider.GetDataAsync(userIds);
                foreach (var entity in entitiesArray)
                {
                    entity.Author = data.FirstOrDefault(d => d.Id == entity.AuthorId);
                }
            }
        }
    }
}