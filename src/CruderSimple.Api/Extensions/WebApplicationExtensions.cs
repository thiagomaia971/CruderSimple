using CruderSimple.Api.Requests.Base;
using Microsoft.AspNetCore.Builder;
using CruderSimple.Core.Extensions;

namespace CruderSimple.Api.Extensions;

public static class WebApplicationExtensions
{
    
    public static WebApplication UseCruderSimpleServices(
        this WebApplication app)
    {
        app.UseRequestDefinitions();
        app.UseCors("MyPolicy");
        return app;
    }

    private static WebApplication UseRequestDefinitions(
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
}