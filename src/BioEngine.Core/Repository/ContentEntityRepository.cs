using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Helpers;

namespace BioEngine.Core.Repository
{
    public abstract class ContentEntityRepository<TEntity> : SiteEntityRepository<TEntity>,
        IContentEntityRepository<TEntity>
        where TEntity : class, IContentEntity
    {
        protected ContentEntityRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }

        public virtual async Task PublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = true;
            item.DatePublished = DateTimeOffset.UtcNow;
            await UpdateAsync(item, operationContext);
        }

        public virtual async Task UnPublishAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null)
        {
            item.IsPublished = false;
            item.DatePublished = null;
            await UpdateAsync(item, operationContext);
        }


        public async Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync()
        {
            var result = await base.GetAllAsync();
            await DoGetAllWithBlocksAsync(result.items);
            return result;
        }

        public async Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync(
            Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var result = await base.GetAllAsync(configureQuery);
            await DoGetAllWithBlocksAsync(result.items);
            return result;
        }

        public async Task<(TEntity[] items, int itemsCount)> GetAllWithBlocksAsync(
            Action<BioQuery<TEntity>> configureQuery)
        {
            var result = await base.GetAllAsync(configureQuery);
            await DoGetAllWithBlocksAsync(result.items);
            return result;
        }

        private async Task DoGetAllWithBlocksAsync(
            TEntity[] items)
        {
            var blocks = await BlocksHelper.GetBlocksAsync(items, DbContext);
            foreach (var item in items)
            {
                item.Blocks = blocks[item.Id];
            }
        }

        public async Task<TEntity?> GetWithBlocksAsync()
        {
            var entity = await base.GetAsync();
            return await DoGetWithBlocksAsync(entity);
        }

        public async Task<TEntity?> GetWithBlocksAsync(Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var entity = await base.GetAsync(configureQuery);
            return await DoGetWithBlocksAsync(entity);
        }

        public async Task<TEntity?> GetWithBlocksAsync(Action<BioQuery<TEntity>> configureQuery)
        {
            var entity = await base.GetAsync(configureQuery);
            return await DoGetWithBlocksAsync(entity);
        }

        private async Task<TEntity> DoGetWithBlocksAsync(TEntity entity)
        {
            if (entity != null)
            {
                entity.Blocks = await BlocksHelper.GetBlocksAsync(entity, DbContext);
            }

            return entity;
        }

        public async Task<TEntity?> GetByIdWithBlocksAsync(Guid id)
        {
            var entity = await base.GetByIdAsync(id);
            return await DoGetWithBlocksAsync(entity);
        }

        public async Task<TEntity?> GetByIdWithBlocksAsync(Guid id,
            Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var entity = await base.GetByIdAsync(id, configureQuery);
            return await DoGetWithBlocksAsync(entity);
        }

        public async Task<TEntity?> GetByIdWithBlocksAsync(Guid id,
            Action<BioQuery<TEntity>> configureQuery)
        {
            var entity = await base.GetByIdAsync(id, configureQuery);
            return await DoGetWithBlocksAsync(entity);
        }

        public async Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids)
        {
            var items = await base.GetByIdsAsync(ids);
            await DoGetAllWithBlocksAsync(items);
            return items;
        }

        public async Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids,
            Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var items = await base.GetByIdsAsync(ids, configureQuery);
            await DoGetAllWithBlocksAsync(items);
            return items;
        }

        public async Task<TEntity[]> GetByIdsWithBlocksAsync(Guid[] ids,
            Action<BioQuery<TEntity>> configureQuery)
        {
            var items = await base.GetByIdsAsync(ids, configureQuery);
            await DoGetAllWithBlocksAsync(items);
            return items;
        }

        public async Task<AddOrUpdateOperationResult<TEntity>> AddWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var result = await DoAddAsync(item, operationContext);
            if (result.isValid)
            {
                await BlocksHelper.AddBlocksAsync(item, DbContext);
                await DoSaveAsync(item, null, null, operationContext);
            }

            return new AddOrUpdateOperationResult<TEntity>(item, result.errors, new PropertyChange[0]);
        }

        public async Task<AddOrUpdateOperationResult<TEntity>> UpdateWithBlocksAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var (validationResult, changes, oldItem) = await DoUpdateAsync(item, operationContext);
            if (validationResult.isValid)
            {
                await BlocksHelper.UpdateBlocksAsync(item, DbContext);
                await DoSaveAsync(item, changes, oldItem, operationContext);
            }

            return new AddOrUpdateOperationResult<TEntity>(item, validationResult.errors, changes);
        }

        public override async Task<TEntity?> DeleteAsync(Guid id,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var entity = await base.DeleteAsync(id, operationContext);
            if (entity != null)
            {
                await BlocksHelper.DeleteBlocksAsync(entity, DbContext);
                await DbContext.SaveChangesAsync();
            }

            return entity;
        }
    }
}
