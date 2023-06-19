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
            var parameter = Expression.Parameter(typeof(TEntity));

            var properties = typeof(TFilter).GetProperties().Where(x =>
                x.PropertyType.IsPublic &&
                Attribute.IsDefined(x, typeof(FilterByAttribute)));

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(filter);

                if (propertyValue != null)
                {
                    var attribute = property.GetCustomAttribute<FilterByAttribute>();

                    var compareToColumn = attribute!.ColumnName ?? property.Name;
                    var comparisonType = attribute!.ComparisonType;

                    var propertyExpression = Expression.Property(parameter, compareToColumn);
                    var constantExpression = Expression.Constant(propertyValue);
                    var condition = GetComparisonExpression(propertyExpression, constantExpression, comparisonType, parameter);

                    var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
                    query = query.Where(lambda);
                }
            }

            return query;
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
