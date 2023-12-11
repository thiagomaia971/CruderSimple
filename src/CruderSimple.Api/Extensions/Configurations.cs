using System.Diagnostics;
using CruderSimple.Api.Requests.Base;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Api.Extensions;

public static class Configurations
{
    public static IServiceCollection AddCruderRequestDefinitions(
        this IServiceCollection services)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes);
        var requestHandlers = types.GetTypesWithHelpAttribute<EndpointRequest>();

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
        this WebApplication app)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes);
        var requestHandlers = types.GetTypesWithHelpAttribute<EndpointRequest>();

        var instances = requestHandlers
            .Select(x => Activator.CreateInstance(x, 
                new object[x.GetConstructors().First().GetParameters().Length]))
            .Cast<IHttpRequestHandler>();
        foreach (var handler in instances)
            handler.AddEndpointDefinition(app);
        return app;
    }
    
    public static IEnumerable<Type> GetTypesWithHelpAttribute<T>(this IEnumerable<Type> types) {
        foreach(Type type in types) {
            if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                yield return (type);
            }
        }
    }
    
    public static WebApplication UseCruderSimpleServices(
        this WebApplication app)
    {
        app.UseRequestDefinitions();
        
        return app;
    }
}