using System.Collections.Generic;
using System.Linq.Expressions;

namespace BioEngine.Core.Extensions
{
    // https://stackoverflow.com/questions/457316/combining-two-expressions-expressionfunct-bool
    internal class SubstExpressionVisitor : ExpressionVisitor
    {
        public readonly Dictionary<Expression, Expression> Subst = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Subst.TryGetValue(node, out var newValue) ? newValue : node;
        }
    }
}
