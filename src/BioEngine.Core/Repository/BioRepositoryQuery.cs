using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Extensions;
using JetBrains.Annotations;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryQuery<TEntity> where TEntity : class, IEntity
    {
        private IQueryable<TEntity> _query;
        public int? Limit { get; private set; }
        public int? Offset { get; private set; }

        internal BioRepositoryQuery(IQueryable<TEntity> query)
        {
            _query = query;
        }

        internal IQueryable<TEntity> BuildQuery()
        {
            return _query.AsQueryable();
        }

        public BioRepositoryQuery<TEntity> Take(int take)
        {
            Limit = take;
            return this;
        }

        public BioRepositoryQuery<TEntity> Skip(int skip)
        {
            Offset = skip;
            return this;
        }

        public BioRepositoryQuery<TEntity> Where(Expression<Func<TEntity, bool>> where)
        {
            _query = _query.Where(where);
            return this;
        }

        public BioRepositoryQuery<TEntity> Where(string whereStr, object[] values)
        {
            _query = _query.Where(whereStr, values.ToArray());
            return this;
        }

        public BioRepositoryQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy)
        {
            _query = _query.OrderByDescending(orderBy);
            return this;
        }

        public BioRepositoryQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
        {
            _query = _query.OrderBy(orderBy);
            return this;
        }

        public BioRepositoryQuery<TEntity> Configure(Func<IQueryable<TEntity>, IQueryable<TEntity>> configureQuery)
        {
            _query = configureQuery(_query);
            return this;
        }

        public BioRepositoryQuery<TEntity> Configure(Action<BioRepositoryQuery<TEntity>>? configureQuery = null)
        {
            configureQuery?.Invoke(this);
            return this;
        }

        public BioRepositoryQuery<TEntity> OrderByString(string orderBy)
        {
            _query = _query.OrderByString(orderBy);
            return this;
        }

        public BioRepositoryQuery<TEntity> WhereByString(string whereJson)
        {
            _query = _query.WhereByString(whereJson);
            return this;
        }
    }

    public static class BioRepositoryQueryExtensions
    {
        public static BioRepositoryQuery<T> ForSite<T>(this BioRepositoryQuery<T> query, [NotNull] Site site)
            where T : class, IEntity, ISiteEntity
        {
            return query.Configure(entities => entities.ForSite(site));
        }

        public static BioRepositoryQuery<T> ForSection<T>(this BioRepositoryQuery<T> query, Section section)
            where T : class, IEntity, ISectionEntity
        {
            return query.Configure(entities => entities.ForSection(section));
        }

        public static BioRepositoryQuery<T> WithTags<T>(this BioRepositoryQuery<T> query, Tag[] tags)
            where T : class, IEntity, ITaggedContentEntity
        {
            return query.Configure(entities => entities.WithTags(tags));
        }
    }
}
