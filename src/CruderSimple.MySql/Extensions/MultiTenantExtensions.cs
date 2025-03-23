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
        else if (typed.Contains(entity.GetType()))
            return;
        else typed.Add(entity.GetType());
        
        var allMultitenantProperties = entity.GetPropertiesWithAttribute<MultiTenantAttribute>();
        foreach (var multitenantProperty in allMultitenantProperties
                     .Where(c => c.PropertyType == typeof(string)))
        {
            var currentValue = multitenantProperty.GetValue(entity) as string;
            if (string.IsNullOrEmpty(currentValue))
                multitenantProperty.SetValue(entity, multiTenantValue);
            else
                multiTenantValue = currentValue;
        }
        
        var properties = entity.GetType().GetProperties()
            .Where(c =>
                typeof(IEntity).IsAssignableFrom(c.PropertyType) ||
                (c.PropertyType.IsGenericType && 
                typeof(IEntity).IsAssignableFrom(c.PropertyType.GenericTypeArguments[0]) && 
                (c.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))))
            .ToList();
            
        foreach (var multitenantProperty in properties)
        {
            if (multitenantProperty.GetValue(entity) is IEnumerable valueList)
            {
                if (valueList is null)
                    continue;
                if (typed.Contains(valueList.GetType().GenericTypeArguments[0]))
                    continue;
            
                foreach (var v in valueList)
                {
                    if (v is not null)
                    {
                        var entryEntity = (IEntity) v;
                        SetAllMultiTenant(entryEntity, tenantType, multiTenantValue, typed);   
                    }
                }   
            }
            else if (multitenantProperty.GetValue(entity) is IEntity valueEntity)
            {
                if (valueEntity is null)
                    continue;
                SetAllMultiTenant(valueEntity, tenantType, multiTenantValue, typed);   
 
            }
        }
    }
    public static void SetAllUserId(this IEntity entity, string userIdValue, List<Type> typed = null)
    {
        if (string.IsNullOrEmpty(userIdValue))
            return;
        if (typed is null)
            typed = new List<Type> { entity.GetType() };
        else if (typed.Contains(entity.GetType()))
            return;
        else typed.Add(entity.GetType());
        
        var userIdProperty = entity.GetPropertyByPropertyName("UserId");
        if (userIdProperty is not null)
        {
            var currentValue = userIdProperty.GetValue(entity) as string;
            if (string.IsNullOrEmpty(currentValue))
                userIdProperty.SetValue(entity, userIdValue);
            else
                userIdValue = currentValue;   
        }
        
        var properties = entity.GetType().GetProperties()
            .Where(c =>
                typeof(IEntity).IsAssignableFrom(c.PropertyType) ||
                (c.PropertyType.IsGenericType && 
                typeof(IEntity).IsAssignableFrom(c.PropertyType.GenericTypeArguments[0]) && 
                (c.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                 c.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))))
            .ToList();
            
        foreach (var multitenantProperty in properties)
        {
            if (multitenantProperty.GetValue(entity) is IEnumerable valueList)
            {
                if (valueList is null)
                    continue;
                if (typed.Contains(valueList.GetType().GenericTypeArguments[0]))
                    continue;
            
                foreach (var v in valueList)
                {
                    if (v is not null)
                    {
                        var entryEntity = (IEntity) v;
                        SetAllUserId(entryEntity, userIdValue, typed);   
                    }
                }   
            }
            else if (multitenantProperty.GetValue(entity) is IEntity valueEntity)
            {
                if (valueEntity is null)
                    continue;
                SetAllUserId(valueEntity, userIdValue, typed);   
 
            }
        }
    }
}