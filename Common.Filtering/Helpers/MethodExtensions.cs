using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering.Helpers
{
    public static class MethodExtensions
    {
        private readonly static ConcurrentDictionary<string, MethodInfo> _methods = new ConcurrentDictionary<string, MethodInfo>();

        public static MethodInfo GetStringToLower()
        {
            var name = nameof(string.ToLower);
            var type = typeof(string);

            return GetOrCacheMethod(name, type);
        }

        public static MethodInfo GetStringToUpper()
        {
            var name = nameof(string.ToUpper);
            var type = typeof(string);

            return GetOrCacheMethod(name, type);
        }

        public static MethodInfo GetStringContains()
        {
            var name = nameof(string.Contains);
            var type = typeof(string);

            return GetOrCacheMethod(name, type);
        }

        public static MethodInfo GetStringEndsWith()
        {
            var name = nameof(string.EndsWith);
            var type = typeof(string);

            return GetOrCacheMethod(name, type);
        }

        public static MethodInfo GetStringStartsWith()
        {
            var name = nameof(string.StartsWith);
            var type = typeof(string);

            return GetOrCacheMethod(name, type);
        }

        public static MethodInfo GetEnumerableContainsGeneric(Type itemType)
        {
            var name = nameof(Enumerable.Contains);
            var methodOnType = typeof(Enumerable);
            return GetOrCacheMethod(name, methodOnType, () => methodOnType.GetMethods().Where(x => x.Name == name).SingleOrDefault(x => x.GetParameters().Length == 2)?.MakeGenericMethod(itemType));
        }

        public static MethodInfo GetLongParse()
        {
            var name = nameof(long.Parse);
            var type = typeof(long);
            return GetOrCacheMethod(name, type, typeof(string));
        }

        public static MethodInfo GetIntParse()
        {
            var name = nameof(int.Parse);
            var type = typeof(int);
            return GetOrCacheMethod(name, type, typeof(string));
        }

        public static MethodInfo GetBoolParse()
        {
            var name = nameof(bool.Parse);
            var type = typeof(bool);
            return GetOrCacheMethod(name, type, typeof(string));
        }

        public static MethodInfo GetDecimalParse()
        {
            var name = nameof(decimal.Parse);
            var type = typeof(decimal);
            return GetOrCacheMethod(name, type, typeof(string));
        }


        //Private methods
        private static MethodInfo GetOrCacheMethod(string name, Type type, Type? paramType = null)
        {
            paramType ??= type;

            return GetOrCacheMethod(name, type, () => type.GetMethod(name, new Type[] { paramType }));
        }

        private static MethodInfo GetOrCacheMethod(string name, Type type, Func<MethodInfo?> getMethod)
        {
            name = type.Name + name;

            if (_methods.ContainsKey(name))
            {
                return _methods[name];
            }

            var method = getMethod();

            if (method == null) throw new Exception($"method {name} not found");

            _methods.TryAdd(name, method);
            return method;
        }
    }
}
