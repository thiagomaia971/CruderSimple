using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using MediatR;

namespace CruderSimple.Api.Requests;

public static class GetBaseRequest
{
    public abstract class Handler<TQuery, TEntity, TDto, TRepository>(TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result>
        where TQuery : GetBaseEndpointQuery
        where TEntity : IEntity
        where TDto : BaseDto
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
    }
}