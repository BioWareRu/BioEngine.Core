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

    public interface IBioRepository<TEntity> : IBioRepository where TEntity : class, IEntity
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(QueryContext<TEntity>? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null);

        Task<int> CountAsync(QueryContext<TEntity>? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null);

        Task<TEntity> GetByIdAsync(Guid id, QueryContext<TEntity>? queryContext = null);
        Task<TEntity> GetAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> where, QueryContext<TEntity>? queryContext = null);

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids, QueryContext<TEntity>? queryContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<bool> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null);
        Task<bool> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        void BeginBatch();
        Task FinishBatchAsync();

        Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        PropertyChange[] GetChanges(TEntity item, TEntity oldEntity);
    }
}
