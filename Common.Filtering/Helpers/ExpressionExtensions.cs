using Common.Filtering.Enums;
using Common.Filtering.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    public static class ExpressionExtensions
    {
        public static bool IsNullableType(this Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            return expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Expression ApplyTransformers(this Expression expresion, StringTransformer transformer)
        {
            if (transformer != StringTransformer.None)
            {
                try
                {
                    switch (transformer)
                    {
                        case StringTransformer.LowerCase:
                            expresion = Expression.Call(expresion, MethodExtensions.GetStringToLower());
                            break;
                        case StringTransformer.UpperCase:
                            expresion = Expression.Call(expresion, MethodExtensions.GetStringToUpper());
                            break;
                        case StringTransformer.Boolean:
                            expresion = Expression.Convert(expresion, typeof(bool), MethodExtensions.GetBoolParse());
                            break;
                        case StringTransformer.Int32:
                            expresion = Expression.Convert(expresion, typeof(int), MethodExtensions.GetIntParse());
                            break;
                        case StringTransformer.Int64:
                            expresion = Expression.Convert(expresion, typeof(long), MethodExtensions.GetLongParse());
                            break;
                        case StringTransformer.Decimal:
                            expresion = Expression.Convert(expresion, typeof(decimal), MethodExtensions.GetDecimalParse());
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Error applying transformer");
                }
            }

            return expresion;
        }

        public static Expression GetComparisonExpression(MemberExpression left, Expression right, CompareWith comparisonType)
        {
            switch (comparisonType)
            {
                case CompareWith.Equals:
                    return Expression.Equal(left, right);
                case CompareWith.GreaterThan:
                    return Expression.GreaterThan(left, right);
                case CompareWith.LessThan:
                    return Expression.LessThan(left, right);
                case CompareWith.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                case CompareWith.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case CompareWith.Contains:
                    return Expression.Call(left, MethodExtensions.GetStringContains(), right);
                case CompareWith.StartsWith:
                    return Expression.Call(left, MethodExtensions.GetStringStartsWith(), right);
                case CompareWith.EndsWith:
                    return Expression.Call(left, MethodExtensions.GetStringEndsWith(), right);
                case CompareWith.NotEquals:
                    return Expression.NotEqual(left, right);
                case CompareWith.IsNull:
                    return Expression.Equal(left, Expression.Constant(null));
                case CompareWith.In:
                    return Expression.Call(MethodExtensions.GetEnumerableContainsGeneric(left.Type), right, left);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
