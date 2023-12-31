﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Common.Filtering.Enums;

namespace Common.Filtering.Helpers
{
    public static class FilterExtensions
    {
        private readonly static ConcurrentDictionary<Type, PropertyInfo[]> _properties = new ConcurrentDictionary<Type, PropertyInfo[]>();


        public static IQueryable<TEntity> FilterBy<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter filter)
            where TEntity : class
        {
            if (filter == null) return query;

            var andList = new List<Expression<Func<TEntity, bool>>>();

            var mainParameter = Expression.Parameter(typeof(TEntity));


            var filterProperties = GetOrCacheTypeProps(typeof(TFilter));
            var properties = filterProperties.Where(x => x.PropertyType.IsPublic && Attribute.IsDefined(x, typeof(FilterByAttribute)));

            foreach (var property in properties)
            {
                var val = property.GetValue(filter);
                if (val == null) continue;

                var lambda = ExpressionExtensions.BuildLambdaFromAttributes(property, filter, mainParameter);
                if (lambda == null) continue;

                andList.Add(Expression.Lambda<Func<TEntity, bool>>(lambda, mainParameter));
            }

            var clause = andList.Aggregate((x, y) => Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(x.Body, y.Body), mainParameter));
            query = query.Where(clause);

            return query;
        }

        private static PropertyInfo[] GetOrCacheTypeProps(Type type)
        {
            if (_properties.ContainsKey(type))
            {
                return _properties[type];
            }

            var properties = type.GetProperties();

            _properties.TryAdd(type, properties);

            return properties;
        }
    }
}
