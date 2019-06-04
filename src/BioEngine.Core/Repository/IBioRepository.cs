using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity, TQueryContext> : IBioRepository where TEntity : class, IEntity
        where TQueryContext : QueryContext<TEntity>
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(TQueryContext? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null);

        Task<int> CountAsync(TQueryContext? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null);

        Task<TEntity> GetByIdAsync(Guid id, TQueryContext? queryContext = null);

        Task<TEntity> GetAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> where,
            TQueryContext? queryContext = null);

        Task<TEntity[]> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> where,
            TQueryContext? queryContext = null);

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids, TQueryContext? queryContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<TEntity> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null);
        Task<TEntity> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        void BeginBatch();
        Task FinishBatchAsync();


        PropertyChange[] GetChanges(TEntity item, TEntity oldEntity);
    }

    public interface IContentEntityRepository<TEntity, TQueryContext> : IBioRepository<TEntity, TQueryContext>
        where TEntity : class, IEntity, IContentEntity where TQueryContext : ContentEntityQueryContext<TEntity>
    {
        Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);
    }
}
