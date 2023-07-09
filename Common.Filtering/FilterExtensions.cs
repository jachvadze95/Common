using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace Common.Filtering
{
    public static class FilterExtensions
    {
        public static IQueryable<TEntity> FilterBy<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter filter)
            where TEntity : class
        {
            if (filter == null) return query;

            var parameter = Expression.Parameter(typeof(TEntity));
            var filterProperties = typeof(TFilter).GetProperties();

            var properties = filterProperties.Where(x =>
                x.PropertyType.IsPublic &&
                Attribute.IsDefined(x, typeof(FilterByAttribute)));

            foreach (var property in properties)
            {
                var lambda = BuildLambda<TEntity>(property, filter, parameter);
                if (lambda == null) continue;

                query = query.Where(lambda);

            }

            var innerRelations = filterProperties.Where(x =>
                           x.PropertyType.IsPublic &&
                           x.PropertyType.IsClass &&
                           Attribute.IsDefined(x, typeof(FilterRelationAttribute)));

            foreach (var innerRelation in innerRelations)
            {
                var propertyValue = innerRelation.GetValue(filter);
                if (propertyValue == null) continue;

                var attribute = innerRelation.GetCustomAttribute<FilterRelationAttribute>();
                var relationNames = attribute!.RelationNames;
                var relationType = attribute!.RelationType;
                var logicalOperator = attribute.LogicalOperator;

                var singleRelation = relationNames.Length == 1;
                if(!singleRelation && logicalOperator == null)
                {
                    throw new ArgumentException("Logical operator must be specified when there are multiple relation names");
                }

                var innerProperties = innerRelation.PropertyType.GetProperties().Where(x =>
                   x.PropertyType.IsPublic &&
                                      Attribute.IsDefined(x, typeof(FilterByAttribute)));

                if (singleRelation)
                {
                    var innerParameter = Expression.Property(parameter, relationNames[0]);
                    var innerParameterType = innerParameter.Type.GenericTypeArguments[0];

                    foreach (var innerProperty in innerProperties)
                    {
                        Expression<Func<TEntity, bool>>? lambda = null;

                        switch (relationType)
                        {
                            case RelationType.Class:
                                lambda = BuildLambda<TEntity>(innerProperty, propertyValue, Expression.Parameter(innerParameterType));
                                break;
                            case RelationType.List:
                                var innerLambda = BuildLambdaGeneric(innerProperty, propertyValue, Expression.Parameter(innerParameterType), innerParameterType);
                                if (innerLambda == null) continue;

                                var anyMethod = typeof(Enumerable).GetMethods()
                                        .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2)!
                                        .MakeGenericMethod(innerParameterType);


                                lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Call(anyMethod, innerParameter, innerLambda), parameter);
                                break;
                        }

                        if (lambda == null) continue;
                        query = query.Where(lambda);
                    }
                }
                else
                {
                    IEnumerable<Expression<Func<TEntity, bool>>> lambdas = new List<Expression<Func<TEntity, bool>>>();

                    foreach (var relationName in relationNames)
                    {
                        var innerParameter = Expression.Property(parameter, relationName);
                        var innerParameterType = innerParameter.Type.GenericTypeArguments[0];

                        foreach (var innerProperty in innerProperties)
                        {
                            Expression<Func<TEntity, bool>>? lambda = null;

                            switch (relationType)
                            {
                                case RelationType.Class:
                                    lambda = BuildLambda<TEntity>(innerProperty, propertyValue, Expression.Parameter(innerParameterType), true);
                                    break;
                                case RelationType.List:
                                    var innerLambda = BuildLambdaGeneric(innerProperty, propertyValue, Expression.Parameter(innerParameterType), innerParameterType);
                                    if (innerLambda == null) continue;

                                    var anyMethod = typeof(Enumerable).GetMethods()
                                            .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2)!
                                            .MakeGenericMethod(innerParameterType);


                                    lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Call(anyMethod, innerParameter, innerLambda), parameter);
                                    break;
                            }

                            if (lambda != null)
                            {
                                lambdas = lambdas.Append(lambda);
                            }
                        }
                    }

                    if (lambdas.Any())
                    {
                        Expression<Func<TEntity, bool>>? lambda = null;

                        if (logicalOperator == LogicalOperator.And)
                        {
                            lambda = lambdas.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(x.Body, y.Body), parameter));
                        }
                        else if (logicalOperator == LogicalOperator.Or)
                        {
                            lambda = lambdas.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.OrElse(x.Body, y.Body), parameter));
                        }

                        if (lambda != null)
                        {
                            query = query.Where(lambda);
                        }
                    }
                }
            }

            return query;
        }

        public static Expression<Func<TEntity, bool>>? BuildLambda<TEntity>(PropertyInfo property, object filter, ParameterExpression parameter, bool checkForNullFirst = false)
        {
            var propertyValue = property.GetValue(filter);

            if (propertyValue == null) return null;

            var attribute = property.GetCustomAttribute<FilterByAttribute>();

            var compareToColumn = attribute!.ColumnName ?? property.Name;
            var comparisonType = attribute!.ComparisonType;

            var propertyExpression = Expression.Property(parameter, compareToColumn);
            var constantExpression = Expression.Constant(propertyValue);

            var condition = GetComparisonExpression(propertyExpression, constantExpression, comparisonType, parameter);
            var mergedCondition = checkForNullFirst ? Expression.AndAlso(Expression.NotEqual(propertyExpression, Expression.Constant(null)), condition) : condition;

            return Expression.Lambda<Func<TEntity, bool>>(mergedCondition, parameter);
        }

        public static Expression? BuildLambdaGeneric(PropertyInfo property, object filter, ParameterExpression parameter, Type type)
        {
            MethodInfo? methodLogAction = typeof(FilterExtensions).GetMethod(nameof(BuildLambda));
            if (methodLogAction == null) throw new Exception("BuildLambda Method Is Missing");

            MethodInfo generic = methodLogAction.MakeGenericMethod(type);

            return (Expression?)generic.Invoke(null, new object[] { property, filter, Expression.Parameter(type) });
        }

        private static Expression GetComparisonExpression(MemberExpression propertyExpression, ConstantExpression constantExpression, CompareWith comparisonType, ParameterExpression parameter)
        {
            switch (comparisonType)
            {
                case CompareWith.Equals:
                    return Expression.Equal(propertyExpression, constantExpression);
                case CompareWith.GreaterThan:
                    return Expression.GreaterThan(propertyExpression, constantExpression);
                case CompareWith.LessThan:
                    return Expression.LessThan(propertyExpression, constantExpression);
                case CompareWith.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(propertyExpression, constantExpression);
                case CompareWith.LessThanOrEqual:
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
                case CompareWith.In:
                    var enumerableType = propertyExpression.Type.GetInterfaces()
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    if (enumerableType == null)
                    {
                        throw new ArgumentException("Property is not of type IEnumerable<T>.");
                    }

                    var elementType = enumerableType.GetGenericArguments()[0];

                    var anyMethod = typeof(Enumerable).GetMethods()
                            .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2)!
                            .MakeGenericMethod(elementType);

                    return Expression.Call(propertyExpression, anyMethod, constantExpression);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
