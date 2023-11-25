using System.Diagnostics;
using System.Reflection;
using CruderSimple.Core.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Core.Extensions;

public static class EndpointDefinitionsExtensions
{
    public static Assembly GlobalAssembly;
    public static IServiceCollection AddRequestDefinitions(
        this IServiceCollection services,
        params Type[] entityScanMarkers)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        GlobalAssembly = entityScanMarkers.First().Assembly;
        var requestHandlers = entityScanMarkers.First().Assembly.GetTypesWithHelpAttribute<EndpointRequest>();

        foreach (var handler in requestHandlers)
        {
            var query = handler.BaseType.GetGenericArguments().First();
            var @interface = typeof(IRequestHandler<,>).MakeGenericType(query, typeof(IResult)); 
            services.AddScoped(@interface, handler);
        }
        
        stopWatch.Stop();
        Console.WriteLine($"AddRequestDefinitions in: {stopWatch.ElapsedMilliseconds}ms");
        return services;
    }
    
    public static WebApplication UseRequestDefinitions(
        this WebApplication app,
        params Type[] entityScanMarkers)
    {
        var requestHandlers = entityScanMarkers.First().Assembly.GetTypesWithHelpAttribute<EndpointRequest>();

        var instances = requestHandlers
            .Select(x => Activator.CreateInstance(x, 
                new object[x.GetConstructors().First().GetParameters().Length]))
            .Cast<IHttpRequestHandler>();
        foreach (var handler in instances)
            handler.AddEndpointDefinition(app);
        
        return app;
    }
    
    public static IEnumerable<Type> GetTypesWithHelpAttribute<T>(this Assembly assembly) {
        foreach(Type type in assembly.GetTypes()) {
            if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                yield return (type);
            }
        }
    }
}