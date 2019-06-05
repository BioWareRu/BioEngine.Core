using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioEngine.Core.Extensions
{
    public static class QueryBuilderExtensions
    {
        public static IQueryable<T> ForSite<T>(this IQueryable<T> query, Site site) where T : IEntity, ISiteEntity
        {
            return query.Where(e => e.SiteIds.Contains(site.Id));
        }

        public static IQueryable<T> ForSection<T>(this IQueryable<T> query, Section section)
            where T : IEntity, ISectionEntity
        {
            return query.Where(e => e.SectionIds.Contains(section.Id));
        }

        public static IQueryable<T> WithTags<T>(this IQueryable<T> query, Tag[] tags)
            where T : IEntity, ITaggedContentEntity
        {
            Expression<Func<T, bool>> ex = null;
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

        public static IQueryable<T> OrderByString<T>(this IQueryable<T> query, string orderBy) where T : IEntity
        {
            var sortQueries = GetSortParameters<T>(orderBy);
            if (sortQueries.Any())
            {
                foreach (var sortQuery in sortQueries)
                {
                    query = sortQuery.isDescending
                        ? query.OrderByDescending(e => EF.Property<T>(e, sortQuery.propertyName))
                        : query.OrderBy(e => EF.Property<T>(e, sortQuery.propertyName));
                }
            }

            return query;
        }

        public static IQueryable<T> WhereByString<T>(this IQueryable<T> query, string whereJson) where T : IEntity
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
                        var propertyInfo = FieldsResolver.GetPropertyInfo<T>(condition.Property);
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
                    query = query.Where(whereStr, values.ToArray());
                }
            }

            return query;
        }

        private static object? ParsePropertyValue(Type propertyType, object value)
        {
            if (value == null)
                return null;
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

                if (Enum.IsDefined(enumType, value.ToString()) || (parsed && Enum.IsDefined(enumType, intValue)))
                    parsedValue = Enum.Parse(enumType, value.ToString());
            }

            else if (propertyType == typeof(bool))
                parsedValue = value.ToString() == "1" ||
                              value.ToString() == "true" ||
                              value.ToString() == "on" ||
                              value.ToString() == "checked";
            else if (propertyType == typeof(Uri))
                parsedValue = new Uri(Convert.ToString(value));
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
                parsedValue = Convert.ChangeType(value.ToString(), propertyType);

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
    }
}
