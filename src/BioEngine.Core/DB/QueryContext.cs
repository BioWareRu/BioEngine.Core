using System;
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

                    var propertyName = FieldsResolver.GetPropertyName<T>(p);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        sortParameters.Add((propertyName, isDescending));
                    }
                });
            }

            return sortParameters;
        }
    }

    internal static class FieldsResolver
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> Properties =
            new ConcurrentDictionary<string, Dictionary<string, string>>();

        internal static string GetPropertyName<T>(string name)
        {
            var typeName = typeof(T).Name;
            if (!Properties.ContainsKey(typeName))
            {
                Properties.TryAdd(typeName,
                    typeof(T).GetProperties().ToDictionary(p => p.Name.ToLowerInvariant(), p => p.Name));
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
}