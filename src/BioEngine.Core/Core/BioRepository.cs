using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Core
{
    public interface IBioRepository
    {
    }

    public abstract class BioRepository<T, TId> : IBioRepository where T : class, IEntity<TId>
    {
        internal readonly BioContext DbContext;
        protected readonly IValidator<T>[] Validators;

        protected BioRepository(BioRepositoryContext<T, TId> repositoryContext)
        {
            DbContext = repositoryContext.DbContext;
            Validators = repositoryContext.Validators;
        }

        public virtual async Task<(List<T> items, int itemsCount)> GetAll(QueryContext<T, TId> queryContext = null)
        {
            var query = GetBaseQuery(queryContext);
            await DbContext.Set<Section>().ToListAsync();
            var itemsCount = await query.CountAsync();

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
            return (items, itemsCount);
        }

        public virtual async Task<T> GetById(TId id, QueryContext<T, TId> queryContext = null)
        {
            return await GetBaseQuery(queryContext).FirstOrDefaultAsync(i => i.Id.Equals(id));
        }

        public virtual async Task<AddOrUpdateOperationResult<T, TId>> Add(T item)
        {
            var validationResult = await Validate(item);
            if (validationResult.isValid)
            {
                DbContext.Add(item);
                await DbContext.SaveChangesAsync();
            }

            return new AddOrUpdateOperationResult<T, TId>(item, validationResult.errors);
        }

        public virtual async Task<AddOrUpdateOperationResult<T, TId>> Update(T item)
        {
            item.DateUpdated = DateTimeOffset.UtcNow;
            var validationResult = await Validate(item);
            if (validationResult.isValid)
            {
                DbContext.Update(item);
                await DbContext.SaveChangesAsync();
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
            foreach (var validator in Validators)
            {
                var result = await validator.ValidateAsync(entity);
                if (!result.IsValid)
                {
                    failures.AddRange(result.Errors);
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
    }

    public abstract class SiteEntityRepository<T, TId> : BioRepository<T, TId>
        where T : class, IEntity<TId>, ISiteEntity
    {
        protected SiteEntityRepository(BioRepositoryContext<T, TId> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SiteId).HasValue)
            {
                query = query.Where(e => e.SiteIds.Contains(queryContext.SiteId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }
    }

    public abstract class SectionEntityRepository<T, TId> : SiteEntityRepository<T, TId>
        where T : class, IEntity<TId>, ISiteEntity, ISectionEntity
    {
        protected SectionEntityRepository(BioRepositoryContext<T, TId> repositoryContext) : base(repositoryContext)
        {
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SectionId).HasValue)
            {
                query = query.Where(e => e.SectionIds.Contains(queryContext.SectionId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }
    }

    public class BioRepositoryContext<T, TId> where T : IEntity<TId>
    {
        internal BioContext DbContext { get; }
        public IValidator<T>[] Validators { get; }

        internal BioRepositoryContext(BioContext dbContext, IValidator<T>[] validators)
        {
            DbContext = dbContext;
            Validators = validators;
        }
    }

    public class AddOrUpdateOperationResult<T, TId> where T : IEntity<TId>
    {
        public bool IsSuccess { get; }
        public T Entity { get; }
        public ReadOnlyCollection<ValidationFailure> Errors { get; }

        public AddOrUpdateOperationResult(T entity, IEnumerable<ValidationFailure> errors)
        {
            Entity = entity;
            Errors = (ReadOnlyCollection<ValidationFailure>) errors;
            IsSuccess = !errors.Any();
        }
    }
}