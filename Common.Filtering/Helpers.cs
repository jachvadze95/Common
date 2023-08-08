using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    internal static class Helpers
    {
        public static bool IsNullableExpression(Expression expression)
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

        public static object? TransformString(object value, StringTransformer transformer)
        {
            if(value == null)
                return null;

            switch (transformer)
            {
                case StringTransformer.UpperCase:
                    return (value as string).ToUpper();
                case StringTransformer.LowerCase:
                    return (value as string).ToLower();
                case StringTransformer.Boolean:
                    return Convert.ChangeType(value, typeof(bool), CultureInfo.InvariantCulture);
                case StringTransformer.Inetger:
                    return Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture);
                case StringTransformer.Decimal:
                    return Convert.ChangeType(value, typeof(decimal), CultureInfo.InvariantCulture);
                default: return value;
            }
        }
    }
}
