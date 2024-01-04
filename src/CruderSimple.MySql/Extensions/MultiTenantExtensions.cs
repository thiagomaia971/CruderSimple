using System.Collections;
using CruderSimple.Core.Attributes;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Attributes;

namespace CruderSimple.MySql.Extensions;

public static class MultiTenantExtensions
{
    public static void SetAllMultiTenant(this IEntity entity, Type tenantType, string multiTenantValue, List<Type> typed = null)
    {
        if (string.IsNullOrEmpty(multiTenantValue))
            return;
        if (typed is null)
            typed = new List<Type> { entity.GetType() };
        else
            typed.Add(entity.GetType());
        
        var allMultitenantProperties = entity.GetPropertiesWithAttribute<MultiTenantAttribute>();
        foreach (var multitenantProperty in allMultitenantProperties
                     .Where(c => c.PropertyType == typeof(string)))
            multitenantProperty.SetValue(entity, multiTenantValue);
        
        var properties = entity.GetType().GetProperties()
            .Where(c =>
                c.PropertyType.IsGenericType && 
                tenantType.IsAssignableFrom(c.PropertyType.GenericTypeArguments[0]) && 
                (c.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            .ToList();
            
        foreach (var multitenantProperty in properties)
        {
            var value = (multitenantProperty.GetValue(entity) as IEnumerable);
            if (value is null)
                continue;
            if (typed.Contains(value.GetType().GenericTypeArguments[0]))
                continue;
            
            foreach (var v in value)
            {
                if (v is not null)
                {
                    var entryEntity = (IEntity) v;
                    SetAllMultiTenant(entryEntity, tenantType, multiTenantValue, typed);   
                }
            }
        }
    }
}