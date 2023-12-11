using System.Reflection;
using CruderSimple.Core.Entities;
using CruderSimple.MySql.Attributes;
using CruderSimple.MySql.Entities;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Extensions;

public static class ModelBuilderExtensions
{
    public static void AutoInclude<T>(this ModelBuilder ModelBuilder)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType.GenericTypeArguments.Any() &&  typeof(IEntity).IsAssignableFrom(x.PropertyType.GenericTypeArguments[0]));
        foreach (var dbSetProperty in properties)
            AutoIncludeDbSet(ModelBuilder, dbSetProperty);
    }

    private static void AutoIncludeDbSet(ModelBuilder modelBuilder, PropertyInfo dbSetProperty)
    {
        var entityType = dbSetProperty.PropertyType;
        if (!typeof(IEntity).IsAssignableFrom(dbSetProperty.PropertyType))
            entityType = dbSetProperty.PropertyType.GenericTypeArguments[0];
        
        var entity = modelBuilder.Entity(entityType);
        var propertiesToInclude = entityType
            .GetProperties()
            .Where(y => y.GetCustomAttribute<IncludeAttribute>() != null);

        foreach (var propertyToInclude in propertiesToInclude)
            entity.Navigation(propertyToInclude.Name).AutoInclude();
    }
}