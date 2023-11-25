using CruderSimple.Core.Entities;
using CruderSimple.Core.Filters;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Core.Requests;

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

        if (httpRequestAttribute.AuthorizeRole is not null && httpRequestAttribute.AuthorizeRole?.Length > 0)
            routeBuilder = routeBuilder.RequireAuthorization(httpRequestAttribute.AuthorizeRole);
        
        if (httpRequestAttribute.IsMultiTenant)
            routeBuilder = routeBuilder.AddEndpointFilter<MultiTenantActionFilter>();
        
        routeBuilder = ConfigureRoute(routeBuilder);
        
        return app;
    }

    public abstract Task<IResult> Handle(TQuery request, CancellationToken cancellationToken);

    public virtual RouteHandlerBuilder ConfigureRoute(RouteHandlerBuilder routeBuilder) 
        => routeBuilder;
}