using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Interfaces
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity> : IBioRepository where TEntity : class, IEntity
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(QueryContext<TEntity> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<int> CountAsync(QueryContext<TEntity> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<TEntity> GetByIdAsync(Guid id, QueryContext<TEntity> queryContext = null);

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids, QueryContext<TEntity> queryContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteAsync(TEntity item);

        void BeginBatch();
        Task FinishBatchAsync();

        Task PublishAsync(TEntity item);
        
        Task UnPublishAsync(TEntity item);

        PropertyChange[] GetChanges(TEntity item);
    }
}
