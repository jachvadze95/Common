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
            return typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == nameof(Enumerable.Contains))?.MakeGenericMethod(type) ?? throw new Exception($"method {nameof(Enumerable.Contains)} not found");
        }
    }
}
