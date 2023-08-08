using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace Common.Filtering
{
    //1. Add Error Handling
    //2. fix Relation List and add not
    //3. Refactor Code to Smaller and simpler
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
                var lambda = BuildLambda(property, filter, mainParameter);
                if (lambda == null) continue;

                andList.Add(Expression.Lambda<Func<TEntity, bool>>(lambda, mainParameter));
            }

            if (filterByRelations)
            {
                var innerRelations = filterProperties.Where(x => x.PropertyType.IsPublic && x.PropertyType.IsClass && Attribute.IsDefined(x, typeof(FilterRelationAttribute)));

                foreach (var innerRelation in innerRelations)
                {
                    var relationalLambda = new List<Expression>();
                    var innerFilterValue = innerRelation.GetValue(filter);
                    if (innerFilterValue == null) continue;

                    var attribute = innerRelation.GetCustomAttribute<FilterRelationAttribute>();

                    var relationName = attribute!.RelationName;
                    var relationType = attribute!.RelationType;

                    var innerProperty = Expression.Property(mainParameter, relationName);



                    var innerParameterType = innerProperty.Type.GenericTypeArguments.Any() ? innerProperty.Type.GenericTypeArguments[0] : innerProperty.Type;
                    var innerParameter = Expression.Parameter(innerParameterType);

                    var innerProperties = innerRelation.PropertyType.GetProperties().Where(x => x.PropertyType.IsPublic && Attribute.IsDefined(x, typeof(FilterByAttribute)));

                    foreach (var property in innerProperties)
                    {
                        var lambda = BuildLambda(property, innerFilterValue, innerParameter);
                        if (lambda == null) continue;
                        relationalLambda.Add(lambda);
                    }

                    if (relationalLambda.Any())
                    {
                        var combined = relationalLambda.Count > 1 ? relationalLambda.Aggregate((x, y) => Expression.Lambda(Expression.AndAlso(x, y), innerParameter)) : Expression.Lambda(relationalLambda[0], innerParameter);

                        switch (relationType)
                        {
                            case RelationType.InList:
                                var anyMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Any" && m.GetParameters().Count() == 2).MakeGenericMethod(innerParameterType);
                                var lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Call(anyMethod, innerProperty, combined), mainParameter);
                                andList.Add(lambda);
                                break;
                            case RelationType.NotInList:
                                var notAnyMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Any" && m.GetParameters().Count() == 2).MakeGenericMethod(innerParameterType);
                                var notLambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Not(Expression.Call(notAnyMethod, innerProperty, combined)), mainParameter);
                                andList.Add(notLambda);
                                break;
                            case RelationType.InClass:
                                throw new NotImplementedException();
                            case RelationType.NotInClass:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            var clause = andList.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(x.Body, y.Body), mainParameter));
            query = query.Where(clause);

            return query;
        }

        private static Expression? BuildLambda(PropertyInfo property, object filter, ParameterExpression parameter, bool checkForNullFirst = false)
        {
            var propertyValue = property.GetValue(filter);

            if (propertyValue == null) return null;

            var attributes = property.GetCustomAttributes<FilterByAttribute>();

            var attributeExpressions = new List<Expression>();

            foreach (var attribute in attributes)
            {
                var compareToColumn = attribute!.ColumnName ?? property.Name;
                var comparisonType = attribute!.ComparisonType;
                var transformer = attribute!.StringTransformer;

                var propertyExpression = Expression.Property(parameter, compareToColumn);
                var constantExpression = Expression.Constant(propertyValue);

                if (transformer != StringTransformer.None)
                {
                    try
                    {
                        var converted = Helpers.TransformString(propertyValue, transformer);
                        constantExpression = Expression.Constant(converted);
                    }
                    catch
                    {
                        //This case needs some kind of warrning that convertion attribute is skipped
                        continue;
                    }
                }

                var condition = GetComparisonExpression(propertyExpression, constantExpression, comparisonType, parameter);
                var expression = checkForNullFirst ? Expression.AndAlso(Expression.NotEqual(propertyExpression, Expression.Constant(null)), condition) : condition;

                attributeExpressions.Add(expression);
            }

            var final = attributeExpressions.First();

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

        private static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Expression GetComparisonExpression(MemberExpression propertyExpression, ConstantExpression constantExpression, CompareWith comparisonType, ParameterExpression parameter)
        {
            switch (comparisonType)
            {
                case CompareWith.Equals:
                    constantExpression = CastToNullablePropertyToType(constantExpression, propertyExpression);

                    return Expression.Equal(propertyExpression, constantExpression);
                case CompareWith.GreaterThan:
                    constantExpression = CastToNullablePropertyToType(constantExpression, propertyExpression);

                    return Expression.GreaterThan(propertyExpression, constantExpression);
                case CompareWith.LessThan:
                    constantExpression = CastToNullablePropertyToType(constantExpression, propertyExpression);

                    return Expression.LessThan(propertyExpression, constantExpression);
                case CompareWith.GreaterThanOrEqual:
                    constantExpression = CastToNullablePropertyToType(constantExpression, propertyExpression);

                    return Expression.GreaterThanOrEqual(propertyExpression, constantExpression);
                case CompareWith.LessThanOrEqual:
                    constantExpression = CastToNullablePropertyToType(constantExpression, propertyExpression);

                    return Expression.LessThanOrEqual(propertyExpression, constantExpression);
                case CompareWith.Contains:
                    var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.Contains)} not found");
                    return Expression.Call(propertyExpression, containsMethod, constantExpression);
                case CompareWith.StartsWith:
                    var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.StartsWith)} not found");
                    return Expression.Call(propertyExpression, startsWithMethod, constantExpression);
                case CompareWith.EndsWith:
                    var endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.EndsWith)} not found");
                    return Expression.Call(propertyExpression, endsWithMethod, constantExpression);
                case CompareWith.NotEquals:
                    return Expression.NotEqual(propertyExpression, constantExpression);
                case CompareWith.IsNull:
                    return Expression.Equal(propertyExpression, Expression.Constant(null));
                case CompareWith.In:
                    var type = propertyExpression.Type;

                    var anyMethod = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "Contains")!.MakeGenericMethod(type);

                    return Expression.Call(anyMethod, constantExpression, propertyExpression);
                default:
                    throw new NotSupportedException();
            }
        }

        public static ConstantExpression CastToNullablePropertyToType(ConstantExpression constant, MemberExpression property)
        {
            var isConstantNullable = Helpers.IsNullableExpression(constant);
            var isPropNullable = Helpers.IsNullableExpression(property);

            if (isPropNullable && !isConstantNullable && Helpers.AreSameType(property, constant))
            {
                constant = Expression.Constant(constant.Value, property.Type);
            }

            return constant;
        }
    }
}
