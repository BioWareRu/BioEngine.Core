using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
    public abstract class BioRepository<TEntity> : IBioRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly BioContext DbContext;
        protected readonly List<IValidator<TEntity>> Validators;
        protected readonly PropertiesProvider PropertiesProvider;
        public BioRepositoryHooksManager HooksManager { get; set; }

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

        public virtual async Task<(TEntity[] items, int itemsCount)> GetAllAsync(IQueryContext<TEntity>? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null)
        {
            var itemsCount = await CountAsync(queryContext, addConditionsCallback);

            var query = GetBaseQuery(queryContext);
            if (addConditionsCallback != null)
            {
                query = addConditionsCallback(query);
            }

            if (queryContext != null)
            {
                if (queryContext.Offset.HasValue)
                {
                    query = query.Skip(queryContext.Offset.Value);
                }

                if (queryContext.Limit.HasValue)
                {
                    query = query.Take(queryContext.Limit.Value);
                }
            }

            var items = await query.ToArrayAsync();
            await AfterLoadAsync(items);

            return (items, itemsCount);
        }

        protected virtual Task AfterLoadAsync(TEntity entity)
        {
            return entity != null ? AfterLoadAsync(new[] {entity}) : Task.CompletedTask;
        }

        protected virtual async Task AfterLoadAsync(TEntity[] entities)
        {
            await PropertiesProvider.LoadPropertiesAsync(entities);
        }

        public virtual async Task<int> CountAsync(IQueryContext<TEntity>? queryContext = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? addConditionsCallback = null)
        {
            var query = GetBaseQuery(queryContext);
            if (addConditionsCallback != null)
            {
                query = addConditionsCallback(query);
            }


            return await query.CountAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(Guid id, IQueryContext<TEntity>? queryContext = null)
        {
            var item = await GetBaseQuery(queryContext).FirstOrDefaultAsync(i => i.Id.Equals(id));
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<TEntity> GetAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> where,
            IQueryContext<TEntity>? queryContext = null)
        {
            var query = where(GetBaseQuery(queryContext));
            var item = await query.FirstOrDefaultAsync();
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<TEntity[]> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> where,
            IQueryContext<TEntity>? queryContext = null)
        {
            var query = where(GetBaseQuery(queryContext));
            var items = await query.ToArrayAsync();
            await AfterLoadAsync(items);
            return items;
        }

        public virtual async Task<TEntity> NewAsync()
        {
            var item = Activator.CreateInstance<TEntity>();
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<TEntity[]> GetByIdsAsync(Guid[] ids, IQueryContext<TEntity>? queryContext = null)
        {
            var items = await GetBaseQuery(queryContext).Where(i => ids.Contains(i.Id)).ToArrayAsync();
            await AfterLoadAsync(items);

            return items;
        }

        public virtual async Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item,
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
                        await SaveChangesAsync();
                        await AfterSaveAsync(item, null, null, operationContext);
                    }
                }
            }

            return new AddOrUpdateOperationResult<TEntity>(item, validationResult.errors, new PropertyChange[0]);
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

        public virtual async Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item,
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
                        await SaveChangesAsync();
                        await AfterSaveAsync(item, changes, oldItem, operationContext);
                    }
                }
            }

            return new AddOrUpdateOperationResult<TEntity>(item, validationResult.errors, changes);
        }

        public Task FinishBatchAsync()
        {
            _batchMode = false;
            return SaveChangesAsync();
        }


        public virtual async Task<TEntity> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null)
        {
            var item = await GetByIdAsync(id);
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

        protected virtual IQueryable<TEntity> GetBaseQuery(IQueryContext<TEntity>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<TEntity>(), queryContext);
        }

        protected virtual IQueryable<TEntity> ApplyContext(IQueryable<TEntity> query, IQueryContext<TEntity>? queryContext)
        {
            if (queryContext == null)
            {
                return query;
            }

            if (queryContext.OrderBy != null)
            {
                query = !queryContext.OrderByDescending
                    ? query.OrderBy(queryContext.OrderBy)
                    : query.OrderByDescending(queryContext.OrderBy);
            }
            else
            {
                query = query.OrderByDescending(e => e.DateAdded);
            }

            if (queryContext.SortQueries.Any())
            {
                foreach (var sortQuery in queryContext.SortQueries)
                {
                    query = sortQuery.isDescending
                        ? query.OrderByDescending(e => EF.Property<TEntity>(e, sortQuery.propertyName))
                        : query.OrderBy(e => EF.Property<TEntity>(e, sortQuery.propertyName));
                }
            }

            if (queryContext.ConditionsGroups.Any())
            {
                var where = new List<string>();
                var valueIndex = 0;
                var values = new List<object?>();
                foreach (var conditionsGroup in queryContext.ConditionsGroups)
                {
                    var groupWhere = new List<string>();
                    foreach (var condition in conditionsGroup.Conditions)
                    {
                        var expression = condition.GetExpression(valueIndex);
                        if (!string.IsNullOrEmpty(expression))
                        {
                            groupWhere.Add(expression);
                            values.Add(condition.Value);
                            valueIndex++;
                        }
                    }

                    where.Add($"({string.Join(" OR ", groupWhere)})");
                }

                var whereStr = string.Join(" AND ", where);
                query = query.Where(whereStr, values.ToArray());
            }

            return query;
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

        protected virtual async Task<bool> AfterSaveAsync(TEntity item, PropertyChange[]? changes = null, TEntity? oldItem = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            var result = await HooksManager.AfterSaveAsync(item, changes, operationContext);


            if (item.Properties != null)
            {
                foreach (var propertiesEntry in item.Properties)
                {
                    foreach (var val in propertiesEntry.Properties)
                    {
                        await PropertiesProvider.SetAsync(val.Value, item, val.SiteId);
                    }
                }
            }

            return result;
        }
    }
}
