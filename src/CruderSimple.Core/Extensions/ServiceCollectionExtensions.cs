using System.Reflection;
using CruderSimple.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CruderSimple.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories<TEntity>(
        this IServiceCollection services,
        Type repositoryInterfaceType,
        Type repositoryImplementationType)
    where TEntity : IEntity
    {
        var types = GetTypes();
        var entityTypes = GetByType<TEntity>();

        foreach (var entityType in entityTypes)
        {
            var genericInterface = repositoryInterfaceType.MakeGenericType(entityType);
            var genericImplementation = repositoryImplementationType.MakeGenericType(entityType);

            var @interface = types.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
            if (@interface is null)
            {
                services.AddScoped(genericInterface, genericImplementation);
            }
            else
            {
                var implementation = types.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                services.TryAddScoped(@interface, implementation);
                services.AddScoped(genericInterface, implementation);
            }
        }

        return services;
    }

    public static IEnumerable<Type> GetTypes()
    { 
        return Assembly
                .GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(x => x.DefinedTypes)
                .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes))
                .Distinct()
                .ToList();
    }

    public static IEnumerable<Type> GetByType<T>()
    {
        return GetTypes()
            .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
    }
}