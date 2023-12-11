using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CruderSimple.Api.Requests;

public static class GetAllRequest
{
    public record Query : IEndpointQuery;

    public class Handler<TQuery, TEntity, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query
        where TEntity : IEntity
        where IOutputDto : OutputDto 
        where IRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var queryAsync = await repository.GetAll();
            return Results.Ok(new Pagination<IOutputDto>()
            {
                Page = 0,//TODO
                Size = queryAsync.Count(),
                Data = queryAsync.ToOutput<TEntity, IOutputDto>()
            });
        }
    }
}