using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using Newtonsoft.Json.Linq;

namespace BioEngine.Core.DB.Queries
{
    public class QueryContext
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public Guid SiteId { get; private set; } = Guid.Empty;
        public Guid SectionId { get; private set; } = Guid.Empty;
        public Guid[] TagIds { get; private set; } = new Guid[0];

        public bool OrderByDescending { get; protected set; }

        public List<(string propertyName, bool isDescending)> SortQueries { get; protected set; } =
            new List<(string propertyName, bool isDescending)>();

        public List<QueryContextConditionsGroup> ConditionsGroups { get; } =
            new List<QueryContextConditionsGroup>();

        public void SetSite(Site site)
        {
            SiteId = site.Id;
        }

        public void SetSection(Section section)
        {
            SectionId = section.Id;
        }

        public void SetTags(Tag[] tags)
        {
            TagIds = tags.Select(t => t.Id).Distinct().ToArray();
        }
    }

    public class QueryContext<T> : QueryContext where T : class, IEntity
    {
        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public void SetOrderBy(Expression<Func<T, object>> keySelector)
        {
            OrderBy = keySelector;
        }

        public void SetOrderByDescending(Expression<Func<T, object>> keySelector)
        {
            OrderBy = keySelector;
            OrderByDescending = true;
        }

        public void SetOrderByString(string orderBy)
        {
            SortQueries = GetSortParameters(orderBy);
        }

        private static List<(string propertyName, bool isDescending)> GetSortParameters(string orderBy)
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

        public void SetWhere(IEnumerable<QueryContextConditionsGroup> conditionsGroups)
        {
            if (conditionsGroups == null)
                return;
            foreach (var conditionsGroup in conditionsGroups)
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
                    ConditionsGroups.Add(group);
                }
            }
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
    }
}
