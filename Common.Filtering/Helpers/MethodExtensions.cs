using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering.Helpers
{
    public static class MethodExtensions
    {
        public static MethodInfo GetStringToLower()
        {
            return typeof(string).GetMethod(nameof(string.ToLower), new Type[] { }) ?? throw new Exception($"method {nameof(string.ToLower)} not found");
        }

        public static MethodInfo GetStringToUpper()
        {
            return typeof(string).GetMethod(nameof(string.ToUpper), new Type[] { }) ?? throw new Exception($"method {nameof(string.ToLower)} not found");
        }

        public static MethodInfo GetStringContains()
        {
            return typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.Contains)} not found");
        }

        public static MethodInfo GetStringEndsWith()
        {
            return typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.EndsWith)} not found");
        }

        public static MethodInfo GetStringStartsWith()
        {
            return typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(string.StartsWith)} not found");
        }

        public static MethodInfo GetEnumerableContainsGeneric(Type type)
        {
            
            return typeof(Enumerable).GetMethods().Where(x => x.Name == "Contains").FirstOrDefault(m => m.GetParameters().Length == 2)?.MakeGenericMethod(type) ?? throw new Exception($"method {nameof(Enumerable.Contains)} not found");
        }

        public static MethodInfo GetLongParse()
        {
            return typeof(long).GetMethod(nameof(long.Parse), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(long.Parse)} not found");
        }

        public static MethodInfo GetIntParse()
        {
            return typeof(int).GetMethod(nameof(int.Parse), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(int.Parse)} not found");
        }

        public static MethodInfo GetBoolParse()
        {
            return typeof(bool).GetMethod(nameof(bool.Parse), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(bool.Parse)} not found");
        }

        public static MethodInfo GetDecimalParse()
        {
            return typeof(decimal).GetMethod(nameof(decimal.Parse), new Type[] { typeof(string) }) ?? throw new Exception($"method {nameof(decimal.Parse)} not found");
        }
    }
}
