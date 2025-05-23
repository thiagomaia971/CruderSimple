﻿using System.Collections;
using System.Data.Entity;
using System.Linq.Expressions;
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

    public static void FilterSoftDelete<T>(this ModelBuilder ModelBuilder)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType.GenericTypeArguments.Any() &&  typeof(IEntity).IsAssignableFrom(x.PropertyType.GenericTypeArguments[0]));
        
        foreach (var dbSetProperty in properties)
            SetSoftDeleteQueryDbSet(ModelBuilder, dbSetProperty);
    }

    public static void DetachLocal(this DbContext context, IEntity t)
    {
        try
        {
            var local = GetLocalDbSet(context, t.GetType(), t.Id);

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
    
    public static IQueryable<IEntity> GetDbSetByType(this DbContext context, Type entityType)
    {
        // Obter a propriedade DbSet dinamicamente pelo Type
        var dbSetProperty = context.GetType()
            .GetProperties()
            .FirstOrDefault(p => p.PropertyType.GenericTypeArguments.Contains(entityType));

        if (dbSetProperty == null)
        {
            throw new Exception($"DbSet para a entidade {entityType.Name} não encontrado no contexto.");
        }

        // Retornar o DbSet como IQueryable
        return dbSetProperty.GetValue(context) as IQueryable<IEntity>;
    }

    public static dynamic GetDbSet(this DbContext context, Type entityType)
    {
        var methodInfo = context.GetType().GetMethod("Set", new Type[0] {  });
        var methodGenericInfo =
            methodInfo.MakeGenericMethod(entityType);

        var dbSet = (dynamic) methodGenericInfo.Invoke(context, new string[0] {  });
        return dbSet;
    }

    public static IEnumerable<IEntity> GetLocalDbSet(this DbContext context, Type entityType)
    {
        var dbSet = context.GetDbSet(entityType);
        var local = (IEnumerable<IEntity>) dbSet.Local;
        return local;
    }
    
    private static IEntity GetLocalDbSet(DbContext context, Type entityType, string entryId)
    {
        var local = context.GetLocalDbSet(entityType);
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
    
    private static void SetSoftDeleteQueryDbSet(ModelBuilder modelBuilder, PropertyInfo dbSetProperty)
    {
        var entityType = dbSetProperty.PropertyType;
        if (!typeof(IEntity).IsAssignableFrom(dbSetProperty.PropertyType))
            entityType = dbSetProperty.PropertyType.GenericTypeArguments[0];
        
        var entity = modelBuilder.Entity(entityType);
        var parameter = Expression.Parameter(entityType, $"{entityType.Name.Substring(0, 2)}");
        
        var property = Expression.Property(Expression.Property(parameter, "DeletedAt"), "HasValue");
        var body = Expression.Not(property);
        var lambda = Expression.Lambda(body, parameter);
        entity.HasQueryFilter(lambda);
    }
}