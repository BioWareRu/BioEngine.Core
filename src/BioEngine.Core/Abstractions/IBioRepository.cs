﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Abstractions
{
    public interface IBioRepository
    {
    }

    public interface IBioRepository<TEntity> : IBioRepository where TEntity : class, IEntity
    {
        Task<(TEntity[] items, int itemsCount)> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);

        Task<int> CountAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);

        Task<TEntity> GetByIdAsync(Guid id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);

        Task<TEntity> GetAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> configureQuery);

        Task<TEntity> NewAsync();

        Task<TEntity[]> GetByIdsAsync(Guid[] ids,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);

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
}