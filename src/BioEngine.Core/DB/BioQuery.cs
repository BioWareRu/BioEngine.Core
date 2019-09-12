using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Entities;
using BioEngine.Core.Extensions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioEngine.Core.DB
{
    public class BioQuery<TEntity> where TEntity : class
    {
        private IQueryable<TEntity> _query;

        private readonly List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> _where =
            new List<Func<IQueryable<TEntity>, IQueryable<TEntity>>>();

        private readonly List<(Expression<Func<TEntity, object>> expression, bool desc)> _orderBy =
            new List<(Expression<Func<TEntity, object>> expression, bool desc)>();

        public int? Limit { get; private set; }
        public int? Offset { get; private set; }

        public BioQuery(IQueryable<TEntity> query)
        {
            _query = query;
        }

        public IQueryable<TEntity> BuildQuery()
        {
            foreach (var func in _where)
            {
                _query = func.Invoke(_query);
            }

            foreach (var orderBy in _orderBy)
            {
                _query = orderBy.desc
                    ? _query.OrderByDescending(orderBy.expression)
                    : _query.OrderBy(orderBy.expression);
            }

            return _query;
        }

        public BioQuery<TEntity> Take(int take)
        {
            Limit = take;
            return this;
        }

        public BioQuery<TEntity> Skip(int skip)
        {
            Offset = skip;
            return this;
        }

        public BioQuery<TEntity> Where(Expression<Func<TEntity, bool>> where)
        {
            _where.Add(query => query.Where(where));
            return this;
        }

        public BioQuery<TEntity> Where(string whereStr, object[] values)
        {
            _where.Add(query => query.Where(whereStr, values));
            return this;
        }

        public BioQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy)
        {
            _orderBy.Add((orderBy, true));
            return this;
        }

        public BioQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
        {
            _orderBy.Add((orderBy, false));
            return this;
        }

        public BioQuery<TEntity> Configure(Func<IQueryable<TEntity>, IQueryable<TEntity>> configureQuery)
        {
            _query = configureQuery(_query);
            return this;
        }

        public BioQuery<TEntity> Configure(Action<BioQuery<TEntity>>? configureQuery = null)
        {
            configureQuery?.Invoke(this);
            return this;
        }

        public BioQuery<TEntity> OrderByString(string orderBy)
        {
            var sortQueries = GetSortParameters<TEntity>(orderBy);
            if (sortQueries.Any())
            {
                foreach (var sortQuery in sortQueries)
                {
                    if (sortQuery.isDescending)
                    {
                        OrderByDescending(e => EF.Property<TEntity>(e, sortQuery.propertyName));
                    }
                    else
                    {
                        OrderBy(e => EF.Property<TEntity>(e, sortQuery.propertyName));
                    }
                }
            }

            return this;
        }

        public BioQuery<TEntity> WhereByString(string whereJson)
        {
            var where = JsonConvert.DeserializeObject<List<QueryContextConditionsGroup>>(whereJson);
            if (where != null)
            {
                var conditionsGroups = new List<QueryContextConditionsGroup>();
                foreach (var conditionsGroup in where)
                {
                    var group = new QueryContextConditionsGroup(new List<QueryContextCondition>());
                    foreach (var condition in conditionsGroup.Conditions)
                    {
                        var propertyInfo = FieldsResolver.GetPropertyInfo<TEntity>(condition.Property);
                        if (propertyInfo != null)
                        {
                            condition.Property = propertyInfo.Value.name;
                            condition.ValueType = propertyInfo.Value.type;
                            if (condition.Value != null)
                            {
                                condition.Value = ParsePropertyValue(condition.ValueType, condition.Value);
                            }

                            group.Conditions.Add(condition);
                        }
                    }

                    if (group.Conditions.Any())
                    {
                        conditionsGroups.Add(group);
                    }
                }

                if (conditionsGroups.Any())
                {
                    var whereQueries = new List<string>();
                    var valueIndex = 0;
                    var values = new List<object?>();
                    foreach (var conditionsGroup in conditionsGroups)
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

                        whereQueries.Add($"({string.Join(" OR ", groupWhere)})");
                    }

                    var whereStr = string.Join(" AND ", whereQueries);
                    Where(whereStr, values.ToArray());
                }
            }

            return this;
        }

        private static object? ParsePropertyValue(Type propertyType, object? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is JArray arr)
            {
                var values = Activator.CreateInstance(typeof(List<>).MakeGenericType(propertyType)) as IList;
                if (values != null)
                {
                    foreach (var child in arr.Children())
                    {
                        values.Add(ParsePropertyValue(propertyType, child));
                    }
                }

                return values;
            }

            object? parsedValue = null;
            var nullableType = Nullable.GetUnderlyingType(propertyType);
            if (nullableType != null)
            {
                propertyType = nullableType;
            }

            if (propertyType.IsEnum)
            {
                var enumType = propertyType;
                var parsed = int.TryParse(value.ToString(), out var intValue);

                if (Enum.IsDefined(enumType, value.ToString()) || parsed && Enum.IsDefined(enumType, intValue))
                    parsedValue = Enum.Parse(enumType, value.ToString());
            }

            else if (propertyType == typeof(bool))
            {
                parsedValue = value.ToString() == "1" ||
                             value.ToString() == "true" ||
                             value.ToString() == "on" ||
                             value.ToString() == "checked";
            }
            else if (propertyType == typeof(Uri))
            {
                parsedValue = new Uri(Convert.ToString(value));
            }
            else if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
            {
                if (DateTimeOffset.TryParse(value.ToString(), out var dto))
                {
                    parsedValue = dto;
                }
            }
            else if (propertyType == typeof(Guid))
            {
                if (Guid.TryParse(value.ToString(), out var dto))
                {
                    parsedValue = dto;
                }
            }
            else
            {
                parsedValue = Convert.ChangeType(value.ToString(), propertyType);
            }

            return parsedValue;
        }

        private static List<(string propertyName, bool isDescending)> GetSortParameters<T>(string orderBy)
        {
            var sortParameters = new List<(string propertyName, bool isDescending)>();
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderBy.Split(',').ToList().ForEach(p =>
                {
                    var isDescending = false;
                    if (p[0] == '-')
                    {
                        isDescending = true;
                        p = p.Substring(1);
                    }

                    var propertyName = FieldsResolver.GetPropertyInfo<T>(p);
                    if (propertyName.HasValue)
                    {
                        sortParameters.Add((propertyName.Value.name, isDescending));
                    }
                });
            }

            return sortParameters;
        }

        public BioQuery<TEntity> Paginate(int page, int itemsPerPage)
        {
            var offset = 0;
            if (page > 0)
            {
                offset = (page - 1) * itemsPerPage;
            }

            Offset = offset;
            Limit = itemsPerPage;
            return this;
        }
    }

    public static class BioQueryExtensions
    {
        public static BioQuery<T> ForSite<T>(this BioQuery<T> query, [NotNull] Site site)
            where T : class, IEntity, ISiteEntity
        {
            return query.Where(e => e.SiteIds.Contains(site.Id));
        }

        public static BioQuery<T> ForSection<T>(this BioQuery<T> query, Section section)
            where T : class, IEntity, ISectionEntity
        {
            return query.Where(e => e.SectionIds.Contains(section.Id));
        }

        public static BioQuery<T> WithTags<T>(this BioQuery<T> query, Tag[] tags)
            where T : class, IEntity, ITaggedContentEntity
        {
            Expression<Func<T, bool>>? ex = null;
            foreach (var tag in tags)
            {
                ex = ex == null ? post => post.TagIds.Contains(tag.Id) : ex.And(post => post.TagIds.Contains(tag.Id));
            }

            if (ex != null)
            {
                query = query.Where(ex);
            }

            return query;
        }

        public static async Task<(T[] items, int itemsCount)> GetAllAsync<T>(this BioQuery<T> query) where T : class
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

            var items = await dbQuery.ToArrayAsync();
            var itemsCount = needCount && (query.Offset > 0 || items.Length == query.Limit)
                ? await query.BuildQuery().CountAsync()
                : items.Length;

            return (items, itemsCount);
        }
    }
}
