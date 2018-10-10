﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public abstract class BioRepository<T, TId> : IBioRepository where T : class, IEntity<TId>
    {
        internal readonly BioContext DbContext;
        protected readonly IValidator<T>[] Validators;
        protected readonly IRepositoryFilter[] Filters;
        protected readonly SettingsProvider SettingsProvider;

        protected BioRepository(BioRepositoryContext<T, TId> repositoryContext)
        {
            DbContext = repositoryContext.DbContext;
            Validators = repositoryContext.Validators;
            Filters = repositoryContext.Filters;
            SettingsProvider = repositoryContext.SettingsProvider;
        }

        public virtual async Task<(List<T> items, int itemsCount)> GetAll(QueryContext<T, TId> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
        {
            var itemsCount = await Count(queryContext, addConditionsCallback);

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

            var items = await query.ToListAsync();
            await AfterLoad(items);

            return (items, itemsCount);
        }

        public virtual async Task AfterLoad(IEnumerable<T> entities)
        {
            await SettingsProvider.LoadSettings<T, TId>(entities);
        }

        public virtual async Task<int> Count(QueryContext<T, TId> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
        {
            var query = GetBaseQuery(queryContext);
            if (addConditionsCallback != null)
            {
                query = addConditionsCallback(query);
            }


            return await query.CountAsync();
        }

        public virtual async Task<T> GetById(TId id, QueryContext<T, TId> queryContext = null)
        {
            var item = await GetBaseQuery(queryContext).FirstOrDefaultAsync(i => i.Id.Equals(id));
            await AfterLoad(new[] {item});
            return item;
        }

        public virtual async Task<T> New()
        {
            var item = Activator.CreateInstance<T>();
            await AfterLoad(new[] {item});
            return item;
        }

        public virtual async Task<IEnumerable<T>> GetByIds(TId[] ids, QueryContext<T, TId> queryContext = null)
        {
            var items = await GetBaseQuery(queryContext).Where(i => ids.Contains(i.Id)).ToListAsync();
            await AfterLoad(items);

            return items;
        }

        public virtual async Task<AddOrUpdateOperationResult<T, TId>> Add(T item)
        {
            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidate(item, validationResult))
            {
                validationResult = await Validate(item);
                if (validationResult.isValid)
                {
                    if (await BeforeSave(item, validationResult))
                    {
                        DbContext.Add(item);
                        await DbContext.SaveChangesAsync();
                        await AfterSave(item);
                    }
                }
            }

            return new AddOrUpdateOperationResult<T, TId>(item, validationResult.errors);
        }

        public virtual async Task<AddOrUpdateOperationResult<T, TId>> Update(T item)
        {
            item.DateUpdated = DateTimeOffset.UtcNow;
            (bool isValid, IList<ValidationFailure> errors) validationResult = (false, new List<ValidationFailure>());
            if (await BeforeValidate(item, validationResult))
            {
                validationResult = await Validate(item);
                if (validationResult.isValid)
                {
                    if (await BeforeSave(item, validationResult))
                    {
                        DbContext.Update(item);
                        await DbContext.SaveChangesAsync();
                        await AfterSave(item);
                    }
                }
            }

            return new AddOrUpdateOperationResult<T, TId>(item, validationResult.errors);
        }

        public virtual async Task Publish(T item)
        {
            item.IsPublished = true;
            item.DatePublished = DateTimeOffset.UtcNow;
            await Update(item);
        }

        public virtual async Task<bool> Delete(TId id)
        {
            var item = await GetById(id);
            if (item != null)
            {
                DbContext.Remove(item);
                await DbContext.SaveChangesAsync();
                return true;
            }

            throw new ArgumentException();
        }

        public virtual async Task<(bool isValid, IList<ValidationFailure> errors)> Validate(T entity)
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

        protected virtual IQueryable<T> GetBaseQuery(QueryContext<T, TId> queryContext = null)
        {
            return ApplyContext(DbContext.Set<T>(), queryContext);
        }

        protected virtual IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
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

            if (queryContext.SortQueries.Any())
            {
                foreach (var sortQuery in queryContext.SortQueries)
                {
                    query = sortQuery.isDescending
                        ? query.OrderByDescending(e => EF.Property<T>(e, sortQuery.propertyName))
                        : query.OrderBy(e => EF.Property<T>(e, sortQuery.propertyName));
                }
            }

            return query;
        }

        protected virtual async Task<bool> BeforeValidate(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.BeforeValidate<T, TId>(item, validationResult))
                {
                    result = false;
                }
            }

            return result;
        }

        protected virtual async Task<bool> BeforeSave(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.BeforeSave<T, TId>(item, validationResult))
                {
                    result = false;
                }
            }

            return result;
        }

        protected virtual async Task<bool> AfterSave(T item)
        {
            var result = true;
            foreach (var repositoryFilter in Filters)
            {
                if (!repositoryFilter.CanProcess(item.GetType())) continue;
                if (!await repositoryFilter.AfterSave<T, TId>(item))
                {
                    result = false;
                }
            }

            foreach (var itemSetting in item.Settings)
            {
                foreach (var val in itemSetting.Settings)
                {
                    await SettingsProvider.Set(val.Value, item, val.SiteId);
                }
            }

            return result;
        }
    }
}