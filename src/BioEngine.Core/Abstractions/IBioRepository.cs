using System;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Abstractions
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity> : IBioRepository where TEntity : class, IEntity
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(Action<BioQuery<TEntity>>? configureQuery = null);

        Task<int> CountAsync(Action<BioQuery<TEntity>>? configureQuery = null);

        Task<TEntity?> GetByIdAsync(Guid id, Action<BioQuery<TEntity>>? configureQuery = null);

        Task<TEntity?> GetAsync(Action<BioQuery<TEntity>>? configureQuery = null);

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids,
            Action<BioQuery<TEntity>>? configureQuery = null);

        Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<TEntity?> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null);
        Task<TEntity> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        void BeginBatch();
        Task FinishBatchAsync();


        PropertyChange[] GetChanges(TEntity item, TEntity oldEntity);
    }
}
