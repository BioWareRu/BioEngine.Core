using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Properties;
using BioEngine.Core.Validation;
using FluentValidation;
using FluentValidation.Results;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [PublicAPI]
    public abstract class BioRepository<T> : IBioRepository<T> where T : class, IEntity
    {
        internal readonly BioContext DbContext;
        protected readonly List<IValidator<T>> Validators;
        protected readonly List<IRepositoryFilter> Filters;
        protected readonly PropertiesProvider PropertiesProvider;

        protected BioRepository(BioRepositoryContext<T> repositoryContext)
        {
            DbContext = repositoryContext.DbContext;
            Validators = repositoryContext.Validators ?? new List<IValidator<T>>();
            Filters = repositoryContext.Filters ?? new List<IRepositoryFilter>();
            PropertiesProvider = repositoryContext.PropertiesProvider;

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

        public virtual async Task<(T[] items, int itemsCount)> GetAllAsync(QueryContext<T> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
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

        protected virtual Task AfterLoadAsync(T entity)
        {
            return entity != null ? AfterLoadAsync(new[] {entity}) : Task.CompletedTask;
        }

        protected virtual async Task AfterLoadAsync(T[] entities)
        {
            await PropertiesProvider.LoadPropertiesAsync(entities);
        }

        public virtual async Task<int> CountAsync(QueryContext<T> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
        {
            var query = GetBaseQuery(queryContext);
            if (addConditionsCallback != null)
            {
                query = addConditionsCallback(query);
            }


            return await query.CountAsync();
        }

        public virtual async Task<T> GetByIdAsync(Guid id, QueryContext<T> queryContext = null)
        {
            var item = await GetBaseQuery(queryContext).FirstOrDefaultAsync(i => i.Id.Equals(id));
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<T> NewAsync()
        {
            var item = Activator.CreateInstance<T>();
            await AfterLoadAsync(item);
            return item;
        }

        public virtual async Task<T[]> GetByIdsAsync(Guid[] ids, QueryContext<T> queryContext = null)
        {
            var items = await GetBaseQuery(queryContext).Where(i => ids.Contains(i.Id)).ToArrayAsync();
            await AfterLoadAsync(items);

            return items;
        }

        public virtual async Task<AddOrUpdateOperationResult<T>> AddAsync(T item)
        {
            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidateAsync(item, validationResult))
            {
                validationResult = await ValidateAsync(item);
                if (validationResult.isValid)
                {
                    if (await BeforeSaveAsync(item, validationResult))
                    {
                        DbContext.Add(item);
                        await SaveChangesAsync();
                        await AfterSaveAsync(item);
                    }
                }
            }

            return new AddOrUpdateOperationResult<T>(item, validationResult.errors);
        }

        public PropertyChange[] GetChanges(T item)
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

            return changes.ToArray();
        }

        public virtual async Task<AddOrUpdateOperationResult<T>> UpdateAsync(T item)
        {
            var changes = GetChanges(item);
            item.DateUpdated = DateTimeOffset.UtcNow;
            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidateAsync(item, validationResult, changes))
            {
                validationResult = await ValidateAsync(item, changes);
                if (validationResult.isValid)
                {
                    if (await BeforeSaveAsync(item, validationResult, changes))
                    {
                        DbContext.Update(item);
                        await SaveChangesAsync();
                        await AfterSaveAsync(item, changes);
                    }
                }
            }

            return new AddOrUpdateOperationResult<T>(item, validationResult.errors);
        }

        public Task FinishBatchAsync()
        {
            _batchMode = false;
            return SaveChangesAsync();
        }

        public virtual async Task PublishAsync(T item)
        {
            item.IsPublished = true;
            item.DatePublished = DateTimeOffset.UtcNow;
            var changes = GetChanges(item);
            await UpdateAsync(item);
            await AfterSaveAsync(item, changes);
        }

        public virtual async Task UnPublishAsync(T item)
        {
            item.IsPublished = false;
            item.DatePublished = null;
            var changes = GetChanges(item);
            await UpdateAsync(item);
            await AfterSaveAsync(item, changes);
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var item = await GetByIdAsync(id);
            if (item != null)
            {
                DbContext.Remove(item);
                await SaveChangesAsync();
                return true;
            }

            throw new ArgumentException();
        }

        public Task<bool> DeleteAsync(T item)
        {
            DbContext.Attach(item);
            DbContext.Remove(item);
            return SaveChangesAsync();
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

        protected virtual async Task<(bool isValid, IList<ValidationFailure> errors)> ValidateAsync(T entity,
            PropertyChange[] changes = null)
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

        protected virtual IQueryable<T> GetBaseQuery(QueryContext<T> queryContext = null)
        {
            return ApplyContext(DbContext.Set<T>(), queryContext);
        }

        protected virtual IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T> queryContext)
        {
            if (queryContext == null) return query;

            if (!queryContext.IncludeUnpublished)
            {
                query = query.Where(x => x.IsPublished);
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
                        ? query.OrderByDescending(e => EF.Property<T>(e, sortQuery.propertyName))
                        : query.OrderBy(e => EF.Property<T>(e, sortQuery.propertyName));
                }
            }

            if (queryContext.ConditionsGroups.Any())
            {
                var where = new List<string>();
                var valueIndex = 0;
                var values = new List<object>();
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

        protected virtual async Task<bool> BeforeValidateAsync(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.BeforeValidateAsync(item, validationResult, changes))
                {
                    result = false;
                }
            }

            return result;
        }

        protected virtual async Task<bool> BeforeSaveAsync(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.BeforeSaveAsync(item, validationResult, changes))
                {
                    result = false;
                }
            }

            return result;
        }

        protected virtual async Task<bool> AfterSaveAsync(T item, PropertyChange[] changes = null)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.AfterSaveAsync(item, changes))
                {
                    result = false;
                }
            }

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

    public struct PropertyChange
    {
        public PropertyChange(string name, object originalValue, object currentValue)
        {
            Name = name;
            OriginalValue = originalValue;
            CurrentValue = currentValue;
        }

        public string Name { get; }
        public object OriginalValue { get; }
        public object CurrentValue { get; }
    }
}
