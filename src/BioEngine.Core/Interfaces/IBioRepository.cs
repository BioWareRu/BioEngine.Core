using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Interfaces
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity, TPk> : IBioRepository where TEntity : class, IEntity<TPk>
    {
        Task<(List<TEntity> items, int itemsCount)> GetAllAsync(QueryContext<TEntity, TPk> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<int> CountAsync(QueryContext<TEntity, TPk> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<TEntity> GetByIdAsync(TPk id, QueryContext<TEntity, TPk> queryContext = null);

        Task<TEntity> NewAsync();

        Task<IEnumerable<TEntity>> GetByIdsAsync(TPk[] ids, QueryContext<TEntity, TPk> queryContext = null);

        Task<AddOrUpdateOperationResult<TEntity, TPk>> AddAsync(TEntity item);

        Task<AddOrUpdateOperationResult<TEntity, TPk>> UpdateAsync(TEntity item);

        Task<bool> DeleteAsync(TPk id);

        Task PublishAsync(TEntity item);
        
        Task UnPublishAsync(TEntity item);

        PropertyChange[] GetChanges(TEntity item);
    }
}