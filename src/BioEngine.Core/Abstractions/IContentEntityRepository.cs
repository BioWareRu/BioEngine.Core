using System;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Abstractions
{
    public interface IContentEntityRepository<TEntity> : IBioRepository<TEntity>
        where TEntity : class, IEntity, IContentEntity
    {
        Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);


        Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync();

        Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync(
            Func<BioQuery<TEntity>, Task> configureQuery);

        Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync(
            Action<BioQuery<TEntity>> configureQuery);

        Task<TEntity> GetWithBlocksAsync(Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity> GetWithBlocksAsync(Action<BioQuery<TEntity>> configureQuery);
        Task<TEntity> GetWithBlocksAsync();
        Task<TEntity> GetByIdWithBlocksAsync(Guid id);
        Task<TEntity> GetByIdWithBlocksAsync(Guid id, Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity> GetByIdWithBlocksAsync(Guid id, Action<BioQuery<TEntity>> configureQuery);
        Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids);
        Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids, Func<BioQuery<TEntity>, Task> configureQuery);
        Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids, Action<BioQuery<TEntity>> configureQuery);

        Task<AddOrUpdateOperationResult<TEntity>> AddWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null);
    }
}
