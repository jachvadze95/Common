using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace Common.Filtering.Helpers
{
    public static class FilterExtensions
    {
        /// <summary>
        /// Method attaches filters on queriable list
        /// </summary>
        /// <typeparam name="TEntity">Entity that will be filtered</typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="filterByRelations">true if TEntitys relations should be filtered and filter object has class member (inner filter) with appropriate attributes, works only 1 level deep</param>
        /// <returns>IQueryable<typeparamref name="TEntity"/>> newly attached filters</returns>
        public static IQueryable<TEntity> FilterBy<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter filter, bool filterByRelations = false)
            where TEntity : class
        {
            if (filter == null) return query;

            var andList = new List<Expression<Func<TEntity, bool>>>();

            var mainParameter = Expression.Parameter(typeof(TEntity));


            var filterProperties = typeof(TFilter).GetProperties();
            var properties = filterProperties.Where(x => x.PropertyType.IsPublic && Attribute.IsDefined(x, typeof(FilterByAttribute)));

            foreach (var property in properties)
            {
                var val = property.GetValue(filter);
                if (val == null) continue;

                var lambda = BuildLambdaFromAttributes(property, filter, mainParameter);
                if (lambda == null) continue;

                andList.Add(Expression.Lambda<Func<TEntity, bool>>(lambda, mainParameter));
            }

            var clause = andList.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(x.Body, y.Body), mainParameter));
            query = query.Where(clause);

            return query;
        }

        private static Expression? BuildLambdaFromAttributes<TFilter>(PropertyInfo property, TFilter filter, ParameterExpression parameter)
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

                if (comparisonType != CompareWith.In && !left.IsNullableType()){
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
