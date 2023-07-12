using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace Common.Filtering
{
    public static class FilterExtensions
    {
        public static IQueryable<TEntity> FilterBy<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter filter, bool filterByRelations = false)
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

            if(filterByRelations)
            {
                query = query.FilterByRelations(filter, filterProperties, parameter);
            }

            Console.WriteLine(query);

            return query;
        }

        public static IQueryable<TEntity> FilterByRelations<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter filter, PropertyInfo[] filterProperties, ParameterExpression parameter)
        {
            var innerRelations = filterProperties.Where(x =>
                           x.PropertyType.IsPublic &&
                           x.PropertyType.IsClass &&
                           Attribute.IsDefined(x, typeof(FilterRelationAttribute)));

            Expression<Func<TEntity, bool>>? whereClause = null;

            foreach (var innerRelation in innerRelations)
            {
                var propertyValue = innerRelation.GetValue(filter);
                if (propertyValue == null) continue;

                var attribute = innerRelation.GetCustomAttribute<FilterRelationAttribute>();

                var relationNames = attribute!.RelationNames;
                var relationType = attribute!.RelationType;

                var innerProperties = innerRelation.PropertyType.GetProperties().Where(x =>
                   x.PropertyType.IsPublic &&
                                      Attribute.IsDefined(x, typeof(FilterByAttribute)));

                var lambdas = BuildRelationalLambda<TEntity>(relationNames, parameter, innerProperties, relationType, propertyValue);

                if (lambdas == null || !lambdas.Any()) continue;
                if (relationNames.Length == 1)
                {
                    whereClause = lambdas.First();
                }
                else
                {
                    whereClause = lambdas.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.OrElse(x.Body, y.Body), parameter));
                }
            }

            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }

            return query;
        }

        private static IEnumerable<Expression<Func<TEntity, bool>>> BuildRelationalLambda<TEntity>(string[] relationNames, ParameterExpression parameter, IEnumerable<PropertyInfo> innerProperties, RelationType relationType, object propertyValue)
        {
            foreach (var relationName in relationNames)
            {
                var innerParameter = Expression.Property(parameter, relationName);
                var innerParameterType = innerParameter.Type.GenericTypeArguments[0];

                var propLambdas = new List<Expression<Func<TEntity, bool>>>();

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

                    propLambdas.Add(lambda);
                }

                if (propLambdas.Any())
                {
                    yield return propLambdas.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(x.Body, y.Body), parameter));
                }
            }
        }

        private static Expression<Func<TEntity, bool>>? BuildLambda<TEntity>(PropertyInfo property, object filter, ParameterExpression parameter, bool checkForNullFirst = false)
        {
            var propertyValue = property.GetValue(filter);

            if (propertyValue == null) return null;

            var attributes = property.GetCustomAttributes<FilterByAttribute>();

            List<Expression> attributeExpressions = new List<Expression>();

            foreach (var attribute in attributes)
            {
                var compareToColumn = attribute!.ColumnName ?? property.Name;
                var comparisonType = attribute!.ComparisonType;

                var propertyExpression = Expression.Property(parameter, compareToColumn);
                var constantExpression = Expression.Constant(propertyValue);

                var condition = GetComparisonExpression(propertyExpression, constantExpression, comparisonType, parameter);
                var expression = checkForNullFirst ? Expression.AndAlso(Expression.NotEqual(propertyExpression, Expression.Constant(null)), condition) : condition;

                attributeExpressions.Add(expression);
            }

            Expression final = attributeExpressions.First();
            
            if (attributeExpressions.Count() > 1) {
                final = attributeExpressions.Aggregate((x, y) => Expression.OrElse(x, y));
            }

            return Expression.Lambda<Func<TEntity, bool>>(final, parameter);
        }

        private static Expression? BuildLambdaGeneric(PropertyInfo property, object filter, ParameterExpression parameter, Type type)
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
