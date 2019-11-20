using System;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Abstractions
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity> : IBioRepository where TEntity : class, IEntity
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync();
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(Func<BioQuery<TEntity>, Task> configureQuery);
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(Action<BioQuery<TEntity>> configureQuery);

        Task<int> CountAsync();
        Task<int> CountAsync(Func<BioQuery<TEntity>, Task> configureQuery);
        Task<int> CountAsync(Action<BioQuery<TEntity>> configureQuery);

        Task<TEntity?> GetByIdAsync(Guid id, Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity?> GetByIdAsync(Guid id, Action<BioQuery<TEntity>> configureQuery);
        Task<TEntity?> GetByIdAsync(Guid id);

        Task<TEntity?> GetAsync(Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity?> GetAsync(Action<BioQuery<TEntity>> configureQuery);
        Task<TEntity?> GetAsync();

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids, Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity[]> GetByIdsAsync(Guid[] ids, Action<BioQuery<TEntity>> configureQuery);
        Task<TEntity[]> GetByIdsAsync(Guid[] ids);

        Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<TEntity?> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null);
        Task<TEntity> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        void BeginBatch();
        Task FinishBatchAsync();


        PropertyChange[] GetChanges(TEntity item, TEntity oldEntity);

        DbSet<T> Set<T>() where T : class;
    }
}
