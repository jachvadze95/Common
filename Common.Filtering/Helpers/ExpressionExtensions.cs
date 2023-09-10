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

        public static bool AreSameType(Expression member, Expression constant)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));
            ArgumentNullException.ThrowIfNull(constant, nameof(constant));

            return Nullable.GetUnderlyingType(member.Type) == constant.Type;
        }

        public static void ApplyTransformers(this Expression expresion, StringTransformer transformer)
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
                            expresion = Expression.Convert(expresion, typeof(bool));
                            break;
                        case StringTransformer.Int32:
                            expresion = Expression.Convert(expresion, typeof(int));
                            break;
                        case StringTransformer.Int64:
                            expresion = Expression.Convert(expresion, typeof(long));
                            break;
                        case StringTransformer.Decimal:
                            expresion = Expression.Convert(expresion, typeof(decimal));
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Error applying transformer");
                }
            }
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
                    var type = left.Type;

                    return Expression.Call(MethodExtensions.GetEnumerableContainsGeneric(type), right, left);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
