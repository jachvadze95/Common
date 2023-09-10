using Common.Filtering.Enums;
using Common.Filtering.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    internal static class ExpressionExtensions
    {
        internal static bool IsNullableType(this Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            return expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static Expression ApplyTransformers(this Expression expresion, StringTransformer transformer)
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

        internal static Expression GetComparisonExpression(MemberExpression left, Expression right, CompareWith comparisonType)
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

        internal static Expression? BuildLambdaFromAttributes<TFilter>(PropertyInfo property, TFilter filter, ParameterExpression parameter)
        {
            var attributes = property.GetCustomAttributes<FilterByAttribute>();

            var attributeExpressions = new List<Expression>();

            foreach (var attribute in attributes)
            {
                var compareToColumn = attribute!.ColumnName ?? property.Name;
                var comparisonType = attribute!.ComparisonType;
                var transformer = attribute!.StringTransformer;

                var left = Expression.Property(parameter, compareToColumn);
                Expression right = Expression.Property(Expression.Constant(filter), property).ApplyTransformers(transformer);

                if (comparisonType != CompareWith.In && !left.IsNullableType())
                {
                    right = Expression.Convert(right, left.Type);
                }

                var attributeExpression = ExpressionExtensions.GetComparisonExpression(left, right, comparisonType);
                attributeExpressions.Add(attributeExpression);
            }

            var final = attributeExpressions.First();

            // if there are more than one attribute, combine them with logical operator
            if (attributeExpressions.Count > 1)
            {
                var combineWith = attributes.First()?.CombineWith;

                if (combineWith == LogicalOperator.And)
                {
                    final = attributeExpressions.Aggregate((x, y) => Expression.AndAlso(x, y));
                }
                else if (combineWith == LogicalOperator.Or)
                {
                    final = attributeExpressions.Aggregate((x, y) => Expression.OrElse(x, y));
                }
            }

            return final;
        }

    }
}
