using System.Collections.Generic;

namespace BioEngine.Core.DB.Queries
{
    public class QueryContextConditionsGroup
    {
        public QueryContextConditionsGroup(List<QueryContextCondition> conditions)
        {
            Conditions = conditions;
        }

        public List<QueryContextCondition> Conditions { get; }
    }
}