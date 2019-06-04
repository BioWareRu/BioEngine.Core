using System.Threading.Tasks;

namespace BioEngine.Core.Abstractions
{
    public interface IContentEntityRepository<TEntity> : IBioRepository<TEntity>
        where TEntity : class, IEntity, IContentEntity
    {
        Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);

        Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);
    }
}
