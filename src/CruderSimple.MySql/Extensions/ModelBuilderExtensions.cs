using System.Reflection;
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
            .Where(x => x.PropertyType.GenericTypeArguments.Any() &&  typeof(Entity).IsAssignableFrom(x.PropertyType.GenericTypeArguments[0]));
        foreach (var dbSets in properties)
            AutoIncludeDbSet(ModelBuilder, dbSets);
    }

    private static void AutoIncludeDbSet(ModelBuilder ModelBuilder, PropertyInfo dbSets)
    {
        var entityType = dbSets.PropertyType.GenericTypeArguments[0];
        var entity = ModelBuilder.Entity(entityType);
        var propertiesToInclude = entityType
            .GetProperties()
            .Where(y => y.GetCustomAttribute<IncludeAttribute>() != null);

        foreach (var propertyToInclude in propertiesToInclude)
            entity.Navigation(propertyToInclude.Name).AutoInclude();
    }
}