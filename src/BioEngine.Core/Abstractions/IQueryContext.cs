using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Abstractions
{
    public interface IQueryContext
    {
        int? Limit { get; set; }
        int? Offset { get; set; }
        Guid SiteId { get; }
        Guid SectionId { get; }
        Guid[] TagIds { get; }

        bool OrderByDescending { get; }

        List<(string propertyName, bool isDescending)> SortQueries { get; }

        List<QueryContextConditionsGroup> ConditionsGroups { get; }

        void SetSite(Site site);

        void SetSection(Section section);

        void SetTags(Tag[] tags);
    }

    public interface IQueryContext<TEntity> : IQueryContext where TEntity : class, IEntity
    {
        void SetOrderBy(Expression<Func<TEntity, object>> keySelector);
        void SetOrderByDescending(Expression<Func<TEntity, object>> keySelector);
        void SetOrderByString(string orderBy);
        void SetWhere(IEnumerable<QueryContextConditionsGroup> conditionsGroups);
        Expression<Func<TEntity, object>>? OrderBy { get; }
    }
}
