using CruderSimple.Api.Requests.Base;
using Microsoft.AspNetCore.Builder;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;

namespace CruderSimple.Api.Extensions;

public static class WebApplicationExtensions
{
    
    public static WebApplication UseCruderSimpleServices<TUser>(
        this WebApplication app)
        where TUser : IUser
    {
        app.UseRequestDefinitions<TUser>();
        app.UseCors("MyPolicy");
        return app;
    }

    private static WebApplication UseRequestDefinitions<TUser>(
        this WebApplication app)
        where TUser : IUser
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes);
        var requestHandlers = types.GetTypesWithHelpAttribute<EndpointRequest>();

        var instances = requestHandlers
            .Select(x => Activator.CreateInstance(x, 
                new object[x.GetConstructors().First().GetParameters().Length]))
            .Cast<IHttpRequestHandler>();
        foreach (var handler in instances)
            handler.AddEndpointDefinition<TUser>(app);

        return app;
    }
}