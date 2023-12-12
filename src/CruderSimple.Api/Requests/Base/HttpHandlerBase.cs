using CruderSimple.Api.Filters;
using CruderSimple.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests.Base;

public abstract class HttpHandlerBase<TQuery, TEntity> : IHttpRequestHandler
    where TQuery : IEndpointQuery
    where TEntity : IEntity
{
    public WebApplication AddEndpointDefinition(WebApplication app)
    {
        var httpRequestAttribute = (EndpointRequest?)
            GetType().GetCustomAttributes(true).FirstOrDefault(x =>
                typeof(EndpointRequest).IsAssignableFrom(x.GetType()));

        var routeBuilder = app.MapMethods(
            $"{httpRequestAttribute.Version}/{httpRequestAttribute.Endpoint}",
            new string[] { httpRequestAttribute.EndpointMethod.ToString() },
            async ([FromServices] IMediator mediator, [AsParameters] TQuery query)
                => await mediator.Send(query))
            .WithTags(typeof(TEntity).Name)
            .WithOpenApi();

        if (httpRequestAttribute.RequireAuthorization)
            routeBuilder = routeBuilder.RequireAuthorization(httpRequestAttribute.Roles ?? Array.Empty<string>());
        
        if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
            routeBuilder = routeBuilder.AddEndpointFilter<MultiTenantActionFilter>();
        
        routeBuilder = ConfigureRoute(routeBuilder);
        
        return app;
    }

    public abstract Task<IResult> Handle(TQuery request, CancellationToken cancellationToken);

    public virtual RouteHandlerBuilder ConfigureRoute(RouteHandlerBuilder routeBuilder) 
        => routeBuilder;
}