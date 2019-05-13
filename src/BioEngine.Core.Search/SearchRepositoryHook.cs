using System;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Search
{
    public class SearchRepositoryHook<TEntity> : BaseRepositoryHook where TEntity : BaseEntity
    {
        private readonly ISearchProvider<TEntity> _searchProvider;

        public SearchRepositoryHook(ISearchProvider<TEntity> searchProvider)
        {
            _searchProvider = searchProvider;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(TEntity).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            if (item is TEntity entity)
            {
                if (entity.IsPublished)
                {
                    await _searchProvider.AddOrUpdateEntityAsync(entity);
                }
                else
                {
                    await _searchProvider.DeleteEntityAsync(entity);
                }

                return true;
            }

            return false;
        }
    }
}
