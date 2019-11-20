using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Properties;
using BioEngine.Core.Validation;
using FluentValidation;
using FluentValidation.Results;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [PublicAPI]
    public abstract class BioRepository<TEntity> : IBioRepository<TEntity> where TEntity : class, IBioEntity
    {
        protected readonly BioContext DbContext;
        protected readonly List<IValidator<TEntity>> Validators;
        protected readonly PropertiesProvider PropertiesProvider;
        protected readonly BioRepositoryHooksManager HooksManager;

        protected BioRepository(BioRepositoryContext<TEntity> repositoryContext)
        {
            DbContext = repositoryContext.DbContext;
            Validators = repositoryContext.Validators ?? new List<IValidator<TEntity>>();
            PropertiesProvider = repositoryContext.PropertiesProvider;
            HooksManager = repositoryContext.HooksManager;
            Init();
        }


        private void Init()
        {
            RegisterValidators();
        }

        protected virtual void RegisterValidators()
        {
            Validators.Add(new EntityValidator());
        }

        public virtual async Task<(TEntity[] items, int itemsCount)> GetAllAsync()
        {
            var query = CreateRepositoryQuery();

            (TEntity[] items, bool needCount) = await DoGetAllAsync(query);

            var itemsCount = needCount && (query.Offset > 0 || items.Length == query.Limit)
                ? await CountAsync()
                : items.Length;
            await AfterLoadAsync(items);

            return (items, itemsCount);
        }

        public virtual async Task<(TEntity[] items, int itemsCount)> GetAllAsync(
            Action<BioQuery<TEntity>> configureQuery)
        {
            var query = CreateRepositoryQuery().Configure(configureQuery);

            (TEntity[] items, bool needCount) = await DoGetAllAsync(query);

            var itemsCount = needCount && (query.Offset > 0 || items.Length == query.Limit)
                ? await CountAsync(configureQuery)
                : items.Length;
            await AfterLoadAsync(items);

            return (items, itemsCount);
        }

        protected virtual async Task<(TEntity[] items, bool needCount)> DoGetAllAsync(BioQuery<TEntity> query)
        {
            var dbQuery = query.BuildQuery();
            var needCount = false;
            if (query.Offset != null)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
                needCount = true;
            }

            if (query.Limit != null)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
                needCount = true;
            }

            return (await AddIncludes(dbQuery).ToArrayAsync(), needCount);
        }

        public virtual async Task<(TEntity[] items, int itemsCount)> GetAllAsync(
            Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var query = await CreateRepositoryQuery().ConfigureAsync(configureQuery);
            var (items, needCount) = await DoGetAllAsync(query);

            var itemsCount = needCount && (query.Offset > 0 || items.Length == query.Limit)
                ? await CountAsync(configureQuery)
                : items.Length;
            await AfterLoadAsync(items);

            return (items, itemsCount);
        }

        protected virtual Task AfterLoadAsync(TEntity entity)
        {
            return entity != null ? AfterLoadAsync(new[] {entity}) : Task.CompletedTask;
        }

        protected virtual Task AfterLoadAsync(TEntity[] entities)
        {
            return Task.CompletedTask;
        }

        public virtual async Task<int> CountAsync()
        {
            return await CreateRepositoryQuery().BuildQuery().CountAsync();
        }

        public virtual async Task<int> CountAsync(Func<BioQuery<TEntity>, Task> configureQuery)
        {
            return await (await CreateRepositoryQuery().ConfigureAsync(configureQuery)).BuildQuery().CountAsync();
        }

        public virtual async Task<int> CountAsync(Action<BioQuery<TEntity>> configureQuery)
        {
            return await CreateRepositoryQuery().Configure(configureQuery).BuildQuery().CountAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            var query = CreateRepositoryQuery().Where(i => i.Id.Equals(id)).BuildQuery();

            return await DoGetAsync(query);
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var query = (await CreateRepositoryQuery().Where(i => i.Id.Equals(id)).ConfigureAsync(configureQuery))
                .BuildQuery();

            return await DoGetAsync(query);
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, Action<BioQuery<TEntity>> configureQuery)
        {
            var query = CreateRepositoryQuery().Where(i => i.Id.Equals(id)).Configure(configureQuery)
                .BuildQuery();

            return await DoGetAsync(query);
        }

        public virtual Task<TEntity?> GetAsync()
        {
            var query = CreateRepositoryQuery().BuildQuery();
            return DoGetAsync(query);
        }

        public virtual async Task<TEntity?> GetAsync(Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var query = (await CreateRepositoryQuery().ConfigureAsync(configureQuery)).BuildQuery();
            return await DoGetAsync(query);
        }

        public virtual Task<TEntity?> GetAsync(Action<BioQuery<TEntity>> configureQuery)
        {
            var query = CreateRepositoryQuery().Configure(configureQuery).BuildQuery();
            return DoGetAsync(query);
        }

        private async Task<TEntity> DoGetAsync(IQueryable<TEntity> query)
        {
            var item = await AddIncludes(query).FirstOrDefaultAsync();
            if (item != null)
            {
                await AfterLoadAsync(item);
            }

            return item;
        }


        public virtual async Task<TEntity> NewAsync()
        {
            var item = Activator.CreateInstance<TEntity>();
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<TEntity[]> GetByIdsAsync(Guid[] ids)
        {
            var query = CreateRepositoryQuery().Where(i => ids.Contains(i.Id));

            (TEntity[] items, _) = await DoGetAllAsync(query);

            return items;
        }

        public virtual async Task<TEntity[]> GetByIdsAsync(Guid[] ids,
            Func<BioQuery<TEntity>, Task> configureQuery)
        {
            var query = await CreateRepositoryQuery().Where(i => ids.Contains(i.Id)).ConfigureAsync(configureQuery);

            (TEntity[] items, _) = await DoGetAllAsync(query);

            return items;
        }

        public virtual async Task<TEntity[]> GetByIdsAsync(Guid[] ids,
            Action<BioQuery<TEntity>> configureQuery)
        {
            var query = CreateRepositoryQuery().Where(i => ids.Contains(i.Id)).Configure(configureQuery);

            (TEntity[] items, _) = await DoGetAllAsync(query);

            return items;
        }

        public virtual async Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var validationResult = await DoAddAsync(item, operationContext);
            if (validationResult.isValid)
            {
                await DoSaveAsync(item, null, null, operationContext);
            }

            return new AddOrUpdateOperationResult<TEntity>(item, validationResult.errors, new PropertyChange[0]);
        }

        protected async Task DoSaveAsync(TEntity item, PropertyChange[]? changes = null,
            TEntity? oldItem = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            await SaveChangesAsync();
            await AfterSaveAsync(item, null, null, operationContext);
        }

        protected async Task<(bool isValid, IList<ValidationFailure> errors)> DoAddAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidateAsync(item, validationResult, null, operationContext))
            {
                validationResult = await ValidateAsync(item);
                if (validationResult.isValid)
                {
                    if (await BeforeSaveAsync(item, validationResult, null, operationContext))
                    {
                        DbContext.Add(item);
                    }
                }
            }

            return validationResult;
        }


        public PropertyChange[] GetChanges(TEntity item, TEntity oldEntity)
        {
            var changes = new List<PropertyChange>();
            foreach (var propertyEntry in DbContext.Entry(item).Properties)
            {
                if (propertyEntry.IsModified)
                {
                    var name = propertyEntry.Metadata.Name;
                    var originalValue = propertyEntry.OriginalValue;
                    var value = propertyEntry.CurrentValue;
                    changes.Add(new PropertyChange(name, originalValue, value));
                }
            }

            foreach (var navigationEntry in DbContext.Entry(item).Navigations)
            {
                var property = item.GetType().GetProperty(navigationEntry.Metadata.Name);
                if (property != null)
                {
                    var value = property.GetValue(item);
                    var originalValue = property.GetValue(oldEntity);
                    if (value == null && originalValue != null || value != null && !value.Equals(originalValue))
                    {
                        var name = navigationEntry.Metadata.Name;
                        changes.Add(new PropertyChange(name, originalValue, value));
                    }
                }
            }

            return changes.ToArray();
        }

        public DbSet<T> Set<T>() where T : class
        {
            return DbContext.Set<T>();
        }

        public virtual async Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var (validationResult, changes, oldItem) = await DoUpdateAsync(item, operationContext);
            if (validationResult.isValid)
            {
                await DoSaveAsync(item, changes, oldItem, operationContext);
            }

            return new AddOrUpdateOperationResult<TEntity>(item, validationResult.errors, changes);
        }

        protected async
            Task<((bool isValid, IList<ValidationFailure> errors) validationResult, PropertyChange[] changes, TEntity
                oldItem)> DoUpdateAsync(TEntity item,
                IBioRepositoryOperationContext? operationContext = null)
        {
            var oldItem = GetBaseQuery().Where(e => e.Id == item.Id).AsNoTracking().First();
            var changes = GetChanges(item, oldItem);
            item.DateUpdated = DateTimeOffset.UtcNow;
            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidateAsync(item, validationResult, changes, operationContext))
            {
                validationResult = await ValidateAsync(item, changes);
                if (validationResult.isValid)
                {
                    if (await BeforeSaveAsync(item, validationResult, changes, operationContext))
                    {
                        DbContext.Update(item);
                    }
                }
            }

            return (validationResult, changes, oldItem);
        }

        public Task FinishBatchAsync()
        {
            _batchMode = false;
            return SaveChangesAsync();
        }


        public virtual async Task<TEntity?> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null)
        {
            var item = await GetByIdAsync(id, (Action<BioQuery<TEntity>>)null);
            if (item != null)
            {
                DbContext.Remove(item);
                await SaveChangesAsync();
                return item;
            }

            throw new ArgumentException();
        }

        public async Task<TEntity> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null)
        {
            DbContext.Attach(item);
            DbContext.Remove(item);
            await SaveChangesAsync();
            return item;
        }

        protected virtual async Task<bool> SaveChangesAsync()
        {
            if (!_batchMode)
            {
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private bool _batchMode;

        public void BeginBatch()
        {
            _batchMode = true;
        }

        protected virtual async Task<(bool isValid, IList<ValidationFailure> errors)> ValidateAsync(TEntity entity,
            PropertyChange[]? changes = null)
        {
            var failures = new List<ValidationFailure>();
            if (Validators != null)
            {
                foreach (var validator in Validators)
                {
                    var result = await validator.ValidateAsync(entity);
                    if (!result.IsValid)
                    {
                        failures.AddRange(result.Errors);
                    }
                }
            }

            return (!failures.Any(), failures);
        }

        public IQueryable<TEntity> GetBaseQuery()
        {
            return DbContext.Set<TEntity>().AsQueryable();
        }

        protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
        {
            return query;
        }

        public virtual BioQuery<TEntity> CreateRepositoryQuery()
        {
            return new BioQuery<TEntity>(GetBaseQuery());
        }

        protected virtual Task<bool> BeforeValidateAsync(TEntity item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
        {
            return HooksManager.BeforeValidateAsync(item, validationResult, changes, operationContext);
        }

        protected virtual Task<bool> BeforeSaveAsync(TEntity item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
        {
            return HooksManager.BeforeSaveAsync(item, validationResult, changes, operationContext);
        }

        protected virtual Task<bool> AfterSaveAsync(TEntity item, PropertyChange[]? changes = null,
            TEntity? oldItem = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            return HooksManager.AfterSaveAsync(item, changes, operationContext);
        }
    }
}
