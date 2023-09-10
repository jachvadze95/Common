using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    internal static class TransfermerExtensions
    {
        public static object? TransformString(object value, StringTransformer transformer)
        {
            if (value == null)
                return null;

            switch (transformer)
            {
                case StringTransformer.UpperCase:
                    return (value as string).ToUpper();
                case StringTransformer.LowerCase:
                    return (value as string).ToLower();
                case StringTransformer.Boolean:
                    return Convert.ChangeType(value, typeof(bool), CultureInfo.InvariantCulture);
                case StringTransformer.Int32:
                    return Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture);
                case StringTransformer.Int64:
                    return Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture);
                case StringTransformer.Decimal:
                    return Convert.ChangeType(value, typeof(decimal), CultureInfo.InvariantCulture);
                default: return value;
            }
        }
    }
}
