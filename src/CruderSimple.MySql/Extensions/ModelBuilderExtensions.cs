using System.Collections;
using System.Data.Entity;
using System.Reflection;
using CruderSimple.Core.Entities;
using CruderSimple.MySql.Attributes;
using CruderSimple.MySql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

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

    public static void DetachLocal(this DbContext context, IEntity t, string entryId)
    {
        try
        {
            var local = GetLocalDbSet(context, t.GetType(), entryId);

            if (local != null)
                context.Entry(local).State = EntityState.Detached;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            context.Entry(t).State = EntityState.Modified;
        }
    }
    
    private static IEntity GetLocalDbSet(DbContext context, Type entityType, string entryId)
    {
        var methodInfo = context.GetType().GetMethod("Set", new Type[0] {  });
        var methodGenericInfo =
            methodInfo.MakeGenericMethod(entityType);

        var dbSet = (dynamic) methodGenericInfo.Invoke(context, new string[0] {  });
        var local = (IEnumerable<IEntity>) dbSet.Local;
        var cc = local.FirstOrDefault(c => c.Id.Equals(entryId));
        return cc;
        // return dbSet.Local.FirstOrDefault(entry => entry.Id.Equals(entryId));
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