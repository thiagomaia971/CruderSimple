using CruderSimple.Core.Requests;
using CruderSimple.Core.ViewModels;
using CruderSimple.DynamoDb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CruderSimple.DynamoDb.Requests;

public static class GetByIdRequest
{
    public record Query(string id) : IEndpointQuery;

    public class Handler<TQuery, TEntity, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query
        where TEntity : Entity
        where IOutputDto : OutputDto 
        where IRepository : Interfaces.IRepository<TEntity>
    {
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var single = await repository.FindById(request.id);
            if (single is null)
                return Results.NotFound();
            return Results.Ok(single.ToOutput());
        }
    }
}