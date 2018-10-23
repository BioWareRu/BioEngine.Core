using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.DB
{
    public class QueryContext<T, TId> where T : class, IEntity<TId>
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public int? SiteId { get; private set; }
        public int? SectionId { get; private set; }
        public int? TagId { get; private set; }
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public bool OrderByDescending { get; private set; }
        public bool IncludeUnpublished { get; set; }

        internal List<(string propertyName, bool isDescending)> SortQueries { get; private set; } =
            new List<(string propertyName, bool isDescending)>();

        internal List<QueryContextConditionsGroup> ConditionsGroups { get; } =
            new List<QueryContextConditionsGroup>();

        public void SetSite(Site site)
        {
            SiteId = site.Id;
        }

        public void SetSection(Section section)
        {
            SectionId = section.Id;
        }

        public void SetTag(Tag tag)
        {
            TagId = tag.Id;
        }

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
            if (conditionsGroups == null) return;
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
                        condition.Value = ParsePropertyValue(condition.ValueType, condition.Value);
                        group.Conditions.Add(condition);
                    }
                }

                if (group.Conditions.Any())
                {
                    ConditionsGroups.Add(group);
                }
            }
        }

        private static object ParsePropertyValue(Type propertyType, object value)
        {
            if (value == null) return null;
            object parsedValue = null;
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
            else parsedValue = Convert.ChangeType(value.ToString(), propertyType);

            return parsedValue;
        }
    }

    internal static class FieldsResolver
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, (string name, Type type)>> Properties =
            new ConcurrentDictionary<string, Dictionary<string, (string name, Type type)>>();

        internal static (string name, Type type)? GetPropertyInfo<T>(string name)
        {
            var typeName = typeof(T).Name;
            if (!Properties.ContainsKey(typeName))
            {
                Properties.TryAdd(typeName,
                    typeof(T).GetProperties()
                        .ToDictionary(p => p.Name.ToLowerInvariant(), p => (p.Name, p.PropertyType)));
            }

            name = name.ToLowerInvariant();

            if (Properties[typeName].ContainsKey(name))
            {
                return Properties[typeName][name];
            }

            return null;
        }
    }

    public enum SortDirection
    {
        Ascending = 1,
        Descending = 2
    }

    internal struct SortQuery
    {
        public readonly string Name;
        public readonly SortDirection SortDirection;

        public SortQuery(string name, SortDirection sortDirection)
        {
            Name = name;
            SortDirection = sortDirection;
        }
    }

    public enum QueryContextOperator
    {
        Equal = 1,
        NotEqual = 2,
        Greater = 3,
        GreaterOrEqual = 4,
        Less = 5,
        LessOrEqual = 6,
        Contains = 7,
        StartsWith = 8,
        EndsWith = 9
    }

    public class QueryContextConditionsGroup
    {
        public QueryContextConditionsGroup(List<QueryContextCondition> conditions)
        {
            Conditions = conditions;
        }

        public List<QueryContextCondition> Conditions { get; }
    }

    public class QueryContextCondition
    {
        public string Property { get; set; }
        public QueryContextOperator Operator { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }

        public string GetExpression(int valueIndex)
        {
            switch (Operator)
            {
                case QueryContextOperator.Equal:
                    return $"{Property} == @{valueIndex}";
                case QueryContextOperator.NotEqual:
                    return $"{Property} != @{valueIndex}";
                case QueryContextOperator.Greater:
                    return $"{Property} > @{valueIndex}";
                case QueryContextOperator.GreaterOrEqual:
                    return $"{Property} >= @{valueIndex}";
                case QueryContextOperator.Less:
                    return $"{Property} < @{valueIndex}";
                case QueryContextOperator.LessOrEqual:
                    return $"{Property} <= @{valueIndex}";
                case QueryContextOperator.Contains:
                    if (ValueType == typeof(string) || typeof(IEnumerable).IsAssignableFrom(ValueType))
                    {
                        return $"{Property}.Contains(@{valueIndex})";
                    }

                    break;
                case QueryContextOperator.StartsWith:
                    if (ValueType == typeof(string))
                    {
                        return $"{Property}.StartsWith(@{valueIndex})";
                    }

                    break;
                case QueryContextOperator.EndsWith:
                    if (ValueType == typeof(string))
                    {
                        return $"{Property}.EndsWith(@{valueIndex})";
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}