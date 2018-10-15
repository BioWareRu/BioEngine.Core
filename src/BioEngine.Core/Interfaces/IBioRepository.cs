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
        Task<(List<TEntity> items, int itemsCount)> GetAll(QueryContext<TEntity, TPk> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<int> Count(QueryContext<TEntity, TPk> queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> addConditionsCallback = null);

        Task<TEntity> GetById(TPk id, QueryContext<TEntity, TPk> queryContext = null);

        Task<TEntity> New();

        Task<IEnumerable<TEntity>> GetByIds(TPk[] ids, QueryContext<TEntity, TPk> queryContext = null);

        Task<AddOrUpdateOperationResult<TEntity, TPk>> Add(TEntity item);

        Task<AddOrUpdateOperationResult<TEntity, TPk>> Update(TEntity item);

        Task<bool> Delete(TPk id);

        Task Publish(TEntity item);
        
        Task UnPublish(TEntity item);

        PropertyChange[] GetChanges(TEntity item);
    }
}