using System;
using System.Collections;

namespace BioEngine.Core.DB.Queries
{
    public class QueryContextCondition
    {
        public string Property { get; set; }
        public QueryContextOperator Operator { get; set; }
        public object? Value { get; set; }
        public Type ValueType { get; set; } = typeof(object);

        public QueryContextCondition(string property, QueryContextOperator conditionOperator, object value)
        {
            Property = property;
            Operator = conditionOperator;
            Value = value;
        }

        public string? GetExpression(int valueIndex)
        {
            switch (Operator)
            {
                case QueryContextOperator.Equal:
                    return $"{Property} == @{valueIndex.ToString()}";
                case QueryContextOperator.NotEqual:
                    return $"{Property} != @{valueIndex.ToString()}";
                case QueryContextOperator.Greater:
                    return $"{Property} > @{valueIndex.ToString()}";
                case QueryContextOperator.GreaterOrEqual:
                    return $"{Property} >= @{valueIndex.ToString()}";
                case QueryContextOperator.Less:
                    return $"{Property} < @{valueIndex.ToString()}";
                case QueryContextOperator.LessOrEqual:
                    return $"{Property} <= @{valueIndex.ToString()}";
                case QueryContextOperator.Contains:
                    if (ValueType == typeof(string) || typeof(IEnumerable).IsAssignableFrom(ValueType))
                    {
                        return $"{Property}.ToLower().Contains(@{valueIndex.ToString()})";
                    }

                    break;
                case QueryContextOperator.StartsWith:
                    if (ValueType == typeof(string))
                    {
                        return $"{Property}.ToLower().StartsWith(@{valueIndex.ToString()})";
                    }

                    break;
                case QueryContextOperator.EndsWith:
                    if (ValueType == typeof(string))
                    {
                        return $"{Property}.ToLower().EndsWith(@{valueIndex.ToString()})";
                    }

                    break;
                case QueryContextOperator.In:
                    return $"@{valueIndex.ToString()}.Contains({Property})";
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}