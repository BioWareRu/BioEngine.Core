using System;
using System.Threading.Tasks;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Abstractions
{
    public interface IContentEntityRepository<TEntity> : IBioRepository<TEntity>
        where TEntity : class, IEntity, IContentEntity
    {
        Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync(
            Action<BioRepositoryQuery<TEntity>> configureQuery = null);

        Task<TEntity> GetWithBlocksAsync(Action<BioRepositoryQuery<TEntity>> configureQuery = null);
        Task<TEntity> GetByIdWithBlocksAsync(Guid id, Action<BioRepositoryQuery<TEntity>> configureQuery = null);
        Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids, Action<BioRepositoryQuery<TEntity>> configureQuery = null);

        Task<AddOrUpdateOperationResult<TEntity>> AddWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext operationContext = null);

        Task<AddOrUpdateOperationResult<TEntity>> UpdateWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext operationContext = null);
    }
}
