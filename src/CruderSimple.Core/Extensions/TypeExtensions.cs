﻿using CruderSimple.Core.Entities;
using System.Reflection;

namespace CruderSimple.Core.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypesWithHelpAttribute<T>(this IEnumerable<Type> types) {
            foreach(Type type in types) {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                    yield return (type);
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithInhiredType<T>(this T type)
            => type.GetType()
                .GetProperties()
                .Where(c => type.GetType().IsAssignableFrom(c.PropertyType));
        
        public static IEnumerable<PropertyInfo> GetPropertiesFromInhiredType<T>(this object value)
            => value.GetType()
                .GetProperties()
                .Where(c => typeof(T).IsAssignableFrom(c.PropertyType) ||
                            typeof(IEnumerable<>).MakeGenericType(typeof(T)).IsAssignableFrom(c.PropertyType) ||
                            typeof(ICollection<>).MakeGenericType(typeof(T)).IsAssignableFrom(c.PropertyType) ||
                            typeof(List<>).MakeGenericType(typeof(T)).IsAssignableFrom(c.PropertyType) ||
                            typeof(IList<>).MakeGenericType(typeof(T)).IsAssignableFrom(c.PropertyType));
        
        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(this object @object)
            where T : Attribute 
            => @object.GetType()
                .GetProperties()
                .Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(T)))
                .ToList();
    
        public static IEnumerable<PropertyInfo> GetPropertiesWithoutAttribute<T>(this object @object)
            where T : Attribute 
            => @object.GetType()
                .GetProperties()
                .Where(x => x.CustomAttributes.All(x => x.AttributeType != typeof(T)))
                .ToList();

        public static bool IsEnumerableType(this Type type, out Type elementType)
        {
            if (type.IsGenericType && (
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            elementType = null;
            return false;
        }

        public static bool IsSameCollectionType(this Type type, Type genericType, Type elementType)
        {
            var result = genericType.MakeGenericType(elementType).IsAssignableFrom(type);
            return result;
        }
    }
}
