using System.Text.Json;
using CruderSimple.Api.Filters;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests.Base;

public abstract class HttpHandlerBase<TQuery, TEntity, TResult> : IHttpRequestHandler
    where TQuery : Core.EndpointQueries.IEndpointQuery
    where TEntity : IEntity
{
    public WebApplication AddEndpointDefinition<TUser>(WebApplication app)
        where TUser : IUser
    {
        var httpRequestAttribute = (EndpointRequest?)
            GetType().GetCustomAttributes(true).FirstOrDefault(x =>
                typeof(EndpointRequest).IsAssignableFrom(x.GetType()));

        var routeBuilder = app.MapMethods(
            $"{httpRequestAttribute.Version}/{httpRequestAttribute.Endpoint.ToLower()}",
            new string[] { httpRequestAttribute.EndpointMethod.ToString() },
            async ([FromServices] IMediator mediator, [AsParameters] TQuery query)
                => await SendRequest(mediator, query))
            .WithTags(typeof(TEntity).Name)
            .WithOpenApi();

        if (httpRequestAttribute.RequireAuthorization)
        {
            var policies = new List<string>(httpRequestAttribute.Roles ?? Array.Empty<string>())
            /*{
                "CanRead",
                "CanWrite"
            }*/;
            routeBuilder = routeBuilder.RequireAuthorization(policies.ToArray());
            routeBuilder = routeBuilder.AddEndpointFilter<PermissionAuthorizationActionFilter<TUser>>();
        }
        
        if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
            routeBuilder = routeBuilder.AddEndpointFilter<MultiTenantActionFilter>();
        
        routeBuilder = ConfigureRoute(routeBuilder);
        
        return app;
    }

    private async Task<IResult> SendRequest(IMediator mediator, TQuery query)
    {
        Result result = await mediator.Send(query);
        switch (result.HttpStatusCode)
        {
            case 200:
                return Results.Ok(result);
            case 201:
                return Results.Created(string.Empty, result);
            case 400:
                return Results.BadRequest(result);
            case 404:
                return Results.NotFound(result);
            case 500:
            default:
                return Results.Json(result, JsonSerializerOptions.Default, null, 500);
        }
    }

    public abstract Task<TResult> Handle(TQuery request, CancellationToken cancellationToken);

    public virtual RouteHandlerBuilder ConfigureRoute(RouteHandlerBuilder routeBuilder) 
        => routeBuilder;
}